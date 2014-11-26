// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceHelper.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides a series of helper methods for dealing with resources.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Hosting;

    using Cruncher.Caching;
    using Cruncher.Web.Configuration;

    /// <summary>
    /// Provides a series of helper methods for dealing with resources.
    /// </summary>
    public class ResourceHelper
    {
        /// <summary>
        /// The physical file regex.
        /// </summary>
        private static readonly Regex PhysicalFileRegex = new Regex(@"^(-)?[0-9]+\.(css|js)$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Get's a value indicating whether the resource is a filename only.
        /// </summary>
        /// <param name="resource">
        /// The resource to test against.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/> indicating whether the resource is a filename only.
        /// </returns>
        public static bool IsResourceFilenameOnly(string resource)
        {
            return Path.GetFileName(resource) == resource.Trim();
        }

        /// <summary>
        /// Returns the file path to the specified resource.
        /// </summary>
        /// <param name="resource">
        /// The resource to return the path for.
        /// </param>
        /// <param name="rootPath">
        /// The root path for the application.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> representing the file path to the resource.
        /// </returns>
        public static string GetFilePath(string resource, string rootPath)
        {
            try
            {
                // Check whether this method is invoked in an http request or not
                if (HttpContext.Current != null)
                {
                    // Check whether it is a correct uri
                    if (Uri.IsWellFormedUriString(resource, UriKind.RelativeOrAbsolute))
                    {
                        // If the uri contains a scheme delimiter (://) then try to see if the authority is the same as the current request
                        if (resource.Contains(Uri.SchemeDelimiter))
                        {
                            string requestAuthority = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                            if (resource.Trim().StartsWith(requestAuthority, StringComparison.CurrentCultureIgnoreCase))
                            {
                                string path = resource.Substring(requestAuthority.Length);
                                return HostingEnvironment.MapPath(string.Format("~{0}", path));
                            }

                            return resource;
                        }

                        // If it is a relative path then combines the request's path with the resource's path
                        if (!Path.IsPathRooted(resource))
                        {
                            return Path.GetFullPath(Path.Combine(rootPath, resource));
                        }

                        // It is an absolute path
                        return HostingEnvironment.MapPath(string.Format("~{0}", resource));
                    }
                }

                // In any other case use the default Path.GetFullPath() method 
                return Path.GetFullPath(resource);
            }
            catch (Exception)
            {
                // If there is an error then the method returns the original resource path since the method doesn't know how to process it
                return resource;
            }
        }

        /// <summary>
        /// The create resource physical file.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="fileContent">
        /// The file content.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string CreateResourcePhysicalFile(string fileName, string fileContent)
        {
            // Cache item to ensure that checking file's creation date is performed only every xx hours
            string cacheIdCheckCreationDate = string.Format("_CruncherCheckFileCreationDate_{0}", fileName);
            const int CheckCreationDateFrequencyHours = 6;

            // Cache item to ensure that checking whether the file exists is performed only every xx hours
            string cacheIdCheckFileExists = string.Format("_CruncherCheckFileExists_{0}", fileName);

            string fileVirtualPath = VirtualPathUtility.AppendTrailingSlash(CruncherConfiguration.Instance.PhysicalFilesPath) + fileName;
            string filePath = HostingEnvironment.MapPath(fileVirtualPath);

            // Trims the physical files folder ensuring that it does not contains files older than xx days 
            // This is performed before creating the physical resource file
            TrimPhysicalFilesFolder(HostingEnvironment.MapPath(CruncherConfiguration.Instance.PhysicalFilesPath));

            // Check whether the resource file already exists
            if (filePath != null)
            {
                // In order to avoid checking whether the file exists for every request (for a very busy site could be be thousands of requests per minute)
                // a new cache item is added that will expire in a minute is created. That means that if the file is deleted (what should never happen) then it will be recreated after one minute.
                // With this improvement IO operations are reduced to one per minute for already existing files
                if (CacheManager.GetItem(cacheIdCheckFileExists) == null)
                {
                    CacheItemPolicy policycacheIdCheckFileExists = new CacheItemPolicy
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(1),
                        Priority = CacheItemPriority.NotRemovable
                    };
                    CacheManager.AddItem(cacheIdCheckFileExists, "1", policycacheIdCheckFileExists);

                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists)
                    {
                        // The resource file exists but it is necessary from time to time to update the file creation date 
                        // in order to avoid the file to be deleted by the clean up process.
                        // To know whether the check has been performed (in order to avoid executing this check everytime) creates 
                        // a cache item that will expire in 12 hours.
                        if (CacheManager.GetItem(cacheIdCheckCreationDate) == null)
                        {
                            File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow);

                            CacheItemPolicy policyCheckCreationDate = new CacheItemPolicy
                            {
                                SlidingExpiration = TimeSpan.FromHours(CheckCreationDateFrequencyHours),
                                Priority = CacheItemPriority.NotRemovable
                            };

                            CacheManager.AddItem(cacheIdCheckCreationDate, "1", policyCheckCreationDate);
                        }
                    }
                    else
                    {
                        // The resource file doesn't exist 
                        // Make sure that the directory exists
                        string directoryPath = HostingEnvironment.MapPath(CruncherConfiguration.Instance.PhysicalFilesPath);
                        if (directoryPath != null)
                        {
                            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
                            if (!directoryInfo.Exists)
                            {
                                // Don't swallow any errors. We want to know if this doesn't work.
                                Directory.CreateDirectory(directoryPath);
                            }
                        }

                        File.WriteAllText(filePath, fileContent);
                    }
                }
            }

            // Return the url absolute path
            return fileVirtualPath.TrimStart('~');
        }

        /// <summary>
        /// Trims the physical files folder ensuring that it does not contains files older than xx days 
        /// </summary>
        /// <param name="path">
        /// The path to the folder.
        /// </param>
        public static void TrimPhysicalFilesFolder(string path)
        {
            // If PhysicalFilesDaysBeforeRemoveExpired is 0 or negative then the trim process is not performed
            if (CruncherConfiguration.Instance.PhysicalFilesDaysBeforeRemoveExpired < 1)
                return;

            // Settings for the clean up process
            const string cacheIdTrimPhysicalFilesFolder = "_CruncherTrimPhysicalFilesFolder";
            const string cacheIdTrimPhysicalFilesFolderAppPoolRecycled = "_CruncherTrimPhysicalFilesFolderAppPoolRecycled";
            const int trimPhysicalFilesFolderDelayedExecutionMin = 5;
            const int trimPhysicalFilesFolderFrequencyHours = 7;

            CacheItemPolicy policy = new CacheItemPolicy
            {
                Priority = CacheItemPriority.NotRemovable
            };

            // To know whether the trim process has already been performed (in order to avoid executing this process everytime) creates 
            // a cache item that will expire in 12 hours. 
            // To avoid that the cleanup process is run just after an APPPool Recycle (or cache recycle) it uses another cache item that will never expire
            // The main reason is because after an AppPool reset there are a lot of things going on and it is not the optimal moment to perform many I/O ops
            if (CacheManager.GetItem(cacheIdTrimPhysicalFilesFolderAppPoolRecycled) == null)
            {
                // Creates the cache item that will expire first
                policy.SlidingExpiration = TimeSpan.FromMinutes(trimPhysicalFilesFolderDelayedExecutionMin);
                CacheManager.AddItem(cacheIdTrimPhysicalFilesFolder, "1", policy);

                // Creates the cache item that will never expire
                policy.SlidingExpiration = TimeSpan.FromDays(365);
                CacheManager.AddItem(cacheIdTrimPhysicalFilesFolderAppPoolRecycled, "1", policy);

                return;
            }

            if (CacheManager.GetItem(cacheIdTrimPhysicalFilesFolder) != null)
            {
                return;
            }

            policy.SlidingExpiration = TimeSpan.FromHours(trimPhysicalFilesFolderFrequencyHours);
            CacheManager.AddItem(cacheIdTrimPhysicalFilesFolder, "1", policy);

            if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                // Regular expression to get resource files which names match Cruncher's filename pattern.
                IEnumerable<FileInfo> files = directoryInfo.EnumerateFiles().Where(f => PhysicalFileRegex.IsMatch(Path.GetFileName(f.Name))).OrderBy(f => f.CreationTimeUtc);
                foreach (FileInfo fileInfo in files)
                {
                    try
                    {
                        // If the file's last write datetime is older that xx days then delete it
                        if (fileInfo.LastWriteTimeUtc.AddDays(CruncherConfiguration.Instance.PhysicalFilesDaysBeforeRemoveExpired) > DateTime.UtcNow)
                        {
                            continue;
                        }

                        // Delete the file
                        fileInfo.Delete();
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                        // Do nothing; skip to the next file.
                    }
                }
            }
        }

        // TODO: Move this to a tests project
        //public static string Test()
        //{
        //    StringBuilder result = new StringBuilder();
        //    List<string> files = new List<string>()
        //        {
        //            "page.aspx",
        //            "style.css",
        //            "javascript.js",
        //            "without-extension",
        //            "directory/page.aspx",
        //            "/directory/page.aspx",
        //            "domain.com/page.aspx",
        //            "domain.com/directory/page.aspx",
        //            "http://domain.com/directory/page.aspx",
        //            "directory\\file.txt",
        //            "\\directory\\file.txt",
        //            "C:\\directory\\file.txt",
        //            "C:\\file.txt",
        //            "\\\\directory\\file.txt",
        //            "\\\\file.txt"
        //        };
        //    foreach (string file in files)
        //    {
        //        result.AppendFormat("{0} --> {1}{2}", file, getFilePath(file), Environment.NewLine);
        //    }
        //    Console.WriteLine(result.ToString());
        //    return result.ToString();
        //}
    }
}
