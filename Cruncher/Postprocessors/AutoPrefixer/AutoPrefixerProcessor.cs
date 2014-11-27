namespace Cruncher.Postprocessors.AutoPrefixer
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Text;

    using Cruncher.Preprocessors.Coffee;

    using Microsoft.ClearScript.Windows;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal sealed class AutoPrefixerProcessor
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
        /// The AutoPrefixer resource.
        /// </summary>
        private static string autoPrefixer = string.Empty;

        /// <summary>
        /// Gets the compiler.
        /// </summary>
        public static string Compiler
        {
            get
            {
                if (string.IsNullOrWhiteSpace(autoPrefixer))
                {
                    autoPrefixer = LoadAutoPrefixerScript();
                }

                return autoPrefixer;
            }
        }

        /// <summary>
        /// Loads the AutoPrefixer assembly manifest resource.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> containing the AutoPrefixer assembly manifest resource.
        /// </returns>
        public static string LoadAutoPrefixerScript()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(GetAssemblyResource(AutoPrefixerLibraryResource));
            stringBuilder.Append(GetAssemblyResource(AutoPrefixerHelperResource));

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets a string containing the compiled CoffeeScript result.
        /// </summary>
        /// <param name="input">
        /// The input to compile.
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
            try
            {
                string result;
                using (JScriptEngine engine = new JScriptEngine(WindowsScriptEngineFlags.EnableStandardsMode))
                {
                    engine.Execute(Compiler);
                    string expression = string.Format(CompilationFunctionCallTemplate, JsonConvert.SerializeObject(input), ConvertAutoPrefixerOptionsToJson(options));
                    result = engine.Evaluate(expression).ToString();
                }

                JObject json = JObject.Parse(result);
                JArray errors = json["errors"] != null ? json["errors"] as JArray : null;

                if (errors != null && errors.Count > 0)
                {
                    throw new AutoPrefixerProcessingException(FormatErrorDetails(errors[0]));
                }

                processedCode = json.Value<string>("processedCode");
            }
            catch (Exception ex)
            {
                throw new CoffeeScriptCompilingException(ex.Message, ex.InnerException);
            }

            return processedCode;
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
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
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
    }
}
