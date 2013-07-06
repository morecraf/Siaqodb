using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace LicenseActivationMac
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Put CustomerCode as argument!");
            }
            else
            {
                try
                {
                    string li = GetLicenseKey(args[0]);
                    Console.WriteLine("Your license key:"+li);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        public static string GetLicenseKey(string customerCode)
        {
            string hostname = Environment.MachineName;
            WebRequest request = WebRequest.Create(@"http://siaqodb.com/licensor/licensor?c=" + customerCode + "&m=" + hostname + "&l=1");
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            return responseFromServer;
        }
    }
}
