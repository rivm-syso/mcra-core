using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledDietaryExposuresTests : CompiledTestsBase {
        protected Func<IDictionary<string, DietaryExposureModel>> _getDietaryExposuresDelegate;
        protected Func<IDictionary<string, Compound>> _getSubstancesDelegate;

        [TestMethod]
        public void CompiledDietaryExposures_TestExposuresOnly() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DietaryExposureModels, @"DietaryExposuresTests\DietaryExposureModels"),
                (ScopingType.DietaryExposurePercentiles, @"DietaryExposuresTests\DietaryExposurePercentiles"),
                (ScopingType.DietaryExposurePercentilesUncertain, @"DietaryExposuresTests\DietaryExposurePercentilesUncertain")
            );

            // Only experiments with all matching codes are loaded (matching response codes are mandatory)
            var models = _getDietaryExposuresDelegate.Invoke();
            Assert.AreEqual(2, models.Count);

            // Count 4 percentiles
            Assert.IsTrue(models.Values.All(r => r.DietaryExposurePercentiles.Count == 4));

            // Count 5 uncertainty sets
            Assert.IsTrue(models.Values.All(r => r.DietaryExposurePercentiles.All(p => p.Value.ExposureUncertainties.Count == 5)));

            // Substances are loaded from valid experiments, so only 4 in this case
            var substances = _getSubstancesDelegate.Invoke();
            Assert.AreEqual(1, substances.Count);
        }
    }
}
