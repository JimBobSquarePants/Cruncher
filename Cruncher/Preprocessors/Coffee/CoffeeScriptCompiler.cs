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
    using System;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Text;
    using Jurassic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    #endregion

    /// <summary>
    /// The CoffeeScript compiler.
    /// </summary>
    internal sealed class CoffeeScriptCompiler
    {
        #region Fields
        /// <summary>
        /// Name of resource, which contains a CoffeeScript-library
        /// </summary>
        private const string CoffeeScriptLibraryResource = "Cruncher.Preprocessors.Coffee.Resources.coffee-script.min.js";

        /// <summary>
        /// Name of resource, which contains a CoffeeScript-compiler helper
        /// </summary>
        private const string CoffeeScriptHelperResource = "Cruncher.Preprocessors.Coffee.Resources.coffee-script-helpers.min.js";

        /// <summary>
        /// Template of function call, which is responsible for compilation
        /// </summary>
        private const string CompilationFunctionCallTemplate = @"coffeeScriptHelper.compile({0}, {1});";

        /// <summary>
        /// The synchronization root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The CoffeeScript resource.
        /// </summary>
        private static string coffeescript = string.Empty;

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
                if (string.IsNullOrWhiteSpace(coffeescript))
                {
                    coffeescript = LoadCoffeeScript();
                }

                return coffeescript;
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
                    lock (SyncRoot)
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
        /// The <see cref="string"/> containing the CoffeeScript assembly manifest resource.
        /// </returns>
        public static string LoadCoffeeScript()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(GetAssemblyResource(CoffeeScriptLibraryResource));
            stringBuilder.Append(GetAssemblyResource(CoffeeScriptHelperResource));

            return stringBuilder.ToString();
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
            string compiledInput;
            try
            {
                string result =
                    CoffeeScriptEngine.Evaluate<string>(
                        string.Format(
                            CompilationFunctionCallTemplate,
                            JsonConvert.SerializeObject(input),
                            "{bare: false}"));

                JObject json = JObject.Parse(result);
                var errors = json["errors"] != null ? json["errors"] as JArray : null;

                if (errors != null && errors.Count > 0)
                {
                    throw new CoffeeScriptCompilingException(FormatErrorDetails(errors[0]));
                }

                compiledInput = json.Value<string>("compiledCode");
            }
            catch (Exception ex)
            {
                throw new CoffeeScriptCompilingException(ex.Message, ex.InnerException);
            }

            return compiledInput;
        }

        /// <summary>
        /// Gets the assembly manifest resource.
        /// </summary>
        /// <param name="resource">
        /// The resource to retrieve.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> containing the specified assembly manifest resource.
        /// </returns>
        /// A <exception cref="MissingManifestResourceException"> containing the error message.
        /// </exception>
        private static string GetAssemblyResource(string resource)
        {
            using (Stream stream = Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }

                throw new MissingManifestResourceException(resource.Split(new[] { "Resources." }, StringSplitOptions.None)[1]);
            }
        }

        /// <summary>
        /// Generates a detailed error message
        /// </summary>
        /// <param name="errorDetails">Error details</param>
        /// <returns>Detailed error message</returns>
        private static string FormatErrorDetails(JToken errorDetails)
        {
            string message = errorDetails.Value<string>("message");
            int lineNumber = errorDetails.Value<int>("lineNumber");
            int columnNumber = errorDetails.Value<int>("columnNumber");

            StringBuilder errorMessage = new StringBuilder();
            errorMessage.AppendFormat("{0}: {1}", "Message", message);
            errorMessage.AppendLine();

            if (lineNumber > 0)
            {
                errorMessage.AppendFormat("{0}: {1}", "Line Number", lineNumber);
                errorMessage.AppendLine();
            }

            if (columnNumber > 0)
            {
                errorMessage.AppendFormat("{0}: {1}", "Column Number", columnNumber);
            }

            return errorMessage.ToString();
        }
        #endregion
    }
}
