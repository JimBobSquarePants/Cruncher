// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoPrefixerProcessor.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The auto prefixer processor.
//   Many thanks here to Taritsyn's <see href="https://bundletransformer.codeplex.com/"/>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Postprocessors.AutoPrefixer
{
    using System;
    using System.Text;

    using JavaScriptEngineSwitcher.Core;
    using JavaScriptEngineSwitcher.Core.Helpers;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The auto prefixer processor.
    /// Many thanks here to Taritsyn's <see href="https://bundletransformer.codeplex.com/"/>
    /// </summary>
    internal sealed class AutoPrefixerProcessor : IDisposable
    {
        /// <summary>
        /// Name of resource, which contains a AutoPrefixer library
        /// </summary>
        private const string AutoPrefixerLibraryResource = "Cruncher.Postprocessors.AutoPrefixer.Resources.autoprefixer.min.js";

        /// <summary>
        /// Name of resource, which contains a AutoPrefixer processor helper
        /// </summary>
        private const string AutoPrefixerHelperResource = "Cruncher.Postprocessors.AutoPrefixer.Resources.autoprefixer-helpers.min.js";

        /// <summary>
        /// Template of function call, which is responsible for compilation
        /// </summary>
        private const string CompilationFunctionCallTemplate = @"autoprefixerHelper.process({0}, {1});";

        /// <summary>
        /// The sync root for locking against.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The javascript engine.
        /// </summary>
        private IJsEngine javascriptEngine;

        /// <summary>
        /// Whether the engine has been initialized.
        /// </summary>
        private bool initialized;

        /// <summary>
        /// A value indicating whether this instance of the given entity has been disposed.
        /// </summary>
        /// <value><see langword="true"/> if this instance has been disposed; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// If the entity is disposed, it must not be disposed a second
        /// time. The isDisposed field is set the first time the entity
        /// is disposed. If the isDisposed field is true, then the Dispose()
        /// method will not dispose again. This help not to prolong the entity's
        /// life in the Garbage Collector.
        /// </remarks>
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPrefixerProcessor"/> class.
        /// </summary>
        /// <param name="javascriptEngineFactory">
        /// The javascript engine factory.
        /// </param>
        public AutoPrefixerProcessor(Func<IJsEngine> javascriptEngineFactory)
        {
            this.javascriptEngine = javascriptEngineFactory();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AutoPrefixerProcessor"/> class. 
        /// </summary>
        /// <remarks>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method 
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in types derived from this class.
        /// </remarks>
        ~AutoPrefixerProcessor()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }

        /// <summary>
        /// Gets a string containing the compiled CoffeeScript result.
        /// </summary>
        /// <param name="input">
        /// The input to process.
        /// </param>
        /// <param name="options">
        /// The AutoPrefixer options.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> containing the compiled CoffeeScript result.
        /// </returns>
        public string Process(string input, AutoPrefixerOptions options)
        {
            string processedCode;

            lock (SyncRoot)
            {
                this.Initialize();

                try
                {
                    string result = this.javascriptEngine.Evaluate<string>(string.Format(CompilationFunctionCallTemplate, JsonConvert.SerializeObject(input), ConvertAutoPrefixerOptionsToJson(options)));

                    JObject json = JObject.Parse(result);
                    JArray errors = json["errors"] != null ? json["errors"] as JArray : null;

                    if (errors != null && errors.Count > 0)
                    {
                        throw new AutoPrefixerProcessingException(FormatErrorDetails(errors[0]));
                    }

                    processedCode = json.Value<string>("processedCode");
                }
                catch (JsRuntimeException ex)
                {
                    throw new AutoPrefixerProcessingException(JsRuntimeErrorHelpers.Format(ex));
                }
            }

            return processedCode;
        }

        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Converts <see cref="AutoPrefixerOptions"/> to JSON
        /// </summary>
        /// <param name="options">AutoPrefixerOptions options</param>
        /// <returns>AutoPrefixerOptions options in JSON format</returns>
        private static JObject ConvertAutoPrefixerOptionsToJson(AutoPrefixerOptions options)
        {
            JObject optionsJson = new JObject(
                new JProperty("browsers", new JArray(options.Browsers)),
                new JProperty("cascade", options.Cascade),
                new JProperty("safe", options.Safe));

            return optionsJson;
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

        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        /// <param name="disposing">If true, the object gets disposed.</param>
        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.javascriptEngine != null)
                {
                    this.javascriptEngine.Dispose();
                    this.javascriptEngine = null;
                }
            }

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // Note disposing is done.
            this.isDisposed = true;
        }

        /// <summary>
        /// Initializes CSS autoprefixer
        /// </summary>
        private void Initialize()
        {
            if (!this.initialized)
            {
                Type type = this.GetType();

                this.javascriptEngine.ExecuteResource(AutoPrefixerLibraryResource, type);
                this.javascriptEngine.ExecuteResource(AutoPrefixerHelperResource, type);

                this.initialized = true;
            }
        }
    }
}
