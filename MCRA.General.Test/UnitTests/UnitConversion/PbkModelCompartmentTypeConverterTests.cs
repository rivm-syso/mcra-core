namespace MCRA.General.Test.UnitTests.UnitConversion {
    [TestClass]
    public class PbkModelCompartmentTypeConverterTests {

        [TestMethod]
        [DataRow("http://purl.obolibrary.org/obo/PBPKO_00448", PbkModelCompartmentType.AlveolarAir)]
        [DataRow("http://purl.obolibrary.org/obo/PBPKO_00558", PbkModelCompartmentType.Liver)]
        public void PbkModelCompartmentTypeConverter_TestFromUri(
            string uri,
            PbkModelCompartmentType type
        ) {
            var result = PbkModelCompartmentTypeConverter.FromUri(uri);
            Assert.AreEqual(type, result);
        }
    }
}
