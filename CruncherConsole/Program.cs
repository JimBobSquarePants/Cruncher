// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CruncherConsole
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Cruncher;

    #endregion

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Mains the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        internal static void Main(string[] args)
        {
            try
            {
                if (args == null || !args.Any())
                {
                    Console.WriteLine(ResourceStrings.Usage);
                    Console.ReadLine();
                }
                else
                {
                    ConsoleConfiguration consoleConfiguration = GenerateConfiguration(args);
                    Crunch(consoleConfiguration);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Generates the configuration.
        /// </summary>
        /// <param name="args">The args to generate configuration from.</param>
        /// <returns>The populated <see cref="ConsoleConfiguration"/>.</returns>
        /// <exception cref="System.ArgumentNullException">args is null.</exception>
        private static ConsoleConfiguration GenerateConfiguration(IEnumerable<string> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            ConsoleConfiguration consoleConfiguration = new ConsoleConfiguration();

            // Process the arguments into variables
            foreach (string arg in args)
            {
                // Check that the argument starts with char '-'
                if (arg[0] == '-')
                {
                    // Split the argument into a key/value pair using a colon as the delimiter.
                    int split = arg.IndexOf(':');
                    string key = arg.Substring(1, (split > 0 ? split : arg.Length) - 1);
                    string value = split > -1 ? arg.Substring(split + 1) : string.Empty;

                    switch (key.ToUpperInvariant())
                    {
                        case "T":
                            consoleConfiguration.TargetType =
                                value == "css" ? CrunchTargetType.CSS : CrunchTargetType.JavaScript;
                            break;

                        case "M":
                            consoleConfiguration.Minify = true;
                            break;

                        case "IN":
                            consoleConfiguration.InputPath = value;
                            break;

                        case "OUT":
                            consoleConfiguration.OutputPath = value;
                            break;
                    }
                }
            }

            return consoleConfiguration;
        }

        /// <summary>
        /// The crunch css.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        private static void Crunch(ConsoleConfiguration configuration)
        {
            // Get the currently operating directory.
            // http://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
            string directoryName = Path.GetDirectoryName(configuration.InputPath);
            string rootFolder = Path.IsPathRooted(directoryName) ? directoryName : AppDomain.CurrentDomain.BaseDirectory;
            string fileName = configuration.InputPath;

            if (!string.IsNullOrWhiteSpace(fileName) && !string.IsNullOrWhiteSpace(rootFolder))
            {
                string actualInputPath = Path.GetFullPath(Path.Combine(rootFolder, fileName));
                string outputPath = GetOutPutPath(
                    rootFolder,
                    actualInputPath,
                    configuration.OutputPath,
                    configuration.TargetType,
                    configuration.Minify);

                CruncherOptions options = new CruncherOptions
                                              {
                                                  Minify = configuration.Minify,
                                                  AllowRemoteFiles = true,
                                                  RootFolder = Path.GetDirectoryName(actualInputPath)
                                              };

                string crunched;

                if (configuration.TargetType == CrunchTargetType.CSS)
                {
                    CssCruncher cssCruncher = new CssCruncher(options);
                    crunched = cssCruncher.Crunch(actualInputPath);
                }
                else
                {
                    JavaScriptCruncher javaScriptCruncher = new JavaScriptCruncher(options);
                    crunched = javaScriptCruncher.Crunch(actualInputPath);
                }

                FileHelper.WriteFile(outputPath, crunched);
            }
        }

        /// <summary>
        /// Gets the absolute path to save the output to.
        /// </summary>
        /// <param name="rootPath">
        /// The root Path.
        /// </param>
        /// <param name="inputPath">
        /// The input path to the file or folder.
        /// </param>
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        /// <param name="targetType">
        /// The target type of the file or folder.
        /// </param>
        /// <param name="minify">
        /// Whether the output should be minified.
        /// </param>
        /// <returns>
        /// The <see cref="T:System.String"/> representing the correct output path.
        /// </returns>
        private static string GetOutPutPath(string rootPath, string inputPath, string outputPath, CrunchTargetType targetType, bool minify)
        {
            string min = minify ? ".min" : string.Empty;
            string extension = targetType == CrunchTargetType.CSS ? ".css" : ".js";

            // Remove .min if present from the input path name.
            inputPath = inputPath.Replace(".min.css", ".css");
            inputPath = inputPath.Replace(".min.js", ".js");
            inputPath = inputPath.Replace(".min.less", ".less");

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                if (Path.HasExtension(inputPath))
                {
                    return Path.GetFullPath(string.Format("{0}{1}{2}", inputPath.Substring(0, inputPath.LastIndexOf('.')), min, extension));
                }

                return Path.GetFullPath(string.Format("{0}{1}{2}", Path.GetDirectoryName(inputPath), min, extension));
            }

            // Remove .min if present from the output path name.
            outputPath = outputPath.Replace(".min.css", ".css");
            outputPath = outputPath.Replace(".min.js", ".js");
            outputPath = outputPath.Replace(".min.less", ".less");

            if (Path.IsPathRooted(outputPath) && Path.HasExtension(outputPath))
            {
                return Path.GetFullPath(string.Format("{0}{1}{2}", outputPath.Substring(0, outputPath.LastIndexOf('.')), min, extension));
            }

            if (Path.HasExtension(outputPath))
            {
                outputPath = outputPath.Substring(0, outputPath.LastIndexOf('.'));
            }

            return Path.GetFullPath(Path.Combine(rootPath, string.Format("{0}{1}{2}", outputPath, min, extension)));
        }
    }
}
