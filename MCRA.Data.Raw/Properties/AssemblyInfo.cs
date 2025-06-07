using System.Reflection;
using System.Runtime.InteropServices;
using Git = ThisAssembly.Git;
using Ver = ThisAssembly.Git.BaseVersion;

// Use the Git versioning using the GitInfo NuGet package
// Create a version number like [Major].[Minor].[Patch].[Revision]
// [Revision] is the number of commits since last release version tag
// (for example "v10.1.20.17")
[assembly: AssemblyVersion(
    $"{Ver.Major}.{Ver.Minor}.{Ver.Patch}.{Git.Commits}")]
[assembly: AssemblyFileVersion(
    $"{Ver.Major}.{Ver.Minor}.{Ver.Patch}.{Git.Commits}")]
[assembly: AssemblyInformationalVersion(
    $"{Ver.Major}.{Ver.Minor}.{Ver.Patch}.{Git.Commits}-{Git.Branch}+{Git.Commit}")]

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MCRA.Data.Raw")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("MCRA.Data.Raw")]
[assembly: AssemblyCopyright("Copyright © Wageningen University and Research 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("66840553-1a3d-4e1c-b2c8-02330f35bc81")]
