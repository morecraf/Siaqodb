using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Exceptions;

namespace Sqo.Utilities
{
    class SqoUnity3DLic
    {
         private static bool? valid = null;
         internal static bool LicenseValid(string licenseKey)
         {
#if UNITY3D
            // valid = true;
             //return true;
#endif
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
                 string key = Sqo.Utilities.Decryptor.DecryptRJ128(sKy, sIV, licenseKey);
                 string[] keyValues = key.Split('|');
                 if (!string.IsNullOrEmpty(keyValues[1]))
                 {
                     if (keyValues[0] == "1")
                     {
                         valid = true;
                         return true;
                     }
                 }

                 valid = false;
                 return false;
             }

             catch (Exception ex)
             {
                 return false;
             }

         }
         internal static bool LicenseValid()
         {
#if UNITY3D && STORE
             valid = true;
             return true;
#endif
             if (valid.HasValue)
             {
                 if (valid.Value)
                 {
                     return true;
                 }
                 
             }
             throw new InvalidLicenseException("License not valid!");
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
