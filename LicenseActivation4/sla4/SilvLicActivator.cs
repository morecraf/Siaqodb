using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace sla4
{
    public class SilvLicActivator
    {
        public static string GetLicenseKey(string customerCode)
        {
            string hostname=Environment.MachineName;
            WebRequest request = WebRequest.Create(@"http://siaqodb.com/licensor/licensor?c="+customerCode+"&m="+hostname+"&l=1");
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
        public static string GetSilverlightLicenseKey(string assGuid, string assName, string customerCode)
        {
            string machineName = Environment.MachineName;
            WebRequest request = WebRequest.Create(@"http://siaqodb.com/licensor/licensor?c=" + customerCode + "&m=" + machineName + "&ag=" + assGuid + "&an=" + assName + "&l=2");
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
