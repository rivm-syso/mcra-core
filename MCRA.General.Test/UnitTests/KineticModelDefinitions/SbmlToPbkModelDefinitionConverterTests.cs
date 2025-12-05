using MCRA.General.Sbml;
using MCRA.Utils.SBML;

namespace MCRA.General.Test.UnitTests.KineticModelDefinitions {

    [TestClass]
    public class SbmlToPbkModelDefinitionConverterTests {

        [TestMethod]
        public void SbmlToPbkModelDefinitionConverter_TestConvertSimple() {
            var filePath = "Resources/SbmlPbkModels/simple.sbml";
            var reader = new SbmlFileReader();
            var sbmlModel = reader.LoadModel(filePath);
            var converter = new SbmlToPbkModelDefinitionConverter();
            var def = converter.Convert(sbmlModel);

            Assert.HasCount(1, def.Forcings);
            Assert.HasCount(5, def.Outputs);
            Assert.HasCount(14, def.Parameters);

            var paramsDict = def.Parameters.ToDictionary(r => r.Id);
            Assert.AreEqual(PbkModelParameterType.BodyWeight, paramsDict["BW"].Type);
        }
    }
}
