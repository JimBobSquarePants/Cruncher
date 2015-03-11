// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CruncherBase.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The cruncher base. Inherit from this to implement your own cruncher. 
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Cruncher.Preprocessors;

    /// <summary>
    /// The cruncher base. Inherit from this to implement your own cruncher. 
    /// </summary>
    public abstract class CruncherBase
    {
        /// <summary>
        /// The remote regex.
        /// </summary>
        private static readonly Regex RemoteRegex = new Regex(@"^http(s?)://", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="CruncherBase"/> class.
        /// </summary>
        /// <param name="options">The options containing instructions for the cruncher.</param>
        protected CruncherBase(CruncherOptions options)
        {
            this.Options = options;
            this.FileMonitors = new ConcurrentBag<string>();
        }

        /// <summary>
        /// Gets or sets the options containing instructions for the cruncher.
        /// </summary>
        public CruncherOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the file monitors.
        /// </summary>
        public ConcurrentBag<string> FileMonitors { get; set; }

        #region Methods
        #region Public
        /// <summary>
        /// Crunches the specified resource.
        /// </summary>
        /// <param name="resource">The file or folder containing the resource(s) to crunch.</param>
        /// <returns>The minified resource.</returns>
        public async Task<string> CrunchAsync(string resource)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (this.IsRemoteFile(resource))
            {
                stringBuilder.Append(await this.LoadRemoteFileAsync(resource));
            }
            else if (this.IsValidPath(resource))
            {
                stringBuilder.Append(this.LoadLocalFolder(resource));
            }
            else
            {
                stringBuilder.Append(this.LoadLocalFile(resource));
            }

            return stringBuilder.ToString();
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
        private async Task<string> LoadRemoteFileAsync(string url)
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
                contents = this.PreProcessInput(await remoteFile.GetFileAsStringAsync(), url);
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
