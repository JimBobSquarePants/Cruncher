// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CssMinifier.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Helper class for performing minification of CSS Stylesheets.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Compression
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.Ajax.Utilities;

    /// <summary>
    /// Helper class for performing minification of CSS Stylesheets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is basically a wrapper for the AjaxMin library(lib/AjaxMin.dll).
    /// <see href="http://ajaxmin.codeplex.com/"/>
    /// <see href="http://www.asp.net/ajaxlibrary/AjaxMinDLL.ashx"/>
    /// </para>
    /// <para>
    /// There are no symbols that come with the AjaxMin dll, so this class gives a bit of intellisense 
    /// help for basic control. AjaxMin is a pretty dense library with lots of different settings, so
    /// everyone's encouraged to use it directly if they want to.
    /// </para>
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class CssMinifier
    {
        /// <summary>
        /// The instance of the <see cref="T:Microsoft.Ajax.Utilities.Minifier">Minifer</see> to use.
        /// </summary>
        private readonly Minifier ajaxMinifier = new Minifier();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Cruncher.Compression.CssMinifier">CssMinifier</see> class. 
        /// </summary>
        public CssMinifier()
        {
            this.RemoveWhiteSpace = true;
            this.ColorNamesRange = ColorNamesRange.W3CStrict;
        }

        /// <summary>
        /// Gets or sets what range of colors the css stylesheet should utilize.
        /// </summary>
        /// <value>What range of colors the css stylesheet should utilize</value>
        public ColorNamesRange ColorNamesRange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whitespace should be removed from the stylesheet.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if whitespace should be removed from the stylesheet; otherwise, <see langword="false"/>.
        /// </value>
        public bool RemoveWhiteSpace { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance of the minifier should perform the minification.
        /// </summary>
        /// <value><see langword="true"/> if the minifier should perform the minification; otherwise, <see langword="false"/>.</value>
        private bool ShouldMinify => this.RemoveWhiteSpace;

        /// <summary>
        /// Gets the minified version of the submitted stylesheet.
        /// </summary>
        /// <param name="styleSheet">The stylesheet to minify.</param>
        /// <returns>The minified version of the submitted stylesheet.</returns>
        public string Minify(string styleSheet)
        {
            if (this.ShouldMinify)
            {
                if (string.IsNullOrWhiteSpace(styleSheet))
                {
                    return string.Empty;
                }

                // The minifier is double escaping '\' when it finds it in the file.
                return this.ajaxMinifier.MinifyStyleSheet(styleSheet, this.CreateCssSettings()).Replace(@"\5c\2e", @"\.");
            }

            return styleSheet;
        }

        /// <summary>
        /// Builds the required CssSettings class needed for the Ajax Minifier.
        /// </summary>
        /// <returns>The required CssSettings class needed for the Ajax Minifier.</returns>
        private CssSettings CreateCssSettings()
        {
            CssSettings cssSettings = new CssSettings
            {
                OutputMode = this.RemoveWhiteSpace ? OutputMode.SingleLine : OutputMode.MultipleLines
            };

            if (this.ShouldMinify)
            {
                switch (this.ColorNamesRange)
                {
                    case ColorNamesRange.W3CStrict:
                        cssSettings.ColorNames = CssColor.Strict;
                        break;
                    case ColorNamesRange.HexadecimalOnly:
                        cssSettings.ColorNames = CssColor.Hex;
                        break;
                    case ColorNamesRange.AllMajorColors:
                        cssSettings.ColorNames = CssColor.Major;
                        break;
                }
            }

            return cssSettings;
        }
    }
}
