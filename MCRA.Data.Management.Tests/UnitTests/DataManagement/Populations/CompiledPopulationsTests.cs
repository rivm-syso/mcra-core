using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledPopulationsTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledPopulationsTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                 (ScopingType.Populations, @"PopulationsTests/Populations"),
                 (ScopingType.PopulationIndividualPropertyValues, @"PopulationsTests/PopulationIndividualPropertyValues")
            );

            var populations = GetAllPopulations(managerType);

            Assert.HasCount(10, populations);

            CollectionAssert.AreEquivalent(
                new[] { "DE", "DE-N", "DE-W", "DE-S", "DE-E", "DE-Summer", "DE-Winter", "DE-PopulationA", "DE-PopulationB", "DE-PopulationC" },
                populations.Keys.ToList()
            );
        }


        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledPopulationsFilterTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                 (ScopingType.Populations, @"PopulationsTests/Populations"),
                 (ScopingType.PopulationIndividualPropertyValues, @"PopulationsTests/PopulationIndividualPropertyValues")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Populations, ["DE-N"]);

            var populations = GetAllPopulations(managerType);
            Assert.HasCount(1, populations);
            var f = populations.First();

            Assert.AreEqual("DE-N", f.Key);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledPopulationsIndividualPropertyMultiValuesTest(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                 (ScopingType.Populations, @"PopulationsTests/Populations"),
                 (ScopingType.PopulationIndividualProperties, @"PopulationsTests/IndividualProperties"),
                 (ScopingType.PopulationIndividualPropertyValues, @"PopulationsTests/PopulationIndividualPropertyMultiValues")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Populations, ["DE", "DE-N", "DE-S"]);
            var populations = GetAllPopulations(managerType);
            Assert.HasCount(3, populations);
            var pop = populations["DE"];

            var pvals = pop.PopulationIndividualPropertyValues;
            Assert.HasCount(5, pvals); //3 individualproperties + hardcoded properties
            Assert.AreEqual($"{GenderType.Female},{GenderType.Male}", pvals["GenderFilter"].Value);
            Assert.AreEqual($"{MonthType.January},{MonthType.February},{MonthType.September}", pvals["MonthFilter"].Value);
            Assert.AreEqual($"{BooleanType.True},{BooleanType.False}", pvals["YesNoFilter"].Value);

            pop = populations["DE-N"];
            pvals = pop.PopulationIndividualPropertyValues;
            Assert.HasCount(4, pvals);
            Assert.AreEqual($"{BooleanType.True}", pvals["YesNoFilter"].Value);
            Assert.AreEqual("North, West", pvals["Region"].Value);

            pop = populations["DE-S"];
            pvals = pop.PopulationIndividualPropertyValues;
            Assert.HasCount(4, pvals);
            Assert.AreEqual($"{MonthType.December}", pvals["MonthFilter"].Value);
            Assert.AreEqual("South, East", pvals["Region"].Value);
        }
    }
}
