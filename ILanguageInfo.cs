using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMonkey
{
    public class CultureInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameEng { get; set; }
    }

    interface ILanguageSupport
    {
        string [] SupportedCultures();
    }
}
