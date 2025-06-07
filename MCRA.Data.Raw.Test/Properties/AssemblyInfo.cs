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

[assembly: AssemblyTitle("MCRA.Data.Raw.Test")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("MCRA.Data.Raw.Test")]
[assembly: AssemblyCopyright("Copyright © Wageningen University and Research 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: Guid("08b6aa1b-b187-47ce-8327-881721821cfc")]
