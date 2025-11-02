using MCRA.Utils.SBML;

namespace MCRA.Utils.Test.UnitTests.Sbml {

    [TestClass]
    public class SbmlFileReaderTests {

        [TestMethod]
        public void SbmlUnitDefinition_TestSimpleUnits() {
            var filename = "Resources/Sbml/simple_lifetime.sbml";
            var reader = new SbmlFileReader();
            var model = reader.LoadModel(filename);
            Assert.IsNotNull(model);
        }
    }
}
