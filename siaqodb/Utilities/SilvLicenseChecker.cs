using System;
using System.Reflection;
using Sqo.Exceptions;
using System.IO;
using System.Security.Cryptography;
using System.Text;



namespace Sqo
{
    internal static class SilvLicenseChecker
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
            Assembly r = System.Windows.Application.Current.GetType().Assembly;
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
                    object[] at = r.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
                    string guid = "";
                    if (at.Length > 0)
                    {
                        guid = ((System.Runtime.InteropServices.GuidAttribute)at[0]).Value;
                    }
                    if (keyValues[4] == r.FullName.Split(',')[0] && keyValues[3]==guid)
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

        internal static bool LicenseValid(string key)
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
                string keyD = Sqo.Utilities.Decryptor.DecryptRJ128(sKy, sIV, key);
                string[] keyValues = keyD.Split('|');

                if (keyValues.Length > 3)
                {
                    valid = true;
                    return true;
                }
                else
                {
                    throw new InvalidLicenseException("License not valid!");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidLicenseException("License not valid!");
            }
        }
        internal static bool LicenseValidManager(string p)
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
            if (p == "SiaqodbManager,SiaqodbManager2")
            {
                valid = true;
            }
            else
            {
                throw new InvalidLicenseException("License not valid!");
            }
            return valid.Value;
        }
       
    }
}
