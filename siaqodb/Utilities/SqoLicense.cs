using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Exceptions;

namespace Sqo.Utilities
{
    class SqoLicense
    {
        private static bool? valid = null;
        internal static bool LicenseValid(string licenseKey)
        {
            //temp BETA,remove afterwards
            if (DateTime.Now <= new DateTime(2014, 02, 12))
            {
                valid = true;
            }

            if (valid!=null)
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
                int lcMode;
                bool notTrial = int.TryParse(keyValues[0], out lcMode);
                if (notTrial && lcMode == 1)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                    #if !SL4 && !WinRT && !WP7
                        if (keyValues[2] != Environment.MachineName)
                        {
                            throw new InvalidLicenseException("License is generated from another machine, you have to use current machine to generate the license key!");
                        }
                    #endif
                    }
                    string licType = keyValues[3];
                    int vers = Convert.ToInt32(keyValues[4]);
                    if (vers < SiaqodbConfigurator.CurrentVersion)
                    {
                        throw new InvalidLicenseException("Your license is not valid for this version, you have to purchase the upgrade to use this version!");
                    }
                    if (MatchLicType(licType))
                    {
                        valid = true;
                        return true;
                    }
                    else
                    {
                        throw new InvalidLicenseException("License not valid for this platform!");
                  
                    }

                }
                else if (keyValues[0].Contains("@"))
                {
                    int year = Convert.ToInt32(keyValues[1].Substring(0, 4));
                    int month = Convert.ToInt32(keyValues[1].Substring(4, 2));
                    int day = Convert.ToInt32(keyValues[1].Substring(6, 2));
                    DateTime trialExpiredDate = new DateTime(year, month, day);
                    trialExpiredDate = trialExpiredDate.AddDays(30);
                    if (DateTime.Now > trialExpiredDate)
                    {
                        throw new InvalidLicenseException("Trial expired, visit http://siaqodb.com to buy a license");
                    }
                    else
                    {
                        valid = true;
                        return true;
                    }
                }

                valid = false;
                return false;
            }
            catch (InvalidLicenseException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private static bool MatchLicType(string licType)
        {

#if WP7
       if (licType == "ALL" || licType == "BOTH" || licType == "WP")
                return true;
#elif SL4
       if (licType == "ALL" || licType == "BOTH" || licType == "SL" || licType == "SILV")
                return true;
#elif XIOS
       if (licType == "ALL" || licType == "MT")
                return true;
#elif MONODROID
       if (licType == "ALL" || licType == "MD")
                return true;
#elif UNITY3D
       if (licType == "ALL" || licType == "U3D")
                return true;
#elif WinRT
       if (licType == "ALL" || licType == "WinRT")
                return true;
#elif CF
       if (licType == "ALL" || licType == "WM")
                return true;
#else
            if (licType == "ALL" || licType == "BOTH" || licType == "NET")
                return true;
#endif
            return false;
        }
        internal static bool LicenseValid()
        {
            //temp BETA,remove afterwards
            if (DateTime.Now <= new DateTime(2014, 02, 12))
            {
                valid = true;
            }

            if (valid.HasValue)
            {
                if (valid.Value)
                {
                    return true;
                }

            }
            throw new InvalidLicenseException("License not valid!");
        }
    }
}
