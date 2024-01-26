using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("MCRA.Data.Raw.Test")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("MCRA.Data.Raw.Test")]
[assembly: AssemblyCopyright("Copyright © Wageningen University and Research 2024")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: Guid("08b6aa1b-b187-47ce-8327-881721821cfc")]

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
