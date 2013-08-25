using System;
using System.Reflection;
using Sqo.Exceptions;
using System.IO;

using System.Text;



namespace Sqo
{
    internal static class WinRTLicenseChecker
    {
        private static bool? valid = null;
        internal static bool LicenseValid()
        {

            if (valid.HasValue)
            {
                if (valid.Value)
                {
                    return true;
                }
                else
                {
                    throw new InvalidLicenseException("License not valid!");
                }
            }
            Assembly r = Windows.UI.Xaml.Application.Current.GetType().GetTypeInfo().Assembly;
            string[] resources = r.GetManifestResourceNames();
            string sqoLic = "";
            foreach (string res in resources)
            {
                if (res.Contains("siaqodb.lic"))
                {
                    sqoLic = res;
                }
            }
            if (string.IsNullOrEmpty(sqoLic))
            {
                throw new InvalidLicenseException("License file not found!");
            }
            else
            {
                try
                {
                    Stream stream = r.GetManifestResourceStream(sqoLic);
                    string key = "";
                    using (TextReader tr = new StreamReader(stream))
                    {
                        key = tr.ReadToEnd();
                    }
                    string sKy = "lkikwfq_j8KLp@sE";
                    string sIV = "74W95wh%YL:2$*1C";
                    string keyD = Sqo.Utilities.Decryptor.DecryptRJ128(sKy, sIV, key);
                    string[] keyValues = keyD.Split('|');

                    if (keyValues.Length > 2)
                    {
                        valid = true;
                        return true;
                    }
                    else
                    {
                        throw new InvalidLicenseException("License not valid!");
                    }
                    //Encoding.

                }
                catch
                {
                    throw new InvalidLicenseException("License not valid!");
                }
            }

        }
        internal static bool LicenseValid(string licenseKey)
        {

            if (valid.HasValue)
            {
                if (valid.Value)
                {
                    return true;
                }
                else
                {
                    throw new InvalidLicenseException("License not valid!");
                }
            }

            try
            {
               string sKy = "lkikwfq_j8KLp@sE";
                string sIV = "74W95wh%YL:2$*1C";
                string keyD = Sqo.Utilities.Decryptor.DecryptRJ128(sKy, sIV, licenseKey);
                string[] keyValues = keyD.Split('|');

                if (keyValues.Length > 2)
                {
                    valid = true;
                    return true;
                }
                else
                {
                    throw new InvalidLicenseException("License not valid!");
                }
                //Encoding.

            }
            catch
            {
                throw new InvalidLicenseException("License not valid!");
            }




        }
    }
}
