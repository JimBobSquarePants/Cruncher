// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PreprocessorManager.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The cruncher configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Preprocessors
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// The cruncher configuration.
    /// </summary>
    public class PreprocessorManager
    {
        #region Fields
        /// <summary>
        /// A new instance Initializes a new instance of the <see cref="T:Cruncher.Preprocessors.PreprocessorManager"/> class.
        /// initialized lazily.
        /// </summary>
        private static readonly Lazy<PreprocessorManager> Lazy =
                        new Lazy<PreprocessorManager>(() => new PreprocessorManager());
        #endregion

        #region Constructors
        /// <summary>
        /// Prevents a default instance of the <see cref="T:Cruncher.Preprocessors.PreprocessorManager"/> class from being created.
        /// </summary>
        private PreprocessorManager()
        {
            this.LoadPreprocessors();
            this.CreateAllowedExtensionRegex();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current instance of the <see cref="T:Cruncher.Preprocessors.PreprocessorManager"/> class.
        /// </summary>
        public static PreprocessorManager Instance
        {
            get
            {
                return Lazy.Value;
            }
        }

        /// <summary>
        /// Gets the list of available Preprocessors.
        /// </summary>
        public IList<IPreprocessor> PreProcessors { get; private set; }

        /// <summary>
        /// Gets the regular expression for matching allowed file type.
        /// </summary>
        public Regex AllowedExtensionsRegex { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the list of available Preprocessors.
        /// </summary>
        private void LoadPreprocessors()
        {
            if (this.PreProcessors == null)
            {
                // Build a list of native IPreprocessors instances.
                Type type = typeof(IPreprocessor);
                IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                    .ToList();

                // Create them and add.
                this.PreProcessors = types.Select(x => (Activator.CreateInstance(x) as IPreprocessor))
                    .ToList();
            }
        }

        /// <summary>
        /// Generates a Regex with a list of allowed file type extensions.
        /// </summary>
        private void CreateAllowedExtensionRegex()
        {
            StringBuilder stringBuilder = new StringBuilder(@"\.CSS|\.JS|");

            foreach (IPreprocessor preprocessor in this.PreProcessors)
            {
                string[] extensions = preprocessor.AllowedExtensions;

                if (extensions != null)
                {
                    foreach (string extension in extensions)
                    {
                        stringBuilder.AppendFormat(@"\{0}|", extension.ToUpperInvariant());
                    }
                }
            }

            this.AllowedExtensionsRegex = new Regex(stringBuilder.ToString().TrimEnd('|'), RegexOptions.IgnoreCase);
        }
        #endregion
    }
}
