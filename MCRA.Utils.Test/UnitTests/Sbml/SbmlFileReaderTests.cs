using MCRA.Utils.SBML;

namespace MCRA.Utils.Test.UnitTests.Sbml {

    [TestClass]
    public class SbmlFileReaderTests {

        [TestMethod]
        [DataRow("simple_lifetime")]
        [DataRow("simple")]
        public void SbmlUnitDefinition_TestSimpleUnits(string name) {
            var filename = $"Resources/Sbml/{name}.sbml";
            var reader = new SbmlFileReader();
            var model = reader.LoadModel(filename);
            Assert.IsNotNull(model);
            Assert.AreEqual("HR", model.TimeUnits);
            Assert.IsNotEmpty(model.Compartments);
            Assert.IsNotEmpty(model.Species);
            Assert.IsNotEmpty(model.Parameters);
        }
    }
}
