// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CruncherConfiguration.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods to allow the retrieval of cruncher settings.
//   <see href="http://csharpindepth.com/Articles/General/Singleton.aspx"/> 
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Cruncher.Postprocessors.AutoPrefixer;

    using JavaScriptEngineSwitcher.Core;

    /// <summary>
    /// Encapsulates methods to allow the retrieval of cruncher settings.
    /// <see href="http://csharpindepth.com/Articles/General/Singleton.aspx"/> 
    /// </summary>
    public class CruncherConfiguration
    {
        /// <summary>
        /// A new instance Initializes a new instance of the <see cref="CruncherConfiguration"/> class.
        /// initialized lazily.
        /// </summary>
        private static readonly Lazy<CruncherConfiguration> Lazy = new Lazy<CruncherConfiguration>(() => new CruncherConfiguration());

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
        private IList<string> cssPaths;

        /// <summary>
        /// An array of registered JavaScript paths for the application.
        /// </summary>
        private IList<string> javaScriptPaths;

        /// <summary>
        /// The auto prefixer options.
        /// </summary>
        private AutoPrefixerOptions autoPrefixerOptions;

        /// <summary>
        /// Delegate that creates an instance of JavaScript engine
        /// </summary>
        private Func<IJsEngine> jsEngineFunc;

        /// <summary>
        /// Prevents a default instance of the <see cref="CruncherConfiguration"/> class from being created.
        /// </summary>
        private CruncherConfiguration()
        {
        }

        #region Properties
        /// <summary>
        /// Gets the current instance of the <see cref="CruncherConfiguration"/> class.
        /// </summary>
        public static CruncherConfiguration Instance
        {
            get
            {
                return Lazy.Value;
            }
        }

        #region Security
        /// <summary>
        /// Gets a list of white-listed urls that images can be downloaded from.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
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
        /// Gets the delegate that creates an instance of JavaScript engine.
        /// </summary>
        public Func<IJsEngine> JsEngineFunc
        {
            get
            {
                if (this.jsEngineFunc == null)
                {
                    string engineName = this.GetCruncherProcessingSection().JsEngine;

                    this.jsEngineFunc = () => JsEngineSwitcher.Current.CreateJsEngineInstance(engineName);
                }

                return this.jsEngineFunc;
            }
        }

        /// <summary>
        /// Gets an array of registered css paths for the application.
        /// </summary>
        public IList<string> CSSPaths
        {
            get
            {
                return this.cssPaths
                       ?? (this.cssPaths = this.GetCruncherProcessingSection().VirtualPaths.CSSPaths.Split(',').Select(p => p.Trim()).ToList());
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
                       ?? (this.javaScriptPaths = this.GetCruncherProcessingSection().VirtualPaths.JavaScriptPaths.Split(',').Select(p => p.Trim()).ToList());
            }
        }

        /// <summary>
        /// Gets the directory's path where to store physical files
        /// </summary>
        public string PhysicalFilesPath
        {
            get
            {
                return this.GetCruncherProcessingSection().PhysicalFiles.Path;
            }
        }

        /// <summary>
        /// Gets the number of days to keep physical files
        /// </summary>
        public int PhysicalFilesDaysBeforeRemoveExpired
        {
            get
            {
                return this.GetCruncherProcessingSection().PhysicalFiles.DaysBeforeRemoveExpired;
            }
        }

        /// <summary>
        /// Gets the auto prefixer options.
        /// </summary>
        public AutoPrefixerOptions AutoPrefixerOptions
        {
            get
            {
                return this.GetAutoPrefixerOptions();
            }
        }
        #endregion
        #endregion

        #region Methods
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

        /// <summary>
        /// Retrieves the auto prefixer configuration options from the current application configuration. 
        /// </summary>
        /// <returns>
        /// The <see cref="AutoPrefixerOptions"/> from the current application configuration.
        /// </returns>
        private AutoPrefixerOptions GetAutoPrefixerOptions()
        {
            if (this.autoPrefixerOptions != null)
            {
                return this.autoPrefixerOptions;
            }

            this.autoPrefixerOptions = new AutoPrefixerOptions
            {
                Browsers = this.GetCruncherProcessingSection().AutoPrefixer.Browsers.Split(',').Select(p => p.Trim()).ToList(),
                Enabled = this.GetCruncherProcessingSection().AutoPrefixer.Enabled,
                Cascade = this.GetCruncherProcessingSection().AutoPrefixer.Cascade,
                Safe = this.GetCruncherProcessingSection().AutoPrefixer.Safe
            };

            return this.autoPrefixerOptions;
        }
        #endregion
    }
}
