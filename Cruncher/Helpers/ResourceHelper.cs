using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web;
using System.IO;

namespace Cruncher.Helpers
{
    class ResourceHelper
    {

        public static string getFilePath(string resource)
        {
            try
            {
                // Check whether this method is invoked in an http request or not
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    // Check whether it is a correct uri
                    if (Uri.IsWellFormedUriString(resource, UriKind.RelativeOrAbsolute))
                    {

                        // If the uri contains a scheme delimiter (://) then try to see if the autority is the same as the currrent request
                        if (resource.Contains(Uri.SchemeDelimiter))
                        {
                            var requestAuthority = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                            if (resource.Trim().StartsWith(requestAuthority, StringComparison.CurrentCultureIgnoreCase))
                            {
                                string path = resource.Substring(requestAuthority.Length);
                                return HttpContext.Current.Server.MapPath(string.Format("~{0}", path));
                            }
                            return resource;
                        }

                        // If it is a relative path then combines the request's path with the resource's path
                        if (!Path.IsPathRooted(resource))
                        {
                            string path = Path.Combine(VirtualPathUtility.GetDirectory(HttpContext.Current.Request.CurrentExecutionFilePath), resource);
                            return HttpContext.Current.Server.MapPath(string.Format("~{0}", path));
                        }
                        else
                        {
                            // It is an absolute path
                            return HttpContext.Current.Server.MapPath(string.Format("~{0}", resource));
                        }
                    }
                }

                // In any other case use the default Path.GetFullPath() method 
                return Path.GetFullPath(resource);
            }
            catch (Exception)
            {
                // If there is an error then the method returns the original resource path since the method doesn't know how to process it
                return resource;
            }
        }

        //public static string Test()
        //{
        //    StringBuilder result = new StringBuilder();
        //    List<string> files = new List<string>()
        //        {
        //            "page.aspx",
        //            "style.css",
        //            "javascript.js",
        //            "without-extension",
        //            "directory/page.aspx",
        //            "/directory/page.aspx",
        //            "domain.com/page.aspx",
        //            "domain.com/directory/page.aspx",
        //            "http://domain.com/directory/page.aspx",
        //            "directory\\file.txt",
        //            "\\directory\\file.txt",
        //            "C:\\directory\\file.txt",
        //            "C:\\file.txt",
        //            "\\\\directory\\file.txt",
        //            "\\\\file.txt"
        //        };
        //    foreach (string file in files)
        //    {
        //        result.AppendFormat("{0} --> {1}{2}", file, getFilePath(file), Environment.NewLine);
        //    }
        //    Console.WriteLine(result.ToString());
        //    return result.ToString();
        //}

    }
}
