using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledDeterministicSubstanceConversionFactorsTests : CompiledTestsBase {

        protected Func<ICollection<DeterministicSubstanceConversionFactor>> _getItemsDelegate;

        [TestMethod]
        public void CompiledDeterministicSubstanceConversionFactors_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple"),
                (ScopingType.Foods, @"FoodsTests/FoodsSimple"),
                (ScopingType.DeterministicSubstanceConversionFactors, @"DeterministicSubstanceConversionFactorsTests/DeterministicSubstanceConversionFactorsSimple")
            );

            var allDefinitions = _getItemsDelegate.Invoke();

            Assert.AreEqual(3, allDefinitions.Count);
        }

        [TestMethod]
        public void CompiledDeterministicSubstanceConversionFactors_TestAutoScopeSubstances() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Foods, @"FoodsTests/FoodsSimple"),
                (ScopingType.DeterministicSubstanceConversionFactors, @"DeterministicSubstanceConversionFactorsTests/DeterministicSubstanceConversionFactorsSimple")
            );

            var allDefinitions = _getItemsDelegate.Invoke();

            Assert.AreEqual(3, allDefinitions.Count);
        }

        [TestMethod]
        public void CompiledDeterministicSubstanceConversionFactors_TestScopeSubstances() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple"),
                (ScopingType.Foods, @"FoodsTests/FoodsSimple"),
                (ScopingType.DeterministicSubstanceConversionFactors, @"DeterministicSubstanceConversionFactorsTests/DeterministicSubstanceConversionFactorsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);

            var allDefinitions = _getItemsDelegate.Invoke();

            Assert.AreEqual(2, allDefinitions.Count);
        }
    }
}
