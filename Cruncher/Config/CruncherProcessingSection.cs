#region Licence
// -----------------------------------------------------------------------
// <copyright file="CruncherProcessingSection.cs" company="James South">
//     Copyright (c) James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.Config
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
        /// Gets or sets the <see cref="T:Cruncher.Config.CruncherProcessingSection.CompressionElement"/>.
        /// </summary>
        /// <value>The <see cref="T:Cruncher.Config.CruncherProcessingSection.CompressionElement"/>.</value>
        [ConfigurationProperty("compression", IsRequired = true)]
        public CompressionElement Compression
        {
            get { return (CompressionElement)this["compression"]; }
            set { this["compression"] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:Cruncher.Config.CruncherProcessingSection.VirtualPathsElement"/>.
        /// </summary>
        /// <value>The <see cref="T:Cruncher.Config.CruncherProcessingSection.VirtualPathsElement"/>.</value>
        [ConfigurationProperty("virtualPaths", IsRequired = true)]
        public VirtualPathsElement VirtualPaths
        {
            get { return (VirtualPathsElement)this["virtualPaths"]; }
            set { this["virtualPaths"] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:Cruncher.Config.CruncherProcessingSection.RelativeCSSRootElement"/>.
        /// </summary>
        /// <value>The <see cref="T:Cruncher.Config.CruncherProcessingSection.RelativeCSSRootElement"/>.</value>
        [ConfigurationProperty("relativeCssRoot", IsRequired = true)]
        public RelativeCSSRootElement RelativeRoot
        {
            get { return (RelativeCSSRootElement)this["relativeCssRoot"]; }
            set { this["relativeCssRoot"] = value; }
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
            /// Gets or sets a value indicating whether the current application is allowed to minify javascript files.
            /// </summary>
            /// <value><see langword="true"/> if the current application is allowed to minify javascript files; otherwise, <see langword="false"/>.</value>
            [ConfigurationProperty("minifyJS", DefaultValue = false, IsRequired = true)]
            public bool MinifyJS
            {
                get { return (bool)this["minifyJS"]; }
                set { this["minifyJS"] = value; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the current application is allowed to compress client resource files.
            /// </summary>
            /// <value><see langword="true"/> if the current application is allowed to minify javascript files; otherwise, <see langword="false"/>.</value>
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
            /// Gets or sets the virtual path(s) of the javascript folder(s).
            /// </summary>
            /// <value>The name of the javascript folder(s).</value>
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
        /// Represents a relativeRoot configuration element within the configuration.
        /// </summary>
        public class RelativeCSSRootElement : ConfigurationElement
        {
            /// <summary>
            /// Gets or sets the value used to replace the token '{root}' within a css file to determine the absolute root path for resources.
            /// </summary>
            /// <value>The value used to replace the token '{root}' within a css file to determine the absolute root path for resources.</value>
            [ConfigurationProperty("path", DefaultValue = "/css", IsRequired = true)]
            public string RelativeCSSRoot
            {
                get
                {
                    return (string)this["path"];
                }

                set
                {
                    this["path"] = value;
                }
            }
        }
    }
}
