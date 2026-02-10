using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.Utils.SBML;

namespace MCRA.General.Test.UnitTests.PbkModelDefinitions.PbkModelSpecifications.Sbml {

    [TestClass]
    public class SbmlPbkModelSpecificationBuilderTests {

        [TestMethod]
        public void SbmlPbkModelSpecificationBuilder_TestCreateSimple() {
            var filePath = "Resources/SbmlPbkModels/simple.sbml";
            var reader = new SbmlFileReader();
            var sbmlModel = reader.LoadModel(filePath);
            var def = SbmlPbkModelSpecificationBuilder.Create(sbmlModel);

            Assert.HasCount(5, def.Compartments);
            Assert.HasCount(5, def.Species);
            Assert.HasCount(14, def.Parameters);

            var paramsDict = def.Parameters.ToDictionary(r => r.Id);
            Assert.AreEqual(PbkModelParameterType.BodyWeight, paramsDict["BW"].Type);
        }
    }
}
