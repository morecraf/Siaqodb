using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InitCouchNode
{
    class Logger
    {
        public static void Log(string message, string logFile)
        {
            try
            {
                using (var file = new System.IO.StreamWriter(logFile, true))
                {
                    file.WriteLine(message);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

}
