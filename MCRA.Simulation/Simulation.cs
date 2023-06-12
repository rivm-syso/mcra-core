using System.Configuration;
using System.Reflection;

namespace MCRA.Simulation {
    public abstract class Simulation {

        private static string _tempDataPath = null;

        public static string TempDataPath {
            get {
                if (_tempDataPath == null) {
                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    _tempDataPath = config.AppSettings.Settings["TempDataFolder"]?.Value ?? Path.GetTempPath();
                }
                return _tempDataPath;
            }
        }

        /// <summary>
        /// returns version
        /// </summary>
        /// <returns></returns>
        public static string RetrieveVersion() {
            var assembly = Assembly.GetExecutingAssembly();
            var path = Path.GetDirectoryName(assembly.Location);
            var version = Assembly.LoadFrom(Path.Combine(path, "MCRA.Simulation.dll")).GetName().Version;
#if (TESTWUR || DEBUG)
            return string.Join(".", version.Major, version.Minor, version.Build, version.Revision);
#else
            return string.Join(".", version.Major, version.Minor, version.Build);
#endif
        }

        public const string BuildVersionMetadataPrefix = "+built";
        /// <summary>
        /// returns time stamp of executable/dll
        /// </summary>
        /// <returns></returns>
        public static DateTime RetrieveLinkerTimestamp() {
            var asm = typeof(Simulation).Assembly;

            var attribute = asm
               .GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (attribute?.InformationalVersion != null) {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0) {
                    value = value[(index + BuildVersionMetadataPrefix.Length)..];
                    if (DateTime.TryParse(value, out var dt)) {
                        return dt;
                    }
                }
            }
            return default;
        }
    }
}
