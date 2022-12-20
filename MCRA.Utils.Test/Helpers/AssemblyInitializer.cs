using MCRA.Utils.R.REngines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.Helpers {
    [TestClass]
    public class AssemblyInitializer {
        [AssemblyInitialize]
        public static void GlobalTestInitializer(TestContext context) {
            RDotNetEngine.R_HomePath = Properties.Settings.Default.RHomePath;
        }
    }
}
