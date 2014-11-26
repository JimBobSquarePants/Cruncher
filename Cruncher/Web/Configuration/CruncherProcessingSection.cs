// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CruncherProcessingSection.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Represents a CruncherProcessingSection within a configuration file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Web.Configuration
{
    #region Using
    using System.Configuration;
    #endregion

    /// <summary>
    /// Represents a CruncherProcessingSection within a configuration file.
    /// </summary>
    public class CruncherProcessingSection : ConfigurationSection
    {
        #region Properties
        /// <summary>
        /// Gets or sets the <see cref="T:Cruncher.Web.Configuration.CruncherProcessingSection.CompressionElement"/>.
        /// </summary>
        /// <value>The <see cref="T:Cruncher.Web.Configuration.CruncherProcessingSection.CompressionElement"/>.</value>
        [ConfigurationProperty("compression", IsRequired = true)]
        public CompressionElement Compression
        {
            get { return (CompressionElement)this["compression"]; }
            set { this["compression"] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:Cruncher.Web.Configuration.CruncherProcessingSection.VirtualPathsElement"/>.
        /// </summary>
        /// <value>The <see cref="T:Cruncher.Web.Configuration.CruncherProcessingSection.VirtualPathsElement"/>.</value>
        [ConfigurationProperty("virtualPaths", IsRequired = true)]
        public VirtualPathsElement VirtualPaths
        {
            get { return (VirtualPathsElement)this["virtualPaths"]; }
            set { this["virtualPaths"] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:Cruncher.Web.Configuration.CruncherProcessingSection.PhysicalFilesElement"/>.
        /// </summary>
        /// <value>The <see cref="T:Cruncher.Web.Configuration.CruncherProcessingSection.PhysicalFilesElement"/>.</value>
        [ConfigurationProperty("physicalFiles", IsRequired = true)]
        public PhysicalFilesElement PhysicalFiles
        {
            get { return (PhysicalFilesElement)this["physicalFiles"]; }
            set { this["physicalFiles"] = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Retrieves the processing configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The processing configuration section from the current application configuration. </returns>
        public static CruncherProcessingSection GetConfiguration()
        {
            CruncherProcessingSection clientResourcesProcessingSection = ConfigurationManager.GetSection("cruncher/processing") as CruncherProcessingSection;

            if (clientResourcesProcessingSection != null)
            {
                return clientResourcesProcessingSection;
            }

            return new CruncherProcessingSection();
        }
        #endregion

        /// <summary>
        /// Represents a relativeRoot configuration element within the configuration.
        /// </summary>
        public class CompressionElement : ConfigurationElement
        {
            /// <summary>
            /// Gets or sets a value indicating whether the current application is allowed to minify css files.
            /// </summary>
            /// <value><see langword="true"/> if the current application is allowed to minify css files; otherwise, <see langword="false"/>.</value>
            [ConfigurationProperty("minifyCSS", DefaultValue = false, IsRequired = true)]
            public bool MinifyCSS
            {
                get { return (bool)this["minifyCSS"]; }
                set { this["minifyCSS"] = value; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the current application is allowed to minify JavaScript files.
            /// </summary>
            /// <value><see langword="true"/> if the current application is allowed to minify JavaScript files; otherwise, <see langword="false"/>.</value>
            [ConfigurationProperty("minifyJS", DefaultValue = false, IsRequired = true)]
            public bool MinifyJS
            {
                get { return (bool)this["minifyJS"]; }
                set { this["minifyJS"] = value; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the current application is allowed to compress client resource files.
            /// </summary>
            /// <value><see langword="true"/> if the current application is allowed to minify JavaScript files; otherwise, <see langword="false"/>.</value>
            [ConfigurationProperty("compressResponse", DefaultValue = false, IsRequired = true)]
            public bool CompressResources
            {
                get { return (bool)this["compressResponse"]; }
                set { this["compressResponse"] = value; }
            }
        }

        /// <summary>
        /// Represents a virtualPaths configuration element within the configuration.
        /// </summary>
        public class VirtualPathsElement : ConfigurationElement
        {
            /// <summary>
            /// Gets or sets the virtual path(s) of the css folder(s).
            /// </summary>
            /// <value>The name of the cache folder(s).</value>
            /// <remarks>Multiple paths are comma separated.</remarks>
            [ConfigurationProperty("cssPaths", DefaultValue = "~/css", IsRequired = true)]
            [StringValidator(MinLength = 3, MaxLength = 200)]
            public string CSSPaths
            {
                get
                {
                    string virtualPath = (string)this["cssPaths"];

                    return virtualPath;
                }

                set
                {
                    this["cssPaths"] = value;
                }
            }

            /// <summary>
            /// Gets or sets the virtual path(s) of the JavaScript folder(s).
            /// </summary>
            /// <value>The name of the JavaScript folder(s).</value>
            /// <remarks>Multiple paths are comma separated.</remarks>
            [ConfigurationProperty("jsPaths", DefaultValue = "~/js", IsRequired = true)]
            [StringValidator(MinLength = 3, MaxLength = 200)]
            public string JavaScriptPaths
            {
                get
                {
                    string virtualPath = (string)this["jsPaths"];

                    return virtualPath;
                }

                set
                {
                    this["jsPaths"] = value;
                }
            }
        }

        /// <summary>
        /// Represents a physicalFiles configuration element within the configuration.
        /// </summary>
        public class PhysicalFilesElement : ConfigurationElement
        {
            /// <summary>
            /// Gets or sets where to create resource files (css/javascript)
            /// </summary>
            /// <value>The path of the cache folder.</value>
            [ConfigurationProperty("path", DefaultValue = "~/assets-cruncher", IsRequired = true)]
            [StringValidator(MinLength = 3, MaxLength = 200)]
            public string Path
            {
                get
                {
                    string virtualPath = (string)this["path"];
                    return virtualPath;
                }

                set
                {
                    this["path"] = value;
                }
            }

            /// <summary>
            /// Gets or sets the number of days to keep old files
            /// </summary>
            /// <value>The number of days</value>
            [ConfigurationProperty("daysBeforeRemoveExpired", DefaultValue = "7", IsRequired = false)]
            [IntegerValidator(ExcludeRange = false)]
            public int DaysBeforeRemoveExpired
            {
                get
                {
                    return (int)this["daysBeforeRemoveExpired"];
                }

                set
                {
                    this["daysBeforeRemoveExpired"] = value;
                }
            }
        }
    }
}
