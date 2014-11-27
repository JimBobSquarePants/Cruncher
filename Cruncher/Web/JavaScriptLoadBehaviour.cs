// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JavaScriptLoadBehaviour.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Describes the various loading behaviour available to the JavaScript script tag.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Web
{
    /// <summary>
    /// Describes the various loading behaviour available to the JavaScript script tag.
    /// <see href="http://www.growingwiththeweb.com/2014/02/async-vs-defer-attributes.html"/>
    /// </summary>
    public enum JavaScriptLoadBehaviour
    {
        /// <summary>
        /// The default behaviour. Loads the script as soon as it is read within the page, blocking
        /// rendering until it is loaded. 
        /// </summary>
        Inline,

        /// <summary>
        /// Adds the boolean async attribute to the rendered script elements allows the external JavaScript file to run 
        /// when it's available, without delaying page load first.
        /// </summary>
        Async,

        /// <summary>
        /// Adds the boolean defer attribute to the rendered script elements allowing the external JavaScript 
        /// file to run when the DOM is loaded, without delaying page load first.
        /// </summary>
        Defer
    }
}
