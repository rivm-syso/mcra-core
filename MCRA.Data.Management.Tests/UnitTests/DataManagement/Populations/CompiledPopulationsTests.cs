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

            CollectionAssert.AreEquivalent(
                new[] { "DE", "DE-N", "DE-W", "DE-S", "DE-E", "DE-Summer", "DE-Winter", "DE-PopulationA", "DE-PopulationB", "DE-PopulationC" },
                populations.Keys.ToList()
            );
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

        [TestMethod]
        public void CompiledPopulationsIndividualPropertyMultiValuesTest() {
            _rawDataProvider.SetDataTables(
                 (ScopingType.Populations, @"PopulationsTests\Populations"),
                 (ScopingType.PopulationIndividualProperties, @"PopulationsTests\IndividualProperties"),
                 (ScopingType.PopulationIndividualPropertyValues, @"PopulationsTests\PopulationIndividualPropertyMultiValues")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Populations, ["DE","DE-N","DE-S"]);
            var populations = _getPopulationsDelegate.Invoke();
            Assert.AreEqual(3, populations.Count);
            var pop = populations["DE"];

            var pvals = pop.PopulationIndividualPropertyValues;
            Assert.AreEqual(5, pvals.Count); //3 individualproperties + hardcoded properties
            Assert.AreEqual($"{GenderType.Female},{GenderType.Male}", pvals["GenderFilter"].Value);
            Assert.AreEqual($"{MonthType.January},{MonthType.February},{MonthType.September}", pvals["MonthFilter"].Value);
            Assert.AreEqual($"{BooleanType.True},{BooleanType.False}", pvals["YesNoFilter"].Value);

            pop = populations["DE-N"];
            pvals = pop.PopulationIndividualPropertyValues;
            Assert.AreEqual(4, pvals.Count);
            Assert.AreEqual($"{BooleanType.True}", pvals["YesNoFilter"].Value);
            Assert.AreEqual("North, West", pvals["Region"].Value);

            pop = populations["DE-S"];
            pvals = pop.PopulationIndividualPropertyValues;
            Assert.AreEqual(4, pvals.Count);
            Assert.AreEqual($"{MonthType.December}", pvals["MonthFilter"].Value);
            Assert.AreEqual("South, East", pvals["Region"].Value);
        }
    }
}
