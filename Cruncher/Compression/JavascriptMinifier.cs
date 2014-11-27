// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JavascriptMinifier.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Helper class for performing minification of JavaScript.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Compression
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.Ajax.Utilities;

    /// <summary>
    /// Helper class for performing minification of JavaScript.
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
    public sealed class JavaScriptMinifier
    {
        /// <summary>
        /// The instance of the <see cref="T:Microsoft.Ajax.Utilities.Minifier">Minifer</see> to use.
        /// </summary>
        private readonly Minifier ajaxMinifier = new Minifier();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Cruncher.Compression.JavaScriptMinifier">JavaScriptMinifier</see> class. 
        /// </summary>
        public JavaScriptMinifier()
        {
            this.RemoveWhiteSpace = true;
            this.PreserveFunctionNames = false;
            this.VariableMinification = VariableMinification.LocalVariablesAndFunctionArguments;
        }

        /// <summary>
        /// Gets or sets whether this Minifier instance should minify local-scoped variables.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this value to LocalVariablesAndFunctionArguments can have a negative impact on some scripts.
        /// <example>A pre-minified jQuery will fail if passed through this.</example>
        /// </para>
        /// </remarks>
        public VariableMinification VariableMinification { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this minifier instance should preserve function names when minifying a script.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this minifier instance should preserve function names when minifying a script; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <remarks>Scripts that have external scripts relying on their functions should leave this set to true.</remarks>
        public bool PreserveFunctionNames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whitespace should be removed from the script.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if whitespace should be removed from the script; otherwise, <see langword="false"/>.
        /// </value>
        public bool RemoveWhiteSpace { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance of the minifier should minify the code.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance of the minifier should minify the code; otherwise, <see langword="false"/>.
        /// </value>
        private bool ShouldMinifyCode
        {
            get { return !this.PreserveFunctionNames || VariableMinification != VariableMinification.None; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance of the minifier should perform the minification.
        /// </summary>
        /// <value><see langword="true"/> if the minifier should perform the minification; otherwise, <see langword="false"/>.</value>
        private bool ShouldMinify
        {
            get { return this.RemoveWhiteSpace || this.ShouldMinifyCode; }
        }

        /// <summary>
        /// Gets the minified version of the submitted script.
        /// </summary>
        /// <param name="script">The script to minify.</param>
        /// <returns>The minified version of the submitted script.</returns>
        public string Minify(string script)
        {
            if (this.ShouldMinify)
            {
                if (string.IsNullOrWhiteSpace(script))
                {
                    return string.Empty;
                }

                return this.ajaxMinifier.MinifyJavaScript(script, this.CreateCodeSettings());
            }

            return script;
        }

        /// <summary>
        /// Builds the required CodeSettings class needed for the Ajax Minifier.
        /// </summary>
        /// <returns>The required CodeSettings class needed for the Ajax Minifier.</returns>
        private CodeSettings CreateCodeSettings()
        {
            CodeSettings codeSettings = new CodeSettings
            {
                MinifyCode = this.ShouldMinifyCode,
                OutputMode = this.RemoveWhiteSpace ? OutputMode.SingleLine : OutputMode.MultipleLines,
            };

            if (this.ShouldMinifyCode)
            {
                switch (this.VariableMinification)
                {
                    case VariableMinification.None:
                        codeSettings.LocalRenaming = LocalRenaming.KeepAll;
                        break;

                    case VariableMinification.LocalVariablesOnly:
                        codeSettings.LocalRenaming = LocalRenaming.KeepLocalizationVars;
                        break;

                    case VariableMinification.LocalVariablesAndFunctionArguments:
                        codeSettings.LocalRenaming = LocalRenaming.CrunchAll;
                        break;
                }

                // This is being set by default. A lot of scripts use eval to parse out various functions
                // and objects. These names need to be kept consistent with the actual arguments.
                codeSettings.EvalTreatment = EvalTreatment.Ignore;

                // This makes sure that function names on objects are kept exactly as they are. This is
                // so functions that other non-minified scripts rely on do not get renamed.
                codeSettings.PreserveFunctionNames = this.PreserveFunctionNames;

                // Specifies whether or not important comments will be retained in the output. 
                // Important comments are frequently used by developers to specify copyright or licensing 
                // information that needs to be retained in distributed scripts.
                // e.g /*! This is important */
                codeSettings.PreserveImportantComments = true;
            }

            return codeSettings;
        }
    }
}
