using MCRA.Data.Compiled.Utils;

namespace MCRA.Data.Compiled.Test {
    [TestClass]
    public class FoodCodeUtilitiesTests {

        [TestMethod]
        public void FoodCodeUtilities_TestIsFoodEx2Code() {
            Assert.IsTrue(FoodCodeUtilities.IsFoodEx2Code("A01CE"));
            Assert.IsTrue(FoodCodeUtilities.IsFoodEx2Code("A01CE#F28.A07LC"));
            Assert.IsFalse(FoodCodeUtilities.IsFoodEx2Code("A01CE#"));
            Assert.IsFalse(FoodCodeUtilities.IsFoodEx2Code("A01C"));
            Assert.IsTrue(FoodCodeUtilities.IsFoodEx2Code("A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR"));
            Assert.IsFalse(FoodCodeUtilities.IsFoodEx2Code("A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT.F28.A07GR"));
            Assert.IsFalse(FoodCodeUtilities.IsFoodEx2Code("A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28$A07GRD"));
        }

        [TestMethod]
        [DataRow("A01CE", "A01CE")]
        [DataRow("A01CE#F28.A07LC", "A01CE")]
        [DataRow("A01CE#", "A01CE")]
        [DataRow("A01C", "A01C")]
        [DataRow("A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR", "A0BYV")]
        [DataRow("A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT.F28.A07GR", "A0BYV")]
        [DataRow("A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28$A07GRD", "A0BYV")]
        [DataRow("P0110050A#F28.A07LN$F28.A07KF", "P0110050A")]
        [DataRow("P0110050A#28.A07LN$F28.A07KF", "P0110050A")]
        [DataRow("P0110050A#", "P0110050A")]
        public void FoodCodeUtilities_TestGetFoodEx2BaseCode(string str, string expected) {
            var result = FoodCodeUtilities.GetFoodEx2BaseCode(str);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("A01CE", false)]
        [DataRow("A01CE#F28.A07LC", true)]
        [DataRow("A01CE#", false)]
        [DataRow("A01C", false)]
        [DataRow("A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR", true)]
        [DataRow("A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT.F28.A07GR", false)]
        [DataRow("A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28$A07GRD", false)]
        [DataRow("P0110050A#F28.A07LN$F28.A07KF", true)]
        [DataRow("P0110050A#28.A07LN$F28.A07KF", false)]
        [DataRow("P0110050A#", false)]
        public void FoodCodeUtilities_TestIsCodeWithFoodEx2Facets(string str, bool expected) {
            var result = FoodCodeUtilities.IsCodeWithFoodEx2Facets(str);
            Assert.AreEqual(expected, result);
        }

        #region GetLastFoodEx2FacetCode

        [TestMethod]
        public void GetLastFoodEx2FacetCode_Test1() {
            var code = "A0BYV#F28.A07GR";
            Assert.AreEqual("F28.A07GR", FoodCodeUtilities.GetLastFoodEx2FacetCode(code));
        }

        [TestMethod]
        public void GetLastFoodEx2FacetCode_Test2() {
            var code = "A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR";
            Assert.AreEqual("F28.A07GR", FoodCodeUtilities.GetLastFoodEx2FacetCode(code));
        }

        [TestMethod]
        public void GetLastFoodEx2FacetCode_Test3() {
            var code = "A0BYV#F02.A06G";
            Assert.AreEqual(string.Empty, FoodCodeUtilities.GetLastFoodEx2FacetCode(code));
        }

        [TestMethod]
        public void GetLastFoodEx2FacetCode_Test4() {
            var code = "A0BYV";
            Assert.AreEqual(string.Empty, FoodCodeUtilities.GetLastFoodEx2FacetCode(code));
        }

        [TestMethod]
        public void GetLastFoodEx2FacetCode_Test5() {
            var code = "A0BY#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR";
            Assert.AreEqual(string.Empty, FoodCodeUtilities.GetLastFoodEx2FacetCode(code));
        }

        #endregion

        #region GetFoodEx2FacetCodes

        [TestMethod]
        [DataRow("A0BYV#F28.A07GR", "F28.A07GR")]
        [DataRow("A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR", "F02.A06GF,F03.A06HY,F04.A00ZT,F28.A07GR")]
        [DataRow("A0BYV#F02.A06G", "")]
        [DataRow("A0BYV", "")]
        [DataRow("A0BY#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR", "")]
        public void FoodCodeUtilities_TestGetFoodEx2FacetCodes(string code, string expected) {
            var facets = expected.Split(',').Where(r => !string.IsNullOrEmpty(r)).ToList();
            CollectionAssert.AreEqual(facets, FoodCodeUtilities.GetFoodEx2FacetCodes(code));
        }

        [TestMethod]
        [DataRow("F28.A07GR", true)]
        [DataRow("F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR", true)]
        [DataRow("F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07R", false)]
        [DataRow("A0BYV#F28.A07GR", false)]
        [DataRow("A0BYV", false)]
        [DataRow("A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR", false)]
        public void FoodCodeUtilities_TestIsFoodEx2FacetString(string code, bool expected) {
            Assert.AreEqual(expected, FoodCodeUtilities.IsFoodEx2FacetString(code));
        }

        [TestMethod]
        [DataRow("F28.A07GR", "F28.A07GR")]
        [DataRow("F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR", "F02.A06GF,F03.A06HY,F04.A00ZT,F28.A07GR")]
        [DataRow("F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07R", "")]
        [DataRow("A0BYV#F28.A07GR", "")]
        [DataRow("A0BYV", "")]
        [DataRow("A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR", "")]
        public void FoodCodeUtilities_TestParseFacetString(string code, string expected) {
            var facets = expected.Split(',').Where(r => !string.IsNullOrEmpty(r)).ToList();
            CollectionAssert.AreEqual(facets, FoodCodeUtilities.ParseFacetString(code));
        }

        #endregion

        #region StripFacetFromFoodEx2Code

        [TestMethod]
        public void FoodCodeUtilities_TestStripFacetFromFoodEx2Code1() {
            var foodCode = "A0BYV#F28.A07GR";
            var facetCode = "F28.A07GR";
            var expected = "A0BYV";
            Assert.AreEqual(expected, FoodCodeUtilities.StripFacetFromFoodEx2Code(foodCode, facetCode));
        }

        [TestMethod]
        public void FoodCodeUtilities_TestStripFacetFromFoodEx2Code2() {
            var foodCode = "A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR";
            var facetCode = "F28.A07GR";
            var expected = "A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT";
            Assert.AreEqual(expected, FoodCodeUtilities.StripFacetFromFoodEx2Code(foodCode, facetCode));
        }

        [TestMethod]
        public void FoodCodeUtilities_TestStripFacetFromFoodEx2Code3() {
            var foodCode = "A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR";
            var facetCode = "F02.A06GF";
            var expected = "A0BYV#F03.A06HY$F04.A00ZT$F28.A07GR";
            Assert.AreEqual(expected, FoodCodeUtilities.StripFacetFromFoodEx2Code(foodCode, facetCode));
        }

        [TestMethod]
        public void FoodCodeUtilities_TestStripFacetFromFoodEx2Code4() {
            var foodCode = "A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR";
            var facetCode = "bla";
            var expected = "A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR";
            Assert.AreEqual(expected, FoodCodeUtilities.StripFacetFromFoodEx2Code(foodCode, facetCode));
        }

        [TestMethod]
        public void FoodCodeUtilities_TestStripFacetFromFoodEx2Code5() {
            var foodCode = "bla#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR";
            var facetCode = "F02.A06GF$";
            var expected = "bla#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR";
            Assert.AreEqual(expected, FoodCodeUtilities.StripFacetFromFoodEx2Code(foodCode, facetCode));
        }

        [TestMethod]
        public void FoodCodeUtilities_TestStripFacetFromFoodEx2Code6() {
            var foodCode = "A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR";
            var facetCode = "A06GF$";
            var expected = "A0BYV#F02.A06GF$F03.A06HY$F04.A00ZT$F28.A07GR";
            Assert.AreEqual(expected, FoodCodeUtilities.StripFacetFromFoodEx2Code(foodCode, facetCode));
        }

        #endregion

        #region SplitFoodEx2FullFacetCode

        [TestMethod]
        public void FoodCodeUtilities_TestSplitFoodEx2FullFacetCode_Valid() {
            var fullFacetCode = "F28.A07GR";
            var result = FoodCodeUtilities.SplitFoodEx2FoodFacetCode(fullFacetCode);
            Assert.AreEqual("F28", result.FacetCode);
            Assert.AreEqual("A07GR", result.DescriptorCode);
        }

        [TestMethod]
        public void FoodCodeUtilities_TestSplitFoodEx2FullFacetCode_Invalid() {
            var fullFacetCode = "F28.A07G";
            var result = FoodCodeUtilities.SplitFoodEx2FoodFacetCode(fullFacetCode);
            Assert.IsNull(result.FacetCode);
            Assert.IsNull(result.DescriptorCode);
        }

        [TestMethod]
        public void FoodCodeUtilities_TestSplitFoodEx2FullFacetCode_EmptyString() {
            var fullFacetCode = string.Empty;
            var result = FoodCodeUtilities.SplitFoodEx2FoodFacetCode(fullFacetCode);
            Assert.IsNull(result.FacetCode);
            Assert.IsNull(result.DescriptorCode);
        }

        #endregion

        [TestMethod]
        public void FoodCodeUtilities_TestIsProcessing() {
            Assert.IsTrue(FoodCodeUtilities.IsProcessedFood("P0211000A-F28.A07GV$F28.A07KV"));
            Assert.IsFalse(FoodCodeUtilities.IsProcessedFood("P0211000A#F28.A07GV$F28.A07KV"));
            Assert.IsFalse(FoodCodeUtilities.IsProcessedFood("P0211000A"));
            Assert.IsTrue(FoodCodeUtilities.IsProcessedFood("P0211000A-8"));
            Assert.IsFalse(FoodCodeUtilities.IsProcessedFood("P0211000A-"));
            Assert.IsFalse(FoodCodeUtilities.IsProcessedFood("-P0211000A"));
            Assert.IsFalse(FoodCodeUtilities.IsProcessedFood(""));
            Assert.IsFalse(FoodCodeUtilities.IsProcessedFood(null));
        }

        [TestMethod]
        [DataRow("P0211000A#F28.A07GV$F28.A07KV", "F28.A07GV$F28.A07KV")]
        [DataRow("P0211000A-F28.A07GV$F28.A07KV", null)]
        public void FoodCodeUtilities_GetFoodEx2FacetString(
            string code,
            string expected
        ) {
            var result = FoodCodeUtilities.GetFoodEx2FacetString(code);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void FoodCodeUtilities_TestGetFoodProcessingParts() {
            CollectionAssert.AreEqual(
                FoodCodeUtilities.GetFoodProcessingParts("P0211000A-F28.A07GV$F28.A07KV"),
                new List<string> { "F28.A07GV$F28.A07KV" }
            );
            CollectionAssert.AreEqual(
                FoodCodeUtilities.GetFoodProcessingParts("P0211000A-1-3"),
                new List<string> { "1", "3" }
            );
            Assert.IsNull(FoodCodeUtilities.GetFoodProcessingParts("P0211000A"));
            Assert.IsNull(FoodCodeUtilities.GetFoodProcessingParts(null));
            Assert.IsNull(FoodCodeUtilities.GetFoodProcessingParts(""));
            Assert.IsNull(FoodCodeUtilities.GetFoodProcessingParts("P0211000A-"));
            Assert.IsNull(FoodCodeUtilities.GetFoodProcessingParts("-P0211000A-"));
        }

        [TestMethod]
        public void FoodCodeUtilities_TestGetProcessedFoodBaseCode() {
            Assert.AreEqual("P0211000A", FoodCodeUtilities.GetProcessedFoodBaseCode("P0211000A-F28.A07GV$F28.A07KV"));
            Assert.AreEqual("P0211000A", FoodCodeUtilities.GetProcessedFoodBaseCode("P0211000A-1-3"));
            Assert.IsNull(FoodCodeUtilities.GetFoodProcessingParts("P0211000A"));
            Assert.IsNull(FoodCodeUtilities.GetFoodProcessingParts(null));
            Assert.IsNull(FoodCodeUtilities.GetFoodProcessingParts(""));
            Assert.IsNull(FoodCodeUtilities.GetFoodProcessingParts("P0211000A-"));
            Assert.IsNull(FoodCodeUtilities.GetFoodProcessingParts("-P0211000A-"));
        }
    }
}
