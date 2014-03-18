using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMonkey.Language
{
    public class Info : ILanguageSupport
    {
        public string [] SupportedCultures()
        {
            // list resources supported by this DLL
            return new string[] {
                "en"
            };
        }
    }
}
