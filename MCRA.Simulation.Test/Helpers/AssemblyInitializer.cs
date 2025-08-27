using MCRA.Utils.R.REngines;
using Microsoft.Extensions.Configuration;

namespace MCRA.Simulation.Test.Helpers {
    /// <summary>
    /// AssemblyInitializer
    /// </summary>
    [TestClass]
    public class AssemblyInitializer {

        /// <summary>
        /// Initialize global test settings.
        /// </summary>
        [AssemblyInitialize]
        public static void GlobalTestInitializer(TestContext context) {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("appsettings.user.json", optional: true);
            var config = builder.Build();
            RDotNetEngine.R_HomePath = config["RHomePath"];
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", config["PythonPath"]);
        }
    }
}
