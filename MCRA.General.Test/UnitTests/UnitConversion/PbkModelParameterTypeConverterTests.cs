using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.UnitConversion {
    [TestClass]
    public class PbkModelParameterTypeConverterTests {

        [TestMethod]
        [DataRow("http://purl.obolibrary.org/obo/PBPKO_00007", PbkModelParameterType.Physiological)]
        [DataRow("http://purl.obolibrary.org/obo/PBPKO_00009", PbkModelParameterType.BodyWeight)]
        public void PbkModelParameterTypeConverter_TestFromUri(
            string uri,
            PbkModelParameterType type
        ) {
            var result = PbkModelParameterTypeConverter.FromUri(uri);
            Assert.AreEqual(type, result);
        }
    }
}
