namespace MCRA.General.Test.UnitTests.UnitConversion {
    [TestClass]
    public class PbkModelSpeciesTypeConverterTests {

        [TestMethod]
        [DataRow("http://identifiers.org/taxonomy/10095", PbkModelSpeciesType.Mice)]
        public void PbkModelSpeciesType_TestEquals_Equal1(string uri, PbkModelSpeciesType expected) {
            var result = PbkModelSpeciesTypeConverter.FromUri(uri);
            Assert.AreEqual(expected, result);
        }
    }
}
