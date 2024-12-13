using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledPopulationsTests : CompiledTestsBase {

        protected Func<IDictionary<string, Population>> _getPopulationsDelegate;

        [TestMethod]
        public void CompiledPopulationsTest() {
            _rawDataProvider.SetDataTables(
                 (ScopingType.Populations, @"PopulationsTests\Populations"),
                 (ScopingType.PopulationIndividualPropertyValues, @"PopulationsTests\PopulationIndividualPropertyValues")
            );

            var populations = _getPopulationsDelegate.Invoke();

            Assert.AreEqual(10, populations.Count);

            CollectionAssert.AreEquivalent(new[] { "DE", "DE-N", "DE-W", "DE-S", "DE-E", "DE-Summer", "DE-Winter", "DE-PopulationA", "DE-PopulationB", "DE-PopulationC" }, populations.Keys.ToList());
        }


        [TestMethod]
        public void CompiledPopulationsFilterTest() {
            _rawDataProvider.SetDataTables(
                 (ScopingType.Populations, @"PopulationsTests\Populations"),
                 (ScopingType.PopulationIndividualPropertyValues, @"PopulationsTests\PopulationIndividualPropertyValues")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Populations, ["DE-N"]);

            var populations = _getPopulationsDelegate.Invoke();
            Assert.AreEqual(1, populations.Count);
            var f = populations.First();

            Assert.AreEqual("DE-N", f.Key);
        }
    }
}
