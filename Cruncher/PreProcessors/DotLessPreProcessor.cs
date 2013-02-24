#region Licence
// -----------------------------------------------------------------------
// <copyright file="IPreProcessor.cs" company="James South">
//     Copyright (c) James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace Cruncher.PreProcessors
{
    #region Using
    using dotless.Core;
    using dotless.Core.configuration;
    #endregion

    /// <summary>
    /// Provides methods to convert LESS into CSS.
    /// </summary>
    class DotLessPreProcessor : IPreProcessor
    {
        #region Properties
        /// <summary>
        /// An instance of the DotlessConfiguration.
        /// </summary>
        private static readonly DotlessConfiguration Config = new DotlessConfiguration
        {
            CacheEnabled = false,
            MinifyOutput = false
        };

        /// <summary>
        /// The Engine Factory that will perform the preprocessing.
        /// </summary>
        private static readonly EngineFactory EngineFactory = new EngineFactory(Config);
        #endregion

        /// <summary>
        /// Transforms the content of the given string from Less into CSS. 
        /// </summary>
        /// <param name="input">The input Less string to transform.</param>
        /// <returns>The transformed CSS string.</returns>
        public string Transform(string input)
        {
            return EngineFactory.GetEngine().TransformToCss(input, null);
        }
    }
}
