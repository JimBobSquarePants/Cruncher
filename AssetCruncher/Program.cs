

namespace AssetCruncher
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks; 
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
