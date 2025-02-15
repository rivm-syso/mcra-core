using MCRA.General.Sbml;
using MCRA.Utils.Sbml.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.KineticModelDefinitions.SbmlPbkExtensions {
    [TestClass]
    public class SbmlModelParameterExtensionsTests {
        [TestMethod]
        [DataRow(@"http://purl.obolibrary.org/obo/PBPKO_00008", PbkModelParameterType.BodyWeight)]
        [DataRow(@"http://purl.obolibrary.org/obo/PBPKO_00010", PbkModelParameterType.BodySurfaceArea)]
        [DataRow(@"http://purl.obolibrary.org/obo/PBPKO_00521", PbkModelParameterType.Age)]
        [DataRow(@"http://purl.obolibrary.org/obo/PBPKO_00127", PbkModelParameterType.MolecularWeight)]
        public void SbmlModelParameterExtensions_TestIsOfType(
            string uri,
            PbkModelParameterType parameterType
        ) {
            var param = new SbmlModelParameter() {
                BqmIsResources = [uri]
            };
            Assert.IsTrue(param.IsOfType(parameterType));
        }
    }
}
