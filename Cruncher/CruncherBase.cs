﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CruncherBase.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Defines the CruncherBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher
{
    #region Using
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Text;
    using System.Text.RegularExpressions;

    using Cruncher.Caching;
    using Cruncher.Extensions;
    using Cruncher.Preprocessors;
    using Cruncher.Web;
    #endregion

    /// <summary>
    /// The cruncher base.
    /// </summary>
    public abstract class CruncherBase
    {
        #region Fields
        /// <summary>
        /// The remote regex.
        /// </summary>
        private static readonly Regex RemoteRegex = new Regex(@"^http(s?)://", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CruncherBase"/> class.
        /// </summary>
        /// <param name="options">The options containing instructions for the cruncher.</param>
        protected CruncherBase(CruncherOptions options)
        {
            this.Options = options;
            this.FileMonitors = new ConcurrentBag<string>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the options containing instructions for the cruncher.
        /// </summary>
        public CruncherOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the file monitors.
        /// </summary>
        public ConcurrentBag<string> FileMonitors { get; set; }
        #endregion

        #region Methods
        #region Public
        /// <summary>
        /// Crunches the specified resource.
        /// </summary>
        /// <param name="resource">The file or folder containing the resource(s) to crunch.</param>
        /// <returns>The minified resource.</returns>
        public string Crunch(string resource)
        {
            string contents = string.Empty;

            // It is necessary to empty the static FileMonitors collection in order to avoid adding old FileMonitors to the current resource
            this.FileMonitors = new ConcurrentBag<string>();

            if (this.Options.CacheFiles)
            {
                contents = (string)CacheManager.GetItem(resource.ToMd5Fingerprint());
            }

            if (string.IsNullOrWhiteSpace(contents))
            {
                StringBuilder stringBuilder = new StringBuilder();

                if (this.IsRemoteFile(resource))
                {
                    stringBuilder.Append(this.LoadRemoteFile(resource));
                }
                else if (this.IsValidPath(resource))
                {
                    stringBuilder.Append(this.LoadLocalFolder(resource));
                }
                else
                {
                    stringBuilder.Append(this.LoadLocalFile(resource));
                }

                contents = stringBuilder.ToString();

                // Cache if applicable.
                this.AddItemToCache(resource, contents);
            }

            return contents;
        }

        /// <summary>
        /// Adds a cached file monitor to the list.
        /// </summary>
        /// <param name="file">
        /// The file to add to the monitors list.
        /// </param>
        /// <param name="contents">
        /// The contents of the file.
        /// </param>
        public void AddFileMonitor(string file, string contents)
        {
            // Cache if applicable.
            if (this.Options.CacheFiles && !string.IsNullOrWhiteSpace(contents))
            {
                this.FileMonitors.Add(file);
            }
        }

        /// <summary>
        /// Minifies the specified resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>The minified resource.</returns>
        public abstract string Minify(string resource);
        #endregion

        #region Protected
        /// <summary>
        /// Loads the local file.
        /// </summary>
        /// <param name="file">The file to load.</param>
        /// <returns>The contents of the local file as a string.</returns>
        protected virtual string LoadLocalFile(string file)
        {
            string contents = string.Empty;

            if (this.IsValidFile(file))
            {
                using (StreamReader streamReader = new StreamReader(file))
                {
                    contents = streamReader.ReadToEnd();
                }
            }

            return contents;
        }

        /// <summary>
        /// Transforms the content of the given string using the correct PreProcessor. 
        /// </summary>
        /// <param name="input">The input string to transform.</param>
        /// <param name="path">The path to the file.</param>
        /// <returns>The transformed string.</returns>
        protected virtual string PreProcessInput(string input, string path)
        {
            string extension = path.Substring(path.LastIndexOf('.')).ToUpperInvariant();

            input = PreprocessorManager.Instance.PreProcessors
                .Where(p => p.AllowedExtensions != null && p.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                .Aggregate(input, (current, p) => p.Transform(current, path, this));

            return input;
        }

        /// <summary>
        /// Adds a resource to the cache.
        /// </summary>
        /// <param name="filename">
        /// The filename of the item to add.
        /// </param>
        /// <param name="contents">
        /// The contents of the file to cache.
        /// </param>
        protected void AddItemToCache(string filename, string contents)
        {
            // The filename could be only one file or multiple files. In this latest case it is necessary to retrieve previously created FileMonitors and 
            // add them to the current FileMonitors collection in order to remove the cache item if any of the files changes
            // In first place retrieves the key for each file from the resource in order to be able to retrieve the FileMonitors 
            ConcurrentBag<string> resourceFilesKeys = (ConcurrentBag<string>)CacheManager.GetItem(filename.ToMd5Fingerprint() + "_FILE_MONITOR_KEYS");
            if (resourceFilesKeys != null && !resourceFilesKeys.IsEmpty)
            {
                // It is necessary to empty the static FileMonitors collection in order to avoid adding old FileMonitors 
                this.FileMonitors = new ConcurrentBag<string>();

                foreach (string fileKey in resourceFilesKeys)
                {
                    // Retrieve the FileMonitors for every file and add them to the current FileMonitor collection
                    ConcurrentBag<string> fileMonitors = (ConcurrentBag<string>)CacheManager.GetItem(fileKey + "_FILE_MONITORS");
                    if (fileMonitors != null && !fileMonitors.IsEmpty)
                    {
                        foreach (string fileMonitor in fileMonitors.ToList())
                        {
                            this.FileMonitors.Add(fileMonitor);
                        }
                    }
                }
            }

            if (this.Options.Minify && !string.IsNullOrWhiteSpace(contents))
            {
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
                int days = this.Options.CacheLength;
                cacheItemPolicy.AbsoluteExpiration = DateTime.UtcNow.AddDays(days != 0 ? days : -1);
                cacheItemPolicy.Priority = CacheItemPriority.NotRemovable;

                string key = filename.ToMd5Fingerprint();

                if (this.FileMonitors.Any())
                {
                    CacheManager.AddItem(key + "_FILE_MONITORS", this.FileMonitors, cacheItemPolicy);
                    cacheItemPolicy.ChangeMonitors.Add(new HostFileChangeMonitor(this.FileMonitors.ToList()));
                }

                CacheManager.AddItem(filename.ToMd5Fingerprint(), contents, cacheItemPolicy);
            }
        }

        #endregion

        #region Private
        /// <summary>
        /// Loads the local folder.
        /// </summary>
        /// <param name="folder">The folder to load resources from.</param>
        /// <returns>The contents of the resources in the folder as a string.</returns>
        private string LoadLocalFolder(string folder)
        {
            StringBuilder stringBuilder = new StringBuilder();
            DirectoryInfo directoryInfo = new DirectoryInfo(folder);

            foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                stringBuilder.Append(this.LoadLocalFile(fileInfo.FullName));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Loads the remote file.
        /// </summary>
        /// <param name="url">The url to the resource.</param>
        /// <returns>The contents of the remote file as a string.</returns>
        private string LoadRemoteFile(string url)
        {
            string contents = string.Empty;

            if (this.Options.AllowRemoteFiles)
            {
                RemoteFile remoteFile = new RemoteFile(new Uri(url))
                {
                    MaxDownloadSize = this.Options.RemoteFileMaxBytes,
                    TimeoutLength = this.Options.RemoteFileTimeout
                };

                // Return the preprocessed css.
                contents = this.PreProcessInput(remoteFile.GetFileAsString(), url);
            }

            return contents;
        }

        /// <summary>
        /// Determines whether the current resource is a valid file.
        /// </summary>
        /// <param name="resource">The file or folder containing the resource(s) to check.</param>
        /// <returns>
        ///   <c>true</c> if the current resource is a valid file; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidFile(string resource)
        {
            return PreprocessorManager.Instance.AllowedExtensionsRegex.IsMatch(resource) && File.Exists(resource);
        }

        /// <summary>
        /// Determines whether the current resource is a valid path.
        /// </summary>
        /// <param name="resource">The file or folder containing the resource(s) to check.</param>
        /// <returns>
        ///   <c>true</c> if the current resource is a valid path; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidPath(string resource)
        {
            return resource.Contains("\\") && Directory.Exists(resource);
        }

        /// <summary>
        /// Determines whether the current resource is a remote file.
        /// </summary>
        /// <param name="resource">The file or folder containing the resource(s) to check.</param>
        /// <returns>
        ///   <c>true</c> if the current resource is a remote file; otherwise, <c>false</c>.
        /// </returns>
        private bool IsRemoteFile(string resource)
        {
            return RemoteRegex.IsMatch(resource);
        }
        #endregion
        #endregion
    }
}
