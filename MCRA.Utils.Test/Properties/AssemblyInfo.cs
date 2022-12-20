using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MCRA.Utils.Test")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("MCRA.Utils.Test")]
[assembly: AssemblyCopyright("Copyright © Wageningen University and Research 2022")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9f9fdd3f-baa7-4c4c-bd96-b60b101ac30f")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// Use the Git versioning using the GitInfo NuGet package
// Create a version number like [Major].[Minor].[Patch].[Revision]
// [Revision] is the number of commits since last release version tag
// (for example "v9.0.36")
[assembly: AssemblyVersion(
    ThisAssembly.Git.BaseVersion.Major + "." +
    ThisAssembly.Git.BaseVersion.Minor + "." +
    ThisAssembly.Git.BaseVersion.Patch + "." +
    ThisAssembly.Git.Commits)]
[assembly: AssemblyFileVersion(
    ThisAssembly.Git.BaseVersion.Major + "." +
    ThisAssembly.Git.BaseVersion.Minor + "." +
    ThisAssembly.Git.BaseVersion.Patch + "." +
    ThisAssembly.Git.Commits)]
[assembly: AssemblyInformationalVersion(
    ThisAssembly.Git.BaseVersion.Major + "." +
    ThisAssembly.Git.BaseVersion.Minor + "." +
    ThisAssembly.Git.BaseVersion.Patch + "." +
    ThisAssembly.Git.Commits + "-" +
    ThisAssembly.Git.Branch + "+" +
    ThisAssembly.Git.Commit)]
// i..e ^: 1.0.2.0-master+c218617
