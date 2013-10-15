// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CruncherOptions.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   The cruncher options.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher
{
    /// <summary>
    /// The cruncher options.
    /// </summary>
    public class CruncherOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to minify the file.
        /// </summary>
        public bool Minify { get; set; }

        /// <summary>
        /// Gets or sets the minify cache key.
        /// </summary>
        public string MinifyCacheKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cache files.
        /// </summary>
        public bool CacheFiles { get; set; }

        /// <summary>
        /// Gets or sets the cache length in days.
        /// </summary>
        public int CacheLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow remote files.
        /// </summary>
        public bool AllowRemoteFiles { get; set; }

        /// <summary>
        /// Gets or sets the remote file timeout in milliseconds.
        /// Set to 0 for no limit.
        /// </summary>
        public int RemoteFileTimeout { get; set; }

        /// <summary>
        /// Gets or sets the remote file max file size in bytes.
        /// Set to 0 for no limit.
        /// </summary>
        public int RemoteFileMaxBytes { get; set; }

        /// <summary>
        /// Gets or sets the root folder.
        /// </summary>
        public string RootFolder { get; set; }
    }
}
