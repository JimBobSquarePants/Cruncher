// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceAwarePlatformAdaptationLayer.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The resource aware platform adaptation layer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors.Sass
{
    #region Using
    using System.IO;
    using System.Reflection;

    using Microsoft.Ajax.Utilities.Configuration;
    using Microsoft.Scripting;
    #endregion

    /// <summary>
    /// The resource aware platform adaptation layer.
    /// </summary>
    public class ResourceAwarePlatformAdaptationLayer : PlatformAdaptationLayer
    {
        /// <summary>
        /// Gets the stream containing the specified manifest resource.
        /// </summary>
        /// <param name="path">
        /// The path to the resource.
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/> containing the manifest resource.
        /// </returns>
        public override Stream OpenInputFileStream(string path)
        {
            var ret = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.PathToResourceName(path));
            if (ret != null)
            {
                return ret;
            }

            if (SassCompiler.RootAppPath == null || !path.ToLowerInvariant().StartsWith(SassCompiler.RootAppPath))
            {
                return null;
            }

            return base.OpenInputFileStream(path);
        }

        /// <summary>
        /// Gets a value indicating whether the specified manifest resource exists.
        /// </summary>
        /// <param name="path">
        /// The path to the resource.
        /// </param>
        /// <returns>
        /// True if the specified resource exists; otherwise, false.
        /// </returns>
        public override bool FileExists(string path)
        {
            string resourcePath = this.PathToResourceName(path);

            if (Assembly.GetExecutingAssembly().GetManifestResourceInfo(resourcePath) != null)
            {
                return true;
            }

            return base.FileExists(path);
        }

        /// <summary>
        /// Gets the path to specified resource name.
        /// </summary>
        /// <param name="path">
        /// The embedded path.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> containing the path to the specified resource.
        /// </returns>
        private string PathToResourceName(string path)
        {
            return path
                .Replace("1.9.1", "_1._9._1")
                .Replace('\\', '.')
                .Replace('/', '.')
                .Replace("R:", "Cruncher.Preprocessors.Sass");
        }
    }
}