// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CoffeeScriptCompiler.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The CoffeeScript compiler.
//   Much thanks here to Justin Etheridge's SquishIt project https://github.com/jetheredge/SquishIt
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors.Coffee
{
    #region Using
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using Jurassic;
    #endregion

    /// <summary>
    /// The CoffeeScript compiler.
    /// </summary>
    public class CoffeeScriptCompiler
    {
        #region Fields
        /// <summary>
        /// The CoffeeScript resource.
        /// </summary>
        private static string coffeescript;

        /// <summary>
        /// The <see cref="T:Jurassic.ScriptEngine"/> that processes the CoffeeScript.
        /// </summary>
        private static ScriptEngine scriptEngine;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the compiler.
        /// </summary>
        public static string Compiler
        {
            get
            {
                return coffeescript ?? (coffeescript = LoadCoffeeScript());
            }
        }

        /// <summary>
        /// Gets the coffee script engine.
        /// </summary>
        public static ScriptEngine CoffeeScriptEngine
        {
            get
            {
                if (scriptEngine == null)
                {
                    lock (typeof(CoffeeScriptCompiler))
                    {
                        ScriptEngine engine = new ScriptEngine { ForceStrictMode = true };
                        engine.Execute(Compiler);
                        scriptEngine = engine;
                    }
                }

                return scriptEngine;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads the CoffeeScript assembly manifest resource.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> containing the CoffeeScript assembly manifest resource..
        /// </returns>
        public static string LoadCoffeeScript()
        {
            using (var stream = Assembly.GetExecutingAssembly()
                                        .GetManifestResourceStream("Cruncher.Preprocessors.Coffee.coffee-script.js"))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }

                throw new MissingManifestResourceException("Cannot load the CoffeeScript.js resource.");
            }
        }

        /// <summary>
        /// Gets a string containing the compiled CoffeeScript result.
        /// </summary>
        /// <param name="input">
        /// The input to compile.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> containing the compiled CoffeeScript result.
        /// </returns>
        public string Compile(string input)
        {
            CoffeeScriptEngine.SetGlobalValue("Source", input);

            // Errors go from here straight on to the rendered page; 
            // we don't want to hide them because they provide valuable feedback
            // on the location of the error.
            string result = CoffeeScriptEngine.Evaluate<string>("CoffeeScript.compile(Source, {bare: false})");

            return result;
        }
        #endregion
    }
}
