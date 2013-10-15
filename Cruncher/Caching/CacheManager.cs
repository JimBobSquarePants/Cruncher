// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CacheManager.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods that allow the caching and retrieval of objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Caching
{
    #region Using
    using System.Runtime.Caching;
    #endregion

    /// <summary>
    /// Encapsulates methods that allow the caching and retrieval of objects.
    /// </summary>
    public static class CacheManager
    {
        #region Fields
        /// <summary>
        /// The cache
        /// </summary>
        private static readonly ObjectCache Cache = MemoryCache.Default;
        #endregion

        #region Methods
        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">
        /// A unique identifier for the cache entry.
        /// </param>
        /// <param name="value">
        /// The object to insert.
        /// </param>
        /// <param name="policy">
        /// Optional. An <see cref="T:System.Runtime.Caching.CacheItemPolicy"/> object that contains eviction details for the cache entry. This object
        /// provides more options for eviction than a simple absolute expiration. The default value for the optional parameter
        /// is null.
        /// </param>
        /// <param name="regionName">
        /// Optional. A named region in the cache to which the cache entry can be added,
        /// if regions are implemented. The default value for the optional parameter
        /// is null.
        /// </param>
        /// <returns>
        /// True if the insertion try succeeds, or false if there is an already an entry
        ///  in the cache with the same key as key.
        /// </returns>
        public static bool AddItem(string key, object value, CacheItemPolicy policy = null, string regionName = null)
        {
            if (policy == null)
            {
                // Create a new cache policy with the default values 
                policy = new CacheItemPolicy();
            }

            return Cache.Add(key, value, policy, regionName);
        }

        /// <summary>
        /// Fetches an item matching the given key from the cache.
        /// </summary>
        /// <param name="key">
        /// A unique identifier for the cache entry.
        /// </param>
        /// <param name="regionName">
        /// Optional. A named region in the cache to which the cache entry can be added,
        /// if regions are implemented. The default value for the optional parameter
        /// is null.
        /// </param>
        /// <returns>
        /// The cache entry that is identified by key.
        /// </returns>
        public static object GetItem(string key, string regionName = null)
        {
            return Cache.Get(key, regionName);
        }

        /// <summary>
        /// Updates an item to the cache.
        /// </summary>
        /// <param name="key">
        /// A unique identifier for the cache entry.
        /// </param>
        /// <param name="value">
        /// The object to insert.
        /// </param>
        /// <param name="policy">
        /// Optional. An <see cref="T:System.Runtime.Caching.CacheItemPolicy"/> object that contains eviction details for the cache entry. This object
        /// provides more options for eviction than a simple absolute expiration. The default value for the optional parameter
        /// is null.
        /// </param>
        /// <param name="regionName">
        /// Optional. A named region in the cache to which the cache entry can be added,
        /// if regions are implemented. The default value for the optional parameter
        /// is null.
        /// </param>
        /// <returns>
        /// True if the update try succeeds, or false if there is an already an entry
        ///  in the cache with the same key as key.
        /// </returns>
        public static bool UpdateItem(string key, object value, CacheItemPolicy policy = null, string regionName = null)
        {
            // Remove the item from the cache if it already exists. MemoryCache will
            // not add an item with an existing name.
            if (GetItem(key, regionName) != null)
            {
                RemoveItem(key, regionName);
            }

            if (policy == null)
            {
                // Create a new cache policy with the default values 
                policy = new CacheItemPolicy();
            }

            return Cache.Add(key, value, policy, regionName);
        }

        /// <summary>
        /// Removes an item matching the given key from the cache.
        /// </summary>
        /// <param name="key">
        /// A unique identifier for the cache entry.
        /// </param>
        /// <param name="regionName">
        /// Optional. A named region in the cache to which the cache entry can be added,
        /// if regions are implemented. The default value for the optional parameter
        /// is null.
        /// </param>
        /// <returns>
        /// True if the removal try succeeds, or false if there is an already an entry
        ///  in the cache with the same key as key.
        /// </returns>
        public static bool RemoveItem(string key, string regionName = null)
        {
            return Cache.Remove(key, regionName) != null;
        }
        #endregion
    }
}