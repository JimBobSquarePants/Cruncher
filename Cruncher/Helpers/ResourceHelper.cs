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
    using System.IO;
    using System.Web;
    using System.Web.Hosting;

    /// <summary>
    /// Provides a series of helper methods for dealing with resources.
    /// </summary>
    public class ResourceHelper
    {
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
                            string path = Path.GetFullPath(Path.Combine(rootPath, resource));
                            return HostingEnvironment.MapPath(string.Format("~{0}", path));
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
