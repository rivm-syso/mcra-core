using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledDeterministicSubstanceConversionFactorsTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledDeterministicSubstanceConversionFactors_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple"),
                (ScopingType.Foods, @"FoodsTests/FoodsSimple"),
                (ScopingType.DeterministicSubstanceConversionFactors,
                    @"DeterministicSubstanceConversionFactorsTests/DeterministicSubstanceConversionFactorsSimple")
            );

            var allDefinitions = GetAllDeterministicSubstanceConversionFactors(managerType);

            Assert.HasCount(3, allDefinitions);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledDeterministicSubstanceConversionFactors_TestAutoScopeSubstances(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodsTests/FoodsSimple"),
                (ScopingType.DeterministicSubstanceConversionFactors,
                    @"DeterministicSubstanceConversionFactorsTests/DeterministicSubstanceConversionFactorsSimple")
            );

            var allDefinitions = GetAllDeterministicSubstanceConversionFactors(managerType);

            Assert.HasCount(3, allDefinitions);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledDeterministicSubstanceConversionFactors_TestScopeSubstances(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple"),
                (ScopingType.Foods, @"FoodsTests/FoodsSimple"),
                (ScopingType.DeterministicSubstanceConversionFactors,
                    @"DeterministicSubstanceConversionFactorsTests/DeterministicSubstanceConversionFactorsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);

            var allDefinitions = GetAllDeterministicSubstanceConversionFactors(managerType);

            Assert.HasCount(2, allDefinitions);
        }
    }
}
