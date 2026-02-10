using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Test.UnitTests.PbkModelDefinitions.PbkModelSpecifications.Sbml {
    [TestClass]
    public class SbmlPbkModelParameterTests {
        [TestMethod]
        [DataRow(@"http://purl.obolibrary.org/obo/PBPKO_00008", PbkModelParameterType.BodyWeight)]
        [DataRow(@"http://purl.obolibrary.org/obo/PBPKO_00010", PbkModelParameterType.BodySurfaceArea)]
        [DataRow(@"http://purl.obolibrary.org/obo/PBPKO_00521", PbkModelParameterType.Age)]
        [DataRow(@"http://purl.obolibrary.org/obo/PBPKO_00127", PbkModelParameterType.MolecularWeight)]
        public void SbmlPbkModelParameter_TestIsOfType(
            string uri,
            PbkModelParameterType parameterType
        ) {
            var param = new SbmlPbkModelParameter() {
                SbmlModelParameter = new SbmlModelParameter() {
                    BqmIsResources = [uri]
                }
            };
            Assert.IsTrue(param.IsOfType(parameterType));
        }
    }
}
