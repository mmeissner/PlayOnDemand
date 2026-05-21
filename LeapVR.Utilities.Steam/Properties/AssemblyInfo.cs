using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("LeapVR.Utilities.Steam")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("LeapVR.Utilities.Steam")]
[assembly: AssemblyCopyright("Copyright ©  2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("cb0ff9a9-08a9-4124-afeb-0a68b553a433")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("2018.4.*")]
[assembly: AssemblyFileVersion("2018.4.0.0")]

// Internal helpers (SteamLib.GetSteamLibraryFolders + ResolveLibraryPath) are
// visible to the xunit test project so the VDF-parsing path that crashed the
// kiosk on first start (post-2021 libraryfolders.vdf schema) can be unit-tested
// without widening the public surface.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LeapVR.Utilities.Steam.Test")]
