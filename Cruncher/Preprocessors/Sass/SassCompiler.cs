// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SassCompiler.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The sass compiler.
//   Much thanks here to Paul Betts' SassAndCoffee project (https://github.com/xpaulbettsx/SassAndCoffee)
//   and Justin Etheridge's SquishIt project https://github.com/jetheredge/SquishIt
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors.Sass
{
    #region MyRegion
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using IronRuby;
    using Microsoft.Scripting;
    using Microsoft.Scripting.Hosting;
    #endregion

    /// <summary>
    /// The sass compiler.
    /// Much thanks here to Paul Betts' SassAndCoffee project (https://github.com/xpaulbettsx/SassAndCoffee)
    /// and Justin Etheridge's SquishIt project https://github.com/jetheredge/SquishIt
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class SassCompiler
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SassCompiler"/> class.
        /// </summary>
        /// <param name="rootPath">
        /// The root path.
        /// </param>
        public SassCompiler(string rootPath)
        {
            RootAppPath = rootPath;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The compiler mode.
        /// </summary>
        public enum CompilerMode
        {
            /// <summary>
            /// Processes .sass files.
            /// </summary>
            Sass,

            /// <summary>
            /// Processes .scss files.
            /// </summary>
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
            Scss
        }

        /// <summary>
        /// Gets the root app path.
        /// </summary>
        public static string RootAppPath { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Gets a string containing the compiled sass output.
        /// </summary>
        /// <param name="input">
        /// The input to compile.
        /// </param>
        /// <param name="fileName">
        /// The name of the file to compile.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> containing the compiled sass output.
        /// </returns>
        public string CompileSass(string input, string fileName)
        {
            CompilerMode mode = CompilerMode.Scss;

            if (Regex.IsMatch(fileName, @"\.sass$"))
            {
                mode = CompilerMode.Sass;
            }

            ResourceAwarePlatformAdaptationLayer resourceAwarePlatform = new ResourceAwarePlatformAdaptationLayer();

            ScriptRuntimeSetup srs = new ScriptRuntimeSetup
                                         {
                                             HostType = typeof(ResourceAwareScriptHost),
                                             HostArguments = new List<object>() { resourceAwarePlatform }
                                         };
            srs.AddRubySetup();
            ScriptRuntime runtime = Ruby.CreateRuntime(srs);
            ScriptEngine engine = runtime.GetRubyEngine();

            engine.SetSearchPaths(new List<string> { @"R:\lib\ironruby", @"R:\lib\ruby\1.9.1" });

            string resouce = Utils.ResourceAsString("Cruncher.Preprocessors.Sass.lib.sass_in_one.rb");
            ScriptSource source = engine.CreateScriptSourceFromString(resouce, SourceCodeKind.File);
            ScriptScope scope = engine.CreateScope();

            source.Execute(scope);

            // Change the executing directory.
            string statement = string.Format("Dir.chdir '{0}'", Path.GetDirectoryName(fileName));
            engine.Execute(statement, scope);

            dynamic sassMode = mode == CompilerMode.Sass
                                   ? engine.Execute("{:cache => false, :syntax => :sass}")
                                   : engine.Execute("{:cache => false, :syntax => :scss}");

            dynamic sassEngine = scope.Engine.Runtime.Globals.GetVariable("Sass");

            try
            {
                return (string)sassEngine.compile(input, sassMode);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Sass::SyntaxError")
                {
                    const string Empty = "";
                    dynamic error = ex;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0}\n\n", error.to_s());
                    sb.AppendFormat("Backtrace:\n{0}\n\n", error.sass_backtrace_str(fileName) ?? Empty);
                    sb.AppendFormat("FileName: {0}\n\n", error.sass_filename() ?? fileName);
                    sb.AppendFormat("MixIn: {0}\n\n", error.sass_mixin() ?? Empty);
                    sb.AppendFormat("Line Number: {0}\n\n", error.sass_line() ?? Empty);
                    sb.AppendFormat("Sass Template:\n{0}\n\n", error.sass_template ?? Empty);
                    throw new Exception(sb.ToString(), ex);
                }

                throw ex;
            }
        }
        #endregion
    }
}
