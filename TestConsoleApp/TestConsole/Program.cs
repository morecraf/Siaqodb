using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sqo;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Sqo.SiaqodbConfigurator.SetLicense("OTwzBeiZoNOuweLgH4OsNHl6DHpwB2txl47RsC30Gos=");
            Siaqodb db = new Siaqodb(Directory.GetCurrentDirectory());

        }
    }
}
