// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceAwareScriptHost.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The resource aware script host.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors.Sass
{
    #region Using
    using Microsoft.Scripting;
    using Microsoft.Scripting.Hosting; 
    #endregion

    /// <summary>
    /// The resource aware script host.
    /// </summary>
    public class ResourceAwareScriptHost : ScriptHost
    {
        /// <summary>
        /// The inner pal.
        /// </summary>
        private PlatformAdaptationLayer adaptionLayer;

        /// <summary>
        /// Gets the platform adaptation layer.
        /// </summary>
        public override PlatformAdaptationLayer PlatformAdaptationLayer
        {
            get { return this.adaptionLayer ?? (this.adaptionLayer = new ResourceAwarePlatformAdaptationLayer()); }
        }
    }
}