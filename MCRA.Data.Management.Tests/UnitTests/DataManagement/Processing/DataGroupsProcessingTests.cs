using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataGroupsProcessingTests : CompiledTestsBase {
        /// <summary>
        /// Tests correct loading of the processing types. Verification by making sure that only
        /// the expected processing types exist in the compiled datasource. Check total count
        /// and assert presence of each of the processing types through the processing type
        /// code.
        /// </summary>
        [TestMethod]
        public void ProcessingFactorsDataTest1() {
            _rawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", SourceTableGroup.Foods);
            var processingTypes = _compiledDataManager.GetAllProcessingTypes();
            var processingTypeCodes = processingTypes.Keys.ToList();

            Assert.AreEqual(5, processingTypes.Count);
            CollectionAssert.Contains(processingTypeCodes, "1");
            CollectionAssert.Contains(processingTypeCodes, "2");
            CollectionAssert.Contains(processingTypeCodes, "3");
            CollectionAssert.Contains(processingTypeCodes, "9");
            CollectionAssert.Contains(processingTypeCodes, "99");

        }

        /// <summary>
        /// Tests correct loading of the processing factors. Verification by making sure that only
        /// the expected processing factors exist in the compiled datasource. Check total count
        /// and assert presence of each of the processing types through the processing type
        /// code.
        /// </summary>
        [TestMethod]
        public void ProcessingFactorsDataTest2() {
            _rawDataProvider.SetDataGroupsFromFolder(
                1,
                "_DataGroupsTest",
                [SourceTableGroup.Foods, SourceTableGroup.Processing]);

            var processingFactors = _compiledDataManager.GetAllProcessingFactors();
            var processingFactorsCooking = processingFactors.Where(pf => pf.ProcessingType.Name == "Cooking").ToList();
            var processingFactorsPeeling = processingFactors.Where(pf => pf.ProcessingType.Name == "Peeling").ToList();

            Assert.AreEqual(2, processingFactorsCooking.Count);
            Assert.AreEqual(2, processingFactorsPeeling.Count);
        }

    }
}
