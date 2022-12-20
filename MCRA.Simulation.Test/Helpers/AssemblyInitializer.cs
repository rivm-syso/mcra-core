using MCRA.Utils.R.REngines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.Helpers {
    /// <summary>
    /// AssemblyInitializer
    /// </summary>
    [TestClass]
    public class AssemblyInitializer {
        /// <summary>
        /// GlobalTestInitializer
        /// </summary>
        /// <param name="context"></param>
        [AssemblyInitialize]
        public static void GlobalTestInitializer(TestContext context) {
            RDotNetEngine.R_HomePath = Properties.Settings.Default.RHomePath;
        }
    }
}
