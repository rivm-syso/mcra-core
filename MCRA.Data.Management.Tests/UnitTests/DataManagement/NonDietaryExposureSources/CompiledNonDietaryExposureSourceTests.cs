using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                (ScopingType.NonDietaryExposureSources, @"NonDietaryExposureSourcesTests\NonDietaryExposureSourcesSimple")
            );

            var sources = _getNonDietaryExposureSourcesDelegate.Invoke();

            Assert.AreEqual(3, sources.Count);

            NonDietaryExposureSource s;
            Assert.IsTrue(sources.TryGetValue("A", out s) && s.Name.Equals("Aftershave"));
            Assert.IsTrue(sources.TryGetValue("B", out s) && s.Name.Equals("Body lotion"));
            Assert.IsTrue(sources.TryGetValue("C", out s) && s.Name.Equals("Conditioner"));
        }

        [TestMethod]
        public virtual void CompiledNonDietaryExposureSources_TestGetAllNonDietaryExposureSourcesFiltered() {
            _rawDataProvider.SetDataTables(
                (ScopingType.NonDietaryExposureSources, @"NonDietaryExposureSourcesTests\NonDietaryExposureSourcesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.NonDietaryExposureSources, new[] { "A", "C" });

            var sources = _getNonDietaryExposureSourcesDelegate.Invoke();
            NonDietaryExposureSource s;
            Assert.AreEqual(2, sources.Count);

            Assert.IsTrue(sources.TryGetValue("A", out s) && s.Name.Equals("Aftershave"));
            Assert.IsTrue(sources.TryGetValue("C", out s) && s.Name.Equals("Conditioner"));
        }
    }
}
