using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledTargetExposuresTests : CompiledTestsBase {
        protected Func<IDictionary<string, TargetExposureModel>> _getTargetExposuresDelegate;
        protected Func<IDictionary<string, Compound>> _getSubstancesDelegate;

        [TestMethod]
        public void CompiledTargetExposuresOnlyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.TargetExposureModels, @"TargetExposuresTests\TargetExposureModels"),
                (ScopingType.TargetExposurePercentiles, @"TargetExposuresTests\TargetExposurePercentiles"),
                (ScopingType.TargetExposurePercentilesUncertain, @"TargetExposuresTests\TargetExposurePercentilesUncertain")
            );

            // Only experiments with all matching codes are loaded (matching response codes are mandatory)
            var models = _getTargetExposuresDelegate.Invoke();
            Assert.AreEqual(2, models.Count);

            // Count 4 percentiles
            Assert.IsTrue(models.Values.All(r => r.TargetExposurePercentiles.Count == 4));

            // Count 5 uncertainty sets
            Assert.IsTrue(models.Values.All(r => r.TargetExposurePercentiles.All(p => p.Value.ExposureUncertainties.Count == 5)));

            // Substances are loaded from valid experiments, so only 4 in this case
            var substances = _getSubstancesDelegate.Invoke();
            Assert.AreEqual(1, substances.Count);
        }
    }
}
