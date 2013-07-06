using System;


namespace SiaqodbSyncProvider.Utilities
{
    class TrialLicense
    {
        private static bool? valid = null;
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
                    throw new Exception("License not valid!");
                }
            }
            try
            {
                string sKy = "lkikwfq_j8KLp@sE";
                string sIV = "74W95wh%YL:2$*1C";
                string key = Utilities.Decryptor.DecryptRJ128(sKy, sIV, licenseKey);
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
                        throw new Exception("Trial expired, visit http://siaqodb.com to buy a license");
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
            throw new Exception("License not valid!");
        }
    }
}
