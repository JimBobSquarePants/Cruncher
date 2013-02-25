

namespace Cruncher.Extensions
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion

    /// <summary>
    /// Encapsulates a series of time saving extension methods to <see cref="T:System.Uri">Uri</see>s.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static string MakeRelativePathTo(this Uri from, Uri to)
        {
            return from.MakeRelativeUri(to).OriginalString;
        }
    }
}
