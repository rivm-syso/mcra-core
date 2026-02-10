using MCRA.General.KineticModelDefinitions;
using MCRA.Utils.SBML;

namespace MCRA.General.Test.UnitTests.KineticModelDefinitions {

    [TestClass]
    public class SbmlPbkModelSpecificationBuilderTests {

        [TestMethod]
        public void SbmlToPbkModelDefinitionConverter_TestCreateSimple() {
            var filePath = "Resources/SbmlPbkModels/simple.sbml";
            var reader = new SbmlFileReader();
            var sbmlModel = reader.LoadModel(filePath);
            var def = SbmlPbkModelSpecificationBuilder.Convert(sbmlModel);

            Assert.HasCount(1, def.Forcings);
            Assert.HasCount(5, def.Outputs);
            Assert.HasCount(14, def.Parameters);

            var paramsDict = def.Parameters.ToDictionary(r => r.Id);
            Assert.AreEqual(PbkModelParameterType.BodyWeight, paramsDict["BW"].Type);
        }
    }
}
