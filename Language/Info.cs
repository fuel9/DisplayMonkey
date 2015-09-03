/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Web;

namespace DisplayMonkey.Language
{
    public static class Info
    {
        #region Private members

        private static CultureInfo[] _cultures = null;
        private static CultureInfo[] _supportedCultures = null;
        private static object _lock = new object();
        
        #endregion

        public static CultureInfo[] SupportedCultures 
        {
            get 
            {
                lock (_lock)
                {
                    if (_supportedCultures == null)
                    {
                        ResourceManager rm = new ResourceManager(typeof(Resources));
                        _supportedCultures = Info.AllCultures
                            .Where(c => rm.GetResourceSet(c, true, false) != null)
                            .OrderBy(c => c.Name)
                            .ToArray()
                            ;
                    }

                    return _supportedCultures;
                }
            } 
        }
        public static bool CultureIsSupported(CultureInfo culture)
        {
            return SupportedCultures.FirstOrDefault(c => c == culture) != null;
        }
        public static CultureInfo[] AllCultures
        {
            get
            {
                lock (_lock)
                {
                    if (_cultures == null)
                    {
                        _cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                            .Where(c => c.Name != "")
                            .OrderBy(c => c.Name)
                            .ToArray()
                            ;
                    }

                    return _cultures;
                }
            }
        }
        public static Uri HelpUri(HttpRequest request, string site)
        {
            CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentUICulture;
            string 
                appPath = request.ApplicationPath, 
                relPath = request.Path,
                culture = CultureIsSupported(cultureInfo) ? cultureInfo.TwoLetterISOLanguageName : "en"
                ;
            if (appPath != "/")
            {
                relPath = relPath.Replace(appPath, "");
            }
            int q = relPath.IndexOfAny(new char[] {'?', '.'}); if (q >= 0) { relPath = relPath.Substring(0, q); }
            string[] segs = relPath.Split('/'); relPath = "";
            for (int i = 1, j = Math.Min(segs.Length, 3); i < j; i++)
            {
                if (segs[i] != "") { relPath += (relPath == "" ? "" : "/") + segs[i]; }
            }
            //string help = "~/help" + relPath;
            string help = string.Format(
                "http://www.displaymonkey.org/dm/documentation/{0}/{1}/{2}/{3}",
                Resources.HelpVersion,
                culture,
                site,
                relPath
                );
            return new Uri(help);
        }

        /*public static List<TimeZoneInfo> getTimeZoneInfoList()
        {
            return TimeZoneInfo.GetSystemTimeZones().ToList();
        }*/
    }
}
