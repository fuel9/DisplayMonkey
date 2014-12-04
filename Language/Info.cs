using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace DisplayMonkey.Language
{
    public static class Info
    {
        private static CultureInfo[] _cultures = null;
        private static CultureInfo[] _supportedCultures = null;
        private static object _lock = new object();
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
    }
}
