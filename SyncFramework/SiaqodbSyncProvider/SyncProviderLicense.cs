using System;

namespace SiaqodbSyncProvider
{
    public class SyncProviderLicense
    {
        #if TRIAL
        public static void SetTrialLicense(string licenseKey)
        {
           Utilities.TrialLicense.LicenseValid(licenseKey);
        }
        #endif
    }
}
