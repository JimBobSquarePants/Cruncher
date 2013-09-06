// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DotLessPathResolver.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The dot less path resolver.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors
{
    #region Using
    using System;
    using System.Web;
    using System.Web.Hosting;
    using dotless.Core.Input;
    #endregion

    /// <summary>
    /// The dot less path resolver.
    /// </summary>
    public class DotLessPathResolver : IPathResolver
    {
        #region Fields
        /// <summary>
        /// The current file directory.
        /// </summary>
        private string currentFileDirectory;

        /// <summary>
        /// The current file path.
        /// </summary>
        private string currentFilePath;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DotLessPathResolver"/> class.
        /// </summary>
        /// <param name="currentFilePath">
        /// The current file path.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if currentFilePath is null.
        /// </exception>
        public DotLessPathResolver(string currentFilePath)
        {
            if (string.IsNullOrWhiteSpace(currentFilePath))
            {
                throw new ArgumentNullException("currentFilePath");
            }

            this.CurrentFilePath = currentFilePath;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the path to the currently processed file.
        /// </summary>
        public string CurrentFilePath
        {
            get
            {
                return this.currentFilePath;
            }

            set
            {
                string virtualRoot = "~/";

                // Get the virtual path for the file to import to.
                string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Uri rootUri = new Uri(rootDirectory);
                Uri fileUri = new Uri(value);

                value = rootUri.MakeRelativeUri(fileUri).ToString();

                if (HostingEnvironment.IsHosted)
                {
                    virtualRoot = HostingEnvironment.ApplicationVirtualPath;
                }

                value = VirtualPathUtility.Combine(virtualRoot, value);

                this.currentFilePath = value;
                this.currentFileDirectory = VirtualPathUtility.GetDirectory(value);
            }
        }

        /// <summary>
        /// Returns the full path for the specified file <param name="path"/>.
        /// </summary>
        /// <param name="path">The imported file path.</param>
        /// <returns>
        /// The <see cref="string"/> containing the imported file path.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if path is null.
        /// </exception>
        public string GetFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            return HostingEnvironment.MapPath(VirtualPathUtility.Combine(this.currentFileDirectory, path));
        }
        #endregion
    }
}