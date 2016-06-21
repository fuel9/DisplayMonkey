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
using System.Linq;
using System.Web;

using System.Threading;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Resources;

namespace System.Web.Mvc
{
    public static class LocalizationHelpers
    {
        public static SelectList TranslatedSelectList<T>(this Nullable<T> enumObj, bool valueAsText = false)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            ResourceManager rm = new ResourceManager(typeof(DisplayMonkey.Language.Resources));

            var items = (from T e in Enum.GetValues(typeof(T))
                        select new SelectListItem
                        {
                            Value = valueAsText ? e.ToString() : e.ToInt32(null).ToString(),
                            //Text = e.TranslatedDescription(),
                            Text = rm.GetString(e.ToString()),
                            //Selected = (!enumObj.Equals(null) && e.Equals(enumObj))
                        })
                        .OrderBy(i => i.Text)
                        ;
            return new SelectList(items, "Value", "Text", enumObj);
        }
    }
}

namespace DisplayMonkey.Models
{
    public static class LocalizationHelpers
    {
        public static string Translate<T>(this T enumObj)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            ResourceManager rm = new ResourceManager(typeof(DisplayMonkey.Language.Resources));
            return rm.GetString(enumObj.ToString());
        }

        public static string Translate<T>(this Nullable<T> enumObj)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            return enumObj == null ? "" : enumObj.Value.Translate();
        }

        /* this doesn't work...
        public static string TranslatedDescription<TEnum>(this TEnum en)
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            Type type = en.GetType();

            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DisplayAttribute)attrs[0]).Description;
            }

            return en.ToString();
        }*/
    }
}

namespace DisplayMonkey
{
    public static class CultureHelpers
    {
        // Valid cultures
        private static readonly List<string> _validCultures = new List<string> { "af", "af-ZA", "sq", "sq-AL", "gsw-FR", "am-ET", "ar", "ar-DZ", "ar-BH", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB", "ar-LY", "ar-MA", "ar-OM", "ar-QA", "ar-SA", "ar-SY", "ar-TN", "ar-AE", "ar-YE", "hy", "hy-AM", "as-IN", "az", "az-Cyrl-AZ", "az-Latn-AZ", "ba-RU", "eu", "eu-ES", "be", "be-BY", "bn-BD", "bn-IN", "bs-Cyrl-BA", "bs-Latn-BA", "br-FR", "bg", "bg-BG", "ca", "ca-ES", "zh-HK", "zh-MO", "zh-CN", "zh-Hans", "zh-SG", "zh-TW", "zh-Hant", "co-FR", "hr", "hr-HR", "hr-BA", "cs", "cs-CZ", "da", "da-DK", "prs-AF", "div", "div-MV", "nl", "nl-BE", "nl-NL", "en", "en-AU", "en-BZ", "en-CA", "en-029", "en-IN", "en-IE", "en-JM", "en-MY", "en-NZ", "en-PH", "en-SG", "en-ZA", "en-TT", "en-GB", "en-US", "en-ZW", "et", "et-EE", "fo", "fo-FO", "fil-PH", "fi", "fi-FI", "fr", "fr-BE", "fr-CA", "fr-FR", "fr-LU", "fr-MC", "fr-CH", "fy-NL", "gl", "gl-ES", "ka", "ka-GE", "de", "de-AT", "de-DE", "de-LI", "de-LU", "de-CH", "el", "el-GR", "kl-GL", "gu", "gu-IN", "ha-Latn-NG", "he", "he-IL", "hi", "hi-IN", "hu", "hu-HU", "is", "is-IS", "ig-NG", "id", "id-ID", "iu-Latn-CA", "iu-Cans-CA", "ga-IE", "xh-ZA", "zu-ZA", "it", "it-IT", "it-CH", "ja", "ja-JP", "kn", "kn-IN", "kk", "kk-KZ", "km-KH", "qut-GT", "rw-RW", "sw", "sw-KE", "kok", "kok-IN", "ko", "ko-KR", "ky", "ky-KG", "lo-LA", "lv", "lv-LV", "lt", "lt-LT", "wee-DE", "lb-LU", "mk", "mk-MK", "ms", "ms-BN", "ms-MY", "ml-IN", "mt-MT", "mi-NZ", "arn-CL", "mr", "mr-IN", "moh-CA", "mn", "mn-MN", "mn-Mong-CN", "ne-NP", "no", "nb-NO", "nn-NO", "oc-FR", "or-IN", "ps-AF", "fa", "fa-IR", "pl", "pl-PL", "pt", "pt-BR", "pt-PT", "pa", "pa-IN", "quz-BO", "quz-EC", "quz-PE", "ro", "ro-RO", "rm-CH", "ru", "ru-RU", "smn-FI", "smj-NO", "smj-SE", "se-FI", "se-NO", "se-SE", "sms-FI", "sma-NO", "sma-SE", "sa", "sa-IN", "sr", "sr-Cyrl-BA", "sr-Cyrl-SP", "sr-Latn-BA", "sr-Latn-SP", "nso-ZA", "tn-ZA", "si-LK", "sk", "sk-SK", "sl", "sl-SI", "es", "es-AR", "es-BO", "es-CL", "es-CO", "es-CR", "es-DO", "es-EC", "es-SV", "es-GT", "es-HN", "es-MX", "es-NI", "es-PA", "es-PY", "es-PE", "es-PR", "es-ES", "es-US", "es-UY", "es-VE", "sv", "sv-FI", "sv-SE", "syr", "syr-SY", "tg-Cyrl-TJ", "tzm-Latn-DZ", "ta", "ta-IN", "tt", "tt-RU", "te", "te-IN", "th", "th-TH", "bo-CN", "tr", "tr-TR", "tk-TM", "ug-CN", "uk", "uk-UA", "wen-DE", "ur", "ur-PK", "uz", "uz-Cyrl-UZ", "uz-Latn-UZ", "vi", "vi-VN", "cy-GB", "wo-SN", "sah-RU", "ii-CN", "yo-NG" };

        public static List<string> _supportedCultures = new List<string>();

        // Include ONLY cultures you are implementing
        private static readonly List<string> _cultures = new List<string> {
            "en-US",  // first culture is the DEFAULT
            "es", // Spanish NEUTRAL culture
            "ar"  // Arabic NEUTRAL culture
        };

        /// <summary>
        /// Returns true if the language is a right-to-left language. Otherwise, false.
        /// </summary>
        public static bool IsRighToLeft()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft;
        }

        /// <summary>
        /// Returns true of culture is valid
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static bool IsValid(string culture)
        {
            return _validCultures.Any(c => c.Equals(culture, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool IsSupported(string culture)
        {
            return _supportedCultures.Any(c => c.Equals(culture, StringComparison.InvariantCultureIgnoreCase))
                || _supportedCultures.Any(c => c.StartsWith(GetNeutralCulture(culture), StringComparison.InvariantCultureIgnoreCase));
        }

        public static string GetSupported(string culture)
        {
            if (culture == null || !IsValid(culture))
                return null;

            string name = null;

            // try exact
            name = _supportedCultures
                .Where(c => c.Equals(culture, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault()
                ;
            if (name != null) return name;

            // try neutral culture
            string nc = GetNeutralCulture(culture);
            name = _supportedCultures
                .Where(c => c.StartsWith(nc, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault()
                ;
            return name;
        }


        /// <summary>
        /// Returns a valid culture name based on "name" parameter. If "name" is not valid, it returns the default culture "en-US"
        /// </summary>
        /// <param name="name" />Culture's name (e.g. en-US)</param>
        public static string GetImplementedCulture(string name)
        {
            // make sure it's not null
            if (string.IsNullOrEmpty(name))
                return GetDefaultCulture(); // return Default culture
            // make sure it is a valid culture first
            if (_validCultures.Where(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count() == 0)
                return GetDefaultCulture(); // return Default culture if it is invalid
            // if it is implemented, accept it
            if (_cultures.Where(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count() > 0)
                return name; // accept it
            // Find a close match. For example, if you have "en-US" defined and the user requests "en-GB",
            // the function will return closes match that is "en-US" because at least the language is the same (ie English) 
            var n = GetNeutralCulture(name);
            foreach (var c in _cultures)
                if (c.StartsWith(n))
                    return c;
            // else
            // It is not implemented
            return GetDefaultCulture(); // return Default culture as no match found
        }
        /// <summary>
        /// Returns default culture name which is the first name decalared (e.g. en-US)
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultCulture()
        {
            return _cultures[0]; // return Default culture
        }
        public static string GetCurrentCulture()
        {
            return Thread.CurrentThread.CurrentCulture.Name;
        }
        public static void SetCurrentCulture(string culture)
        {
            if (culture != null)
            {
                Thread.CurrentThread.CurrentCulture =
                    new System.Globalization.CultureInfo(culture)
                    ;
            }
        }
        public static void SetCurrentUICulture(string culture)
        {
            if (culture != null)
            {
                Thread.CurrentThread.CurrentUICulture =
                    new System.Globalization.CultureInfo(culture)
                    ;
            }
        }
        public static string GetCurrentNeutralCulture()
        {
            return GetNeutralCulture(GetCurrentCulture());
        }
        public static string GetNeutralCulture(string name)
        {
            if (name.Length <= 2)
                return name;
            return name.Substring(0, 2); // Read first two chars only. E.g. "en", "es"
        }
    }
}


