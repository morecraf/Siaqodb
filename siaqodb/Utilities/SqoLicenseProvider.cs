#if TRIAL
#else

using System.Runtime.Remoting;
using System.ComponentModel.Design;
using System.Diagnostics;
using System;
using System.Reflection;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Sqo.Exceptions;

namespace Sqo
{
    internal class SqoLicenseProvider : LicenseProvider
    {
        /// <include file='doc\LicFileLicenseProvider.uex' path='docs/doc[@for="LicFileLicenseProvider.IsKeyValid"]/*' />
        /// <devdoc>
        /// <para>Determines if the key retrieved by the <see cref='System.ComponentModel.LicFileLicenseProvider.GetLicense'/> method is valid 
        ///    for the specified type.</para>
        /// </devdoc>
        protected virtual bool IsKeyValid(string key, Type type)
        {
            // If string isn't empty
            if (key != null)
            {
                //return ( key.StartsWith(string.Format("{0} is a licensed component.", type.FullName)));
                try
                {
                    string[] keyValues = key.Split('|');
                    if (!string.IsNullOrEmpty(keyValues[1]))
                    {
                        if (keyValues[0] == "1")
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    
                }
               
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        /// <include file='doc\LicFileLicenseProvider.uex' path='docs/doc[@for="LicFileLicenseProvider.GetLicense"]/*' />
        /// <devdoc>
        ///    <para>Gets a license for the instance of the component and determines if it is valid.</para>
        /// </devdoc>
        public override License GetLicense(LicenseContext context, Type type, object instance, bool allowExceptions)
        {
            SqoLicense lic = null;

            Debug.Assert(context != null, "No context provided!");
            //if no context is provided, do nothing
            if (context != null)
            {
                //if this control is in runtime mode
                if (context.UsageMode == LicenseUsageMode.Runtime)
                {
                    //retreive the stored license key
                    string key = context.GetSavedLicenseKey(type, null);

                    //check if the stored license key is null
                    // and call IsKeyValid to make sure its valid
                    if (key != null && IsKeyValid(key, type))
                    {
                        //if the key is valid create a new license
                        lic = new SqoLicense(this, key);
                    }
                }

                //if we're in design mode or 
                //a suitable license key wasn't found in 
                //the runtime context.
                //attempt to look for a .LIC file
                if (lic == null)
                {
                    //build up the path where the .LIC file
                    //should be
                    string modulePath = null;

                    // try and locate the file for the assembly
                    if (context != null)
                    {
                        ITypeResolutionService resolver = (ITypeResolutionService)context.GetService(typeof(ITypeResolutionService));
                        if (resolver != null)
                            modulePath = resolver.GetPathOfAssembly(type.Assembly.GetName());
                    }


                    if (modulePath == null)
                        modulePath = type.Module.FullyQualifiedName;

                    //get the path from the file location
                    string moduleDir = Path.GetDirectoryName(modulePath);

                    //build the path of the .LIC file
                    string licenseFile = moduleDir + "\\siaqodb.lic";

                   // Debug.WriteLine("Path of license file: " + licenseFile);

                    //if the .LIC file exists, dig into it
                    if (File.Exists(licenseFile))
                    {
                        //crack the file and get the first line
                        Stream licStream = new FileStream(licenseFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                        StreamReader sr = new StreamReader(licStream);
                        string s = sr.ReadToEnd();
                        sr.Close();
                        string keyD = "";
                        try
                        {
                            string sKy = "lkikwfq_j8KLp@sE";
                            string sIV = "74W95wh%YL:2$*1C";
                            keyD = Sqo.Utilities.Decryptor.DecryptRJ128(sKy, sIV, s);
                            string[] keyValues = keyD.Split('|');

                            if (keyValues[2] ==Environment.MachineName)
                            {
                               
                            }
                            else
                            {
                                throw new InvalidLicenseException("Invalid license");
                            }

                        }
                        catch
                        {
                            return null;
                        }
                        //Debug.WriteLine("Contents of license file: " + keyD);

                        //check if the key is valid
                        if (IsKeyValid(keyD, type))
                        {
                            //valid key so create a new License
                            lic = new SqoLicense(this, keyD);
                        }
                    }

                    //if we managed to create a license, stuff it into the context
                    if (lic != null)
                    {
                        context.SetSavedLicenseKey(type, lic.LicenseKey);
                    }
                }

            }
            return lic;
        }
        
    

        internal class SqoLicense : License
        {
            private SqoLicenseProvider owner;
            private string key;
            //int validProcCount;
            public SqoLicense(SqoLicenseProvider owner, string key)
            {
                this.owner = owner;
                this.key = key;
                //this.validProcCount = Int32.Parse(key.Substring(key.IndexOf(',') + 1));
            }
            public override string LicenseKey
            {
                get
                {
                    return key;
                }
            }



            public override void Dispose()
            {
            }
        }
        


    }
}


#endif

