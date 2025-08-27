using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledPbkModelsTests : CompiledTestsBase {

        protected Func<ICollection<KineticModelInstance>> _getItemsDelegate;

        [TestMethod]
        public void CompiledKineticModelInstancesSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.KineticModelInstances, @"KineticModelsTests/KineticModelInstancesSimple")
            );

            var models = _getItemsDelegate.Invoke();
            var modelIds = models.Select(m => m.IdModelInstance).ToList();

            CollectionAssert.AreEqual(new[] { "km01", "km02", "km03", "km04", "km05", "km06", "km07", "km08", "km09", "km10" }, modelIds);

            Assert.AreEqual(0, models.Sum(a => a.KineticModelInstanceParameters?.Count ?? 0));
        }

        [TestMethod]
        public void CompiledKineticModelInstancesSimpleSubstanceFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.KineticModelInstances, @"KineticModelsTests/KineticModelInstancesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var models = _getItemsDelegate.Invoke();
            var modelIds = models.Select(m => m.IdModelInstance).ToList();

            CollectionAssert.AreEqual(new[] { "km02", "km03", "km07", "km08" }, modelIds);

            Assert.AreEqual(0, models.Sum(a => a.KineticModelInstanceParameters?.Count ?? 0));
        }
    }
}
