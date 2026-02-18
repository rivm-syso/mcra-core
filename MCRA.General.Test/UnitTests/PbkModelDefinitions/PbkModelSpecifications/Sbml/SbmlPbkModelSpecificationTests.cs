using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.Utils.SBML;

namespace MCRA.General.Test.UnitTests.PbkModelDefinitions.PbkModelSpecifications.Sbml {

    [TestClass]
    public class SbmlPbkModelSpecificationTests {

        [TestMethod]
        [DataRow(false, 1)]
        [DataRow(true, 3)]
        public void SbmlPbkModelSpecification_TestGetRouteInputSpecies(bool fallbackSystemic, int expected) {
            var filePath = "Resources/SbmlPbkModels/simple.sbml";
            var def = loadModel(filePath);
            Assert.HasCount(expected, def.GetRouteInputSpecies(fallbackSystemic));
        }

        [TestMethod]
        public void SbmlPbkModelSpecification_TestGetOutputMatrices() {
            var filePath = "Resources/SbmlPbkModels/simple.sbml";
            var def = loadModel(filePath);
            var result = def.GetOutputMatrices();
            // Currently present 3 out of the 5 compartments are linked to a biological matrix
            Assert.HasCount(3, result);
        }

        private static SbmlPbkModelSpecification loadModel(string filePath) {
            var reader = new SbmlFileReader();
            var sbmlModel = reader.LoadModel(filePath);
            var def = SbmlPbkModelSpecificationBuilder.Create(sbmlModel);
            return def;
        }
    }
}
