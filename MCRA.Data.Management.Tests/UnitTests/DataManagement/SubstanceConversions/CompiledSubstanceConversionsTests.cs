using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledSubstanceConversionsTests : CompiledTestsBase {

        protected Func<ICollection<SubstanceConversion>> _getItemsDelegate;

        [TestMethod]
        public void CompiledSubstanceConversions_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple"),
                (ScopingType.SubstanceConversions, @"ResidueDefinitionsTests/ResidueDefinitionsSimple")
            );

            var allDefinitions = _getItemsDelegate.Invoke();

            Assert.HasCount(2, allDefinitions);
        }

        [TestMethod]
        public void CompiledSubstanceConversions_TestEffectsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple"),
                (ScopingType.SubstanceConversions, @"ResidueDefinitionsTests/ResidueDefinitionsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);

            var allDefinitions = _compiledDataManager.GetAllSubstanceConversions();

            Assert.HasCount(1, allDefinitions);
        }
    }
}
