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
            valid = true;
            hasSync = true;
            hasAMSSync = true;
            isStarterEdition = false;
            
            return true;
        }

        private static bool MatchLicType(string licType)
        {
            return true;
        }
        internal static bool LicenseValid()
        {
            return true;
        }
    }
}
