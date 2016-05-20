using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("siaqodb")]
[assembly: AssemblyDescription("NoSQL embedded database for .NET")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Dotissi")]
[assembly: AssemblyProduct("siaqodb")]
[assembly: AssemblyCopyright("Copyright © Dotissi 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if UNITY3D || CF 
#else
//[assembly: AssemblyKeyName("siaqodb.pfx")]
#endif
// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]


// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6499f830-6a14-4472-9eb7-dabbf0470ceb")]
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("5.5.0.3")]
/*#if XIOS
[assembly: System.Runtime.Versioning.TargetFramework("MonoTouch,Version=v1.0", FrameworkDisplayName="Xamarin.iOS Support")]
#endif
#if MONODROID && !UNITY3D
[assembly: System.Runtime.Versioning.TargetFramework("MonoAndroid,Version=v2.3", FrameworkDisplayName="Xamarin.Android Support")]
#endif*/

