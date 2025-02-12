using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            Assert.AreEqual(2, allDefinitions.Count);
        }

        [TestMethod]
        public void CompiledSubstanceConversions_TestEffectsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple"),
                (ScopingType.SubstanceConversions, @"ResidueDefinitionsTests/ResidueDefinitionsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);

            var allDefinitions = _compiledDataManager.GetAllSubstanceConversions();

            Assert.AreEqual(1, allDefinitions.Count);
        }
    }
}
