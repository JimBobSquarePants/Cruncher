// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DotLessPreprocessor.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Provides methods to convert LESS into CSS.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors.Less
{
    #region Using
    using dotless.Core;
    using dotless.Core.configuration;
    using dotless.Core.Importers;
    using dotless.Core.Input;
    using dotless.Core.Parser;
    #endregion

    /// <summary>
    /// Provides methods to convert LESS into CSS.
    /// </summary>
    public class DotLessPreprocessor : IPreprocessor
    {
        #region Fields
        /// <summary>
        /// An instance of the <see cref="DotlessConfiguration"/>.
        /// </summary>
        private static readonly DotlessConfiguration Config = new DotlessConfiguration
        {
            CacheEnabled = false,
            MinifyOutput = false,
        };

        /// <summary>
        /// The Engine Factory that will perform the preprocessing.
        /// </summary>
        private static readonly ILessEngine Engine = new EngineFactory(Config).GetEngine();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the extension that this filter processes.
        /// </summary>
        public string[] AllowedExtensions
        {
            get
            {
                return new[] { ".LESS" };
            }
        }
        #endregion

        /// <summary>
        /// Transforms the content of the given string from Less into CSS. 
        /// </summary>
        /// <param name="input">The input Less string to transform.</param>
        /// <param name="path">The path to the given input Less string to transform.</param>
        /// <returns>The transformed CSS string.</returns>
        public string Transform(string input, string path)
        {
            // The standard engine returns a FileNotFoundExecption so I've rolled my own path resolver.
            Parser parser = new Parser();
            DotLessPathResolver dotLessPathResolver = new DotLessPathResolver(path);
            FileReader fileReader = new FileReader(dotLessPathResolver);
            parser.Importer = new Importer(fileReader);

            ILessEngine lessEngine = new LessEngine(parser);

            return lessEngine.TransformToCss(input, path);
        }
    }
}
