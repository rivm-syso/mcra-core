using System.Configuration;
using System.Reflection;
using MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation;
using MCRA.Utils.Sbml.BatchSimulation;
using Microsoft.Extensions.Options;

namespace MCRA.Simulation {
    public abstract class Simulation {

        private static SbmlRoadRunnerOptions _sbmlRoadRunnerOptions = Options.Create(new SbmlRoadRunnerOptions()).Value;

        /// <summary>RoadRunner options shared by SBML calculators and worker processes.</summary>
        public static SbmlRoadRunnerOptions RoadRunnerOptions => _sbmlRoadRunnerOptions;

        /// <summary>Configures RoadRunner options from the application configuration.</summary>
        public static void ConfigureSbmlRoadRunnerOptions(Microsoft.Extensions.Configuration.IConfiguration configuration) {
            var section = configuration.GetSection(SbmlRoadRunnerOptions.SectionName);
            var options = new SbmlRoadRunnerOptions();
            if (section[nameof(SbmlRoadRunnerOptions.WorkerCount)] is string workerCount) {
                options.WorkerCount = workerCount;
            }
            if (Enum.TryParse<RoadRunnerWorkerProxyType>(section[nameof(SbmlRoadRunnerOptions.ProxyType)], true, out var proxyType)) {
                options.ProxyType = proxyType;
            }
            if (bool.TryParse(section[nameof(SbmlRoadRunnerOptions.ShowConsoleWindow)], out var showConsoleWindow)) {
                options.ShowConsoleWindow = showConsoleWindow;
            }
            if (bool.TryParse(section[nameof(SbmlRoadRunnerOptions.ExtensiveConsoleLogging)], out var extensiveConsoleLogging)) {
                options.ExtensiveConsoleLogging = extensiveConsoleLogging;
            }
            _sbmlRoadRunnerOptions = Options.Create(options).Value;
        }

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
