using MCRA.General.Sbml;
using MCRA.Utils.Sbml.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.KineticModelDefinitions.SbmlPbkExtensions {
    [TestClass]
    public class SbmlModelParameterExtensionsTests {
        [TestMethod]
        [DataRow(@"http://purl.obolibrary.org/obo/NCIT_C81328", PbkModelParameterType.BodyWeight)]
        [DataRow(@"http://purl.obolibrary.org/obo/NCIT_C25157", PbkModelParameterType.BodySurfaceArea)]
        [DataRow(@"http://purl.obolibrary.org/obo/PBPKO_00007", PbkModelParameterType.Physiological)]
        [DataRow(@"http://purl.obolibrary.org/obo/PBPKO_00167", PbkModelParameterType.PartitionCoefficient)]
        public void SbmlModelParameterExtensions_TestIsOfType(
            string uri,
            PbkModelParameterType parameterType
        ) {
            var param = new SbmlModelParameter() {
                BqbIsResources = [uri]
            };
            Assert.IsTrue(param.IsOfType(parameterType));
        }
    }
}
