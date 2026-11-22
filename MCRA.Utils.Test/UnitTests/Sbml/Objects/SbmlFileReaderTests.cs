using MCRA.Utils.SBML;

namespace MCRA.Utils.Test.UnitTests.Sbml.Objects {

    [TestClass]
    public class SbmlFileReaderTests {

        [TestMethod]
        [DataRow("simple_lifetime")]
        [DataRow("simple")]
        public void SbmlFileReader_TestRead(string name) {
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
