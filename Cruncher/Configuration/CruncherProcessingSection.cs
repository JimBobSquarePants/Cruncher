// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CruncherProcessingSection.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Represents a CruncherProcessingSection within a configuration file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Configuration
{
    using System.Configuration;

    /// <summary>
    /// Represents a CruncherProcessingSection within a configuration file.
    /// </summary>
    public class CruncherProcessingSection : ConfigurationSection
    {
        #region Properties
        /// <summary>
        /// Gets or sets the JavaScript Engine for processing embedded JavaScript resources for the application.
        /// </summary>
        /// <value>The JavaScript Engine for processing embedded JavaScript.</value>
        /// <remarks>Defaults to 'V8JsEngine' if not set.</remarks>
        [ConfigurationProperty("jsEngine", DefaultValue = "V8JsEngine", IsRequired = true)]
        public string JsEngine
        {
            get
            {
                return (string)this["jsEngine"];
            }

            set
            {
                this["jsEngine"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="VirtualPathsElement"/>.
        /// </summary>
        /// <value>The <see cref="VirtualPathsElement"/>.</value>
        [ConfigurationProperty("virtualPaths", IsRequired = true)]
        public VirtualPathsElement VirtualPaths
        {
            get { return (VirtualPathsElement)this["virtualPaths"]; }
            set { this["virtualPaths"] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="PhysicalFilesElement"/>.
        /// </summary>
        /// <value>The <see cref="PhysicalFilesElement"/>.</value>
        [ConfigurationProperty("physicalFiles", IsRequired = true)]
        public PhysicalFilesElement PhysicalFiles
        {
            get { return (PhysicalFilesElement)this["physicalFiles"]; }
            set { this["physicalFiles"] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="AutoPrefixerElement"/>.
        /// </summary>
        /// <value>The <see cref="AutoPrefixerElement"/>.</value>
        [ConfigurationProperty("autoPrefixer", IsRequired = true)]
        public AutoPrefixerElement AutoPrefixer
        {
            get { return (AutoPrefixerElement)this["autoPrefixer"]; }
            set { this["autoPrefixer"] = value; }
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

        /// <summary>
        /// Represents a auto prefixer configuration element within the configuration.
        /// </summary>
        public class AutoPrefixerElement : ConfigurationElement
        {
            /// <summary>
            /// Gets or sets a value indicating whether the current application should auto-prefix CSS files before minification.
            /// </summary>
            /// <value><see langword="true"/> if the current application is allowed to auto-prefix CSS files; otherwise, <see langword="false"/>.</value>
            [ConfigurationProperty("enabled", DefaultValue = true, IsRequired = true)]
            public bool Enabled
            {
                get { return (bool)this["enabled"]; }
                set { this["enabled"] = value; }
            }

            /// <summary>
            /// Gets or sets the browser(s) to provide prefixes for.
            /// </summary>
            /// <value>The browser(s) to provide prefixes for.</value>
            /// <remarks>Multiple browsers are comma separated.</remarks>
            [ConfigurationProperty("browsers", DefaultValue = "> 1%, last 2 versions, Firefox ESR, Opera 12.1", IsRequired = true)]
            [StringValidator(MinLength = 2, MaxLength = 200)]
            public string Browsers
            {
                get
                {
                    string virtualPath = (string)this["browsers"];

                    return virtualPath;
                }

                set
                {
                    this["browsers"] = value;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether to create nice visual cascade of prefixes.
            /// </summary>
            /// <value><see langword="true"/> if the current application is should cascade prefixes; otherwise, <see langword="false"/>.</value>
            [ConfigurationProperty("cascade", DefaultValue = true, IsRequired = true)]
            public bool Cascade
            {
                get { return (bool)this["cascade"]; }
                set { this["cascade"] = value; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether to enable the special safe mode to parse broken CSS.
            /// </summary>
            /// <value><see langword="true"/> if the current application is enable safe mode; otherwise, <see langword="false"/>.</value>
            [ConfigurationProperty("safe", DefaultValue = false, IsRequired = true)]
            public bool Safe
            {
                get { return (bool)this["safe"]; }
                set { this["safe"] = value; }
            }
        }
    }
}
