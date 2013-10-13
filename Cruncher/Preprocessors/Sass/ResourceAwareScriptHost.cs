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
    internal sealed class ResourceAwareScriptHost : ScriptHost
    {
        /// <summary>
        /// The inner platform adaptation layer.
        /// </summary>
        private ResourceAwarePlatformAdaptationLayer adaptionLayer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceAwareScriptHost"/> class.
        /// </summary>
        /// <param name="adaptationLayer">
        /// The adaptation layer.
        /// </param>
        public ResourceAwareScriptHost(ResourceAwarePlatformAdaptationLayer adaptationLayer)
        {
            this.adaptionLayer = adaptationLayer;
        }

        /// <summary>
        /// Gets the platform adaptation layer.
        /// </summary>
        public override PlatformAdaptationLayer PlatformAdaptationLayer
        {
            get { return this.adaptionLayer ?? (this.adaptionLayer = new ResourceAwarePlatformAdaptationLayer()); }
        }
    }
}