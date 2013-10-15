// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JavaScriptCruncher.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The JavaScript cruncher.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher
{
    #region Using
    using Cruncher.Compression;
    #endregion

    /// <summary>
    /// The JavaScript cruncher.
    /// </summary>
    public class JavaScriptCruncher : CruncherBase
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptCruncher"/> class.
        /// </summary>
        /// <param name="options">The options containing instructions for the cruncher.</param>
        public JavaScriptCruncher(CruncherOptions options)
            : base(options)
        {
        }
        #endregion

        #region Methods
        #region Public
        /// <summary>
        /// Minifies the specified resource.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>
        /// The minified resource.
        /// </returns>
        public override string Minify(string resource)
        {
            JavaScriptMinifier minifier;

            if (this.Options.Minify)
            {
                minifier = new JavaScriptMinifier
                {
                    VariableMinification = VariableMinification.LocalVariablesAndFunctionArguments
                };
            }
            else
            {
                minifier = new JavaScriptMinifier
                {
                    VariableMinification = VariableMinification.None,
                    PreserveFunctionNames = true,
                    RemoveWhiteSpace = false
                };
            }

            string result = minifier.Minify(resource);

            if (this.Options.CacheFiles)
            {
                this.AddItemToCache(this.Options.MinifyCacheKey, result);
            }

            return result;
        }
        #endregion

        #region Protected
        /// <summary>
        /// Loads the local file.
        /// </summary>
        /// <param name="file">The file to load.</param>
        /// <returns>
        /// The contents of the local file as a string.
        /// </returns>
        protected override string LoadLocalFile(string file)
        {
            string contents = base.LoadLocalFile(file);

            contents = this.PreProcessInput(contents, file);

            // Cache if applicable.
            this.AddFileMonitor(file, contents);

            return contents;
        }
        #endregion
        #endregion
    }
}
