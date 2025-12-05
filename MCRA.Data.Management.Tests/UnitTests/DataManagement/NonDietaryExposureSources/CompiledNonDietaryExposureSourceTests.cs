using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {

    /// <summary>
    /// Note: this is not marked as TestClass, the subclasses define the method to use
    /// to retrieve the non-dietary exposure sources (_getNonDietaryExposureSourcesDelegate)
    /// These tests are run multiple times (the subclasses) because of the redundancy in
    /// non-dietary exposure sources retrieval in the CompiledDataManager and SubsetManager.
    /// </summary>
    public class CompiledNonDietaryExposureSourceTests : CompiledTestsBase {

        protected Func<IDictionary<string, NonDietaryExposureSource>> _getNonDietaryExposureSourcesDelegate;

        [TestMethod]
        public virtual void CompiledNonDietaryExposureSources_TestGetAllNonDietaryExposureSources() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietaryExposureSources, @"NonDietaryExposureSourcesTests/NonDietaryExposureSourcesSimple")
            );

            var sources = _getNonDietaryExposureSourcesDelegate.Invoke();

            Assert.HasCount(3, sources);

            Assert.IsTrue(sources.TryGetValue("A", out var s) && s.Name.Equals("Aftershave"));
            Assert.IsTrue(sources.TryGetValue("B", out s) && s.Name.Equals("Body lotion"));
            Assert.IsTrue(sources.TryGetValue("C", out s) && s.Name.Equals("Conditioner"));
        }

        [TestMethod]
        public virtual void CompiledNonDietaryExposureSources_TestGetAllNonDietaryExposureSourcesFiltered() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietaryExposureSources, @"NonDietaryExposureSourcesTests/NonDietaryExposureSourcesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.NonDietaryExposureSources, ["A", "C"]);

            var sources = _getNonDietaryExposureSourcesDelegate.Invoke();
            Assert.HasCount(2, sources);

            Assert.IsTrue(sources.TryGetValue("A", out var s) && s.Name.Equals("Aftershave"));
            Assert.IsTrue(sources.TryGetValue("C", out s) && s.Name.Equals("Conditioner"));
        }
    }
}
