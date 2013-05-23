#region Licence
// -----------------------------------------------------------------------
// <copyright file="CruncherConfiguration.cs" company="James South">
//     Copyright (c) James South.
//     Licensed under the Apache License, Version 2.0.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.Config
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Cruncher.Preprocessors;
    #endregion

    /// <summary>
    /// Encapsulates methods to allow the retrieval of cruncher settings.
    /// http://csharpindepth.com/Articles/General/Singleton.aspx
    /// </summary>
    public class CruncherConfiguration
    {
        #region Fields
        /// <summary>
        /// A new instance Initializes a new instance of the <see cref="T:Cruncher.Config.CruncherConfiguration"/> class.
        /// intitialized lazily.
        /// </summary>
        private static readonly Lazy<CruncherConfiguration> Lazy =
                        new Lazy<CruncherConfiguration>(() => new CruncherConfiguration());

        /// <summary>
        /// Represents a CruncherCacheSection within a configuration file.
        /// </summary>
        private CruncherCacheSection cacheSection;

        /// <summary>
        /// Represents a CruncherSecuritySection within a configuration file.
        /// </summary>
        private CruncherSecuritySection securitySection;

        /// <summary>
        /// Represents a CruncherProcessingSection within a configuration file.
        /// </summary>
        private CruncherProcessingSection processingSection;

        /// <summary>
        /// An array of registered css paths for the application.
        /// </summary>
        private string[] cssPaths;

        /// <summary>
        /// An array of registered JavaScript paths for the application.
        /// </summary>
        private string[] javaScriptPaths;
        #endregion

        /// <summary>
        /// Gets the list of available Preprocessors.
        /// </summary>
        public IList<IPreprocessor> PreProcessors { get; private set; }

        /// <summary>
        /// The regular expression for matching allowed filetype.
        /// </summary>
        public Regex AllowedExtensionsRegex { get; private set; }

        #region Constructors
        /// <summary>
        /// Prevents a default instance of the <see cref="T:Cruncher.Config.CruncherConfiguration"/> class from being created.
        /// </summary>
        private CruncherConfiguration()
        {
            this.LoadPreprocessors();
            this.CreateAllowedExtensionRegex();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current instance of the <see cref="T:Cruncher.Config.CruncherConfiguration"/> class.
        /// </summary>
        public static CruncherConfiguration Instance
        {
            get
            {
                return Lazy.Value;
            }
        }

        #region Caching
        /// <summary>
        /// Gets the maximum number of days to store files in the cache.
        /// </summary>
        public int MaxCacheDays
        {
            get
            {
                return this.GetCruncherCacheSection().MaxDays;
            }
        }
        #endregion

        #region Security
        /// <summary>
        /// Gets a list of whitelisted urls that images can be downloaded from.
        /// </summary>
        public CruncherSecuritySection.WhiteListElementCollection RemoteFileWhiteList
        {
            get
            {
                return this.GetCruncherSecuritySection().WhiteList;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current application is allowed to download remote files.
        /// </summary>
        public bool AllowRemoteDownloads
        {
            get
            {
                return this.GetCruncherSecuritySection().AllowRemoteDownloads;
            }
        }

        /// <summary>
        /// Gets the maximum length to wait in milliseconds before throwing an error requesting a remote file.
        /// </summary>
        public int Timeout
        {
            get
            {
                return this.GetCruncherSecuritySection().Timeout;
            }
        }

        /// <summary>
        /// Gets the maximum allowable size in bytes of e remote file to process.
        /// </summary>
        public int MaxBytes
        {
            get
            {
                return this.GetCruncherSecuritySection().MaxBytes;
            }
        }
        #endregion

        #region Processing
        /// <summary>
        /// Gets an array of registered css paths for the application.
        /// </summary>
        public IList<string> CSSPaths
        {
            get
            {
                return this.cssPaths
                       ?? (this.cssPaths = this.GetCruncherProcessingSection().VirtualPaths.CSSPaths.Split(','));
            }
        }

        /// <summary>
        /// Gets an array of registered JavaScript paths for the application.
        /// </summary>
        public IList<string> JavaScriptPaths
        {
            get
            {
                return this.javaScriptPaths
                       ?? (this.javaScriptPaths = this.GetCruncherProcessingSection().VirtualPaths.JavaScriptPaths.Split(','));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current application is allowed to minify css files.
        /// </summary>
        public bool MinifyCSS
        {
            get
            {
                return this.GetCruncherProcessingSection().Compression.MinifyCSS;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current application is allowed to minify JavaScript files.
        /// </summary>
        public bool MinifyJavaScript
        {
            get
            {
                return this.GetCruncherProcessingSection().Compression.MinifyJS;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current application is allowed to compress client resource files.
        /// </summary>
        public bool CompressResources
        {
            get
            {
                return this.GetCruncherProcessingSection().Compression.CompressResources;
            }
        }
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Gets the list of available Preprocessors.
        /// </summary>
        private void LoadPreprocessors()
        {
            if (this.PreProcessors == null)
            {
                // Build a list of native IPreprocessors instances.
                Type type = typeof(IPreprocessor);
                IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                    .ToList();

                // Create them and add.
                this.PreProcessors = types.Select(x => (Activator.CreateInstance(x) as IPreprocessor))
                    .ToList();
            }
        }

        /// <summary>
        /// Generates a Regex with a list of allowed file type extensions.
        /// </summary>
        private void CreateAllowedExtensionRegex()
        {
            StringBuilder stringBuilder = new StringBuilder(@"\.CSS|\.JS|");

            foreach (IPreprocessor preprocessor in this.PreProcessors)
            {
                string extension = preprocessor.AllowedExtension;

                if (!string.IsNullOrWhiteSpace(extension))
                {
                    stringBuilder.AppendFormat(@"\{0}|", extension.ToUpperInvariant());
                }
            }

            this.AllowedExtensionsRegex = new Regex(stringBuilder.ToString().TrimEnd('|'), RegexOptions.IgnoreCase);
        }

        #region Configuration
        /// <summary>
        /// Retrieves the caching configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The caching configuration section from the current application configuration. </returns>
        private CruncherCacheSection GetCruncherCacheSection()
        {
            return this.cacheSection ?? (this.cacheSection = CruncherCacheSection.GetConfiguration());
        }

        /// <summary>
        /// Retrieves the security configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The security configuration section from the current application configuration. </returns>
        private CruncherSecuritySection GetCruncherSecuritySection()
        {
            return this.securitySection ?? (this.securitySection = CruncherSecuritySection.GetConfiguration());
        }

        /// <summary>
        /// Retrieves the processing configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The processing configuration section from the current application configuration. </returns>
        private CruncherProcessingSection GetCruncherProcessingSection()
        {
            return this.processingSection ?? (this.processingSection = CruncherProcessingSection.GetConfiguration());
        }
        #endregion
        #endregion
    }
}
