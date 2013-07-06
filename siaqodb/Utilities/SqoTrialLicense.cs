using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sqo.Exceptions;

namespace Sqo.Utilities
{
    class SqoTrialLicense
    {
        private static bool? valid = null;
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
                if (!string.IsNullOrEmpty(keyValues[1]))
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
        internal static bool LicenseValid()
        {
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
