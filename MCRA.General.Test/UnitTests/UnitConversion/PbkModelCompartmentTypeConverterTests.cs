using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {
    [TestClass]
    public class PbkModelCompartmentTypeConverterTests {

        [TestMethod]
        [DataRow("http://purl.obolibrary.org/obo/PBPKO_00458", PbkModelCompartmentType.AlveolarAir)]
        public void PbkModelCompartmentTypeConverter_TestFromUri(
            string uri,
            PbkModelCompartmentType type
        ) {
            var result = PbkModelCompartmentTypeConverter.FromUri(uri);
            Assert.AreEqual(type, result);
        }
    }
}
