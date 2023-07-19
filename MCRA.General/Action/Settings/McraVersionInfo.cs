using System.Xml.Serialization;

namespace MCRA.General.Action.Settings.Serialization {
    public class McraVersionInfo {

        /// <summary>
        /// Major version number.
        /// </summary>
        public int Major { get; set; } = 0;

        /// <summary>
        /// Minor version number.
        /// </summary>
        public int Minor { get; set; } = 0;

        /// <summary>
        /// Build number.
        /// </summary>
        public int Build { get; set; } = 0;

        /// <summary>
        /// Revision number.
        /// </summary>
        public int Revision { get; set; } = 0;

        /// <summary>
        /// Specific commit (Git).
        /// </summary>
        public string Commit { get; set; } = null;

        /// <summary>
        /// Revision tag of specific commit.
        /// </summary>
        public string Tag { get; set; } = null;

        /// <summary>
        /// Returns a boolean whether this version is older than the current assembly
        /// version.
        /// </summary>
        [XmlIgnore]
        public bool IsPreviousVersion {
            get {
                var thisMajorVersion = int.Parse(ThisAssembly.Git.BaseVersion.Major);
                var thisMinorVersion = int.Parse(ThisAssembly.Git.BaseVersion.Minor);
                var thisBuildVersion = int.Parse(ThisAssembly.Git.BaseVersion.Patch);
                var older = Major < thisMajorVersion
                         || (Major == thisMajorVersion
                             && (Minor < thisMinorVersion
                                 || (Minor == thisMinorVersion && Build < thisBuildVersion)));
                return older;
            }
        }

        /// <summary>
        /// Sets all members to the current MCRA version
        /// from the static ThisAssembly class.
        /// </summary>
        public void SetCurrentVersionData() {
            //uses the Git.BaseVersion class, this returns
            //the unadjusted latest version tag
            Major = int.Parse(ThisAssembly.Git.BaseVersion.Major);
            Minor = int.Parse(ThisAssembly.Git.BaseVersion.Minor);
            Build = int.Parse(ThisAssembly.Git.BaseVersion.Patch);
            //the number of commits since the base version determines the
            //revision number. This should be 0 for all release builds
            Revision = int.Parse(ThisAssembly.Git.Commits);
            Tag = ThisAssembly.Git.BaseTag;
            Commit = ThisAssembly.Git.Commit;
        }

        /// <summary>
        /// Checks the minimal version number. Returns true when the version is
        /// greater than or equal to the specified major/minor combination.
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="build"></param>
        /// <param name="revision"></param>
        /// <returns></returns>
        public bool CheckMinimalVersionNumber(int major, int minor, int build = -1, int revision = -1) {
            if (Major > major) {
                return true;
            } else if (Major == major) {
                if (Minor > minor) {
                    return true;
                } else if (Minor == minor) {
                    if (build < 0 || Build > build) {
                        return true;
                    } else if (Build == build) {
                        if (revision < 0 || Revision >= revision) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
