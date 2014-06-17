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
        internal static bool hasSync;
        internal static bool hasAMSSync;
        internal static bool isStarterEdition;
        internal static bool LicenseValid(string licenseKey)
        {
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
#if !CF
                bool notTrial = int.TryParse(keyValues[0], out lcMode);
#else
                bool notTrial=false;
                try { lcMode = Convert.ToInt32(keyValues[0]); notTrial = true; }
                finally { }
#endif
                if (notTrial && lcMode == 1)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
#if !SL4 && !WinRT && !WP7 && !UNITY3D && !MONODROID && !CF
                        if (keyValues[2] != Environment.MachineName)
                        {
                            throw new InvalidLicenseException("License is generated from another machine, you have to use current machine to generate the license key!");
                        }
#endif
                    }
                    string licType = keyValues[3];
                    int vers = Convert.ToInt32(keyValues[4]);
                    int year = Convert.ToInt32(keyValues[5].Substring(0, 4));
                    int month = Convert.ToInt32(keyValues[5].Substring(4, 2));
                    int day = Convert.ToInt32(keyValues[5].Substring(6, 2));
                    hasSync = keyValues[6] == "1";
                    hasAMSSync = keyValues[7] == "1";

                    DateTime expiredSubscriptionDate = new DateTime(year+1, month, day);
                    if (expiredSubscriptionDate<SiaqodbConfigurator.CurrentVersion.Value)
                    {
                        valid = false;
                        throw new InvalidLicenseException("Your license is not valid for this version, you have to renew your subscription to use this version!");
                    }
                    if (MatchLicType(licType))
                    {
                        valid = true;
                        return true;
                    }
                    else
                    {
                        valid = false;
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
                        valid = false;
                        throw new InvalidLicenseException("Trial expired, visit http://siaqodb.com to buy a license");
                    }
                    else
                    {
                        valid = true;
                        hasAMSSync = true;
                        hasSync = true;
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
                valid = false;
                throw new InvalidLicenseException("License not valid!");
            }

        }

        private static bool MatchLicType(string licType)
        {

#if WP7
       if (licType.Contains("ALL") || licType.Contains("BOTH") || licType.Contains("WP"))
                return true;
#elif SL4
       if (licType.Contains("ALL") || licType.Contains("BOTH") || licType.Contains("SL") || licType.Contains("SILV"))
                return true;
#elif XIOS
       if (licType.Contains("ALL") || licType.Contains("MT"))
                return true;
#elif MONODROID
       if (licType.Contains("ALL") || licType.Contains("MD"))
                return true;
#elif UNITY3D
       if (licType.Contains("ALL") || licType.Contains("U3D"))
                return true;
#elif WinRT
       if (licType.Contains("ALL") || licType.Contains("WinRT"))
                return true;
#elif CF
       if (licType.Contains( "ALL") || licType.Contains("WM"))
                return true;
#else
            if (licType.Contains("ALL") || licType.Contains("BOTH") || licType.Contains("NET"))
                return true;
#endif
            return false;
        }
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
            else
            {
                isStarterEdition = true;
                return true;
            }
            
        }
    }
}
