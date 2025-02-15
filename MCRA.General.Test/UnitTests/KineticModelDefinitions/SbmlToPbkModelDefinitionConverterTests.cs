using MCRA.General.Sbml;
using MCRA.Utils.SBML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            Assert.AreEqual(1, def.Forcings.Count);
            Assert.AreEqual(5, def.Outputs.Count);
            Assert.AreEqual(14, def.Parameters.Count);

            var paramsDict = def.Parameters.ToDictionary(r => r.Id);
            Assert.AreEqual(PbkModelParameterType.BodyWeight, paramsDict["BW"].Type);
        }
    }
}
