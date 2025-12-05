using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledRisksTests : CompiledTestsBase {
        protected Func<IDictionary<string, RiskModel>> _getRisksDelegate;
        protected Func<IDictionary<string, Compound>> _getSubstancesDelegate;

        [TestMethod]
        public void CompiledRisks_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.RiskModels, @"RisksTests/RiskModels"),
                (ScopingType.RiskPercentiles, @"RisksTests/RiskPercentiles"),
                (ScopingType.RiskPercentilesUncertain, @"RisksTests/RiskPercentilesUncertain")
            );

            // Only experiments with all matching codes are loaded (matching response codes are mandatory)
            var models = _getRisksDelegate.Invoke();
            Assert.HasCount(2, models);

            // Count 4 percentiles
            Assert.IsTrue(models.Values.All(r => r.RiskPercentiles.Count == 4));

            // Count 5 uncertainty sets
            Assert.IsTrue(models.Values.All(r => r.RiskPercentiles.All(p => p.Value.RiskUncertainties.Count == 5)));

            // Substances are loaded from valid experiments, so only 4 in this case
            var substances = _getSubstancesDelegate.Invoke();
            Assert.HasCount(1, substances);
        }
    }
}
