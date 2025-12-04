using System.Reflection;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects.Populations;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.SimulatedPopulations {

    [TestClass]
    public class SimulatedPopulationTests {

        [TestMethod]
        public void SimulatedPopulation_TestCompare() {
            var population = FakePopulationsGenerator.CreateSingle("NL-2020");
            var simulatedPopulation = new SimulatedPopulation(population);

            // Test population as simulated population
            Assert.IsTrue(population.Equals(simulatedPopulation));
            Assert.IsTrue(simulatedPopulation.Equals(population));
            Assert.IsTrue(population == simulatedPopulation);
            Assert.IsTrue(simulatedPopulation == population);

            // Test population as population
            var simulatedPopulationAsPopulation = new SimulatedPopulation(population) as Population;
            Assert.IsTrue(population.Equals(simulatedPopulationAsPopulation));
            Assert.IsTrue(simulatedPopulationAsPopulation.Equals(population));
            Assert.IsTrue(population == simulatedPopulationAsPopulation);
            Assert.IsTrue(simulatedPopulationAsPopulation == population);

            // Test not equals
            var fakeOtherPopulation = FakePopulationsGenerator.CreateSingle("FR-2010");
            Assert.IsFalse(population.Equals(fakeOtherPopulation));
            Assert.IsFalse(fakeOtherPopulation.Equals(population));
            Assert.IsTrue(simulatedPopulation != fakeOtherPopulation);
            Assert.IsTrue(fakeOtherPopulation != simulatedPopulationAsPopulation);
        }

        [TestMethod]
        public void SimulatedPopulation_TestMembers() {
            var orig = FakePopulationsGenerator.CreateSingle("NL-2020");
            orig.Location = "NL";
            orig.StartDate = new DateTime(2001, 2, 3);
            orig.EndDate = new DateTime(2012, 3, 4);
            orig.SizeUncertaintyLower = 30;
            orig.SizeUncertaintyUpper = 50;
            orig.PopulationIndividualPropertyValues = [];

            var simulated = new SimulatedPopulation(orig);
            var type = typeof(Population);
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                if (!prop.CanRead || !prop.CanWrite) {
                    continue;
                }
                var origValue = prop.GetValue(orig);
                var simulatedValue = prop.GetValue(simulated);
                Assert.IsNotNull(
                    origValue,
                    $"Property value of {prop.Name} is null in original population. Update unit test for proper testing."
                );
                Assert.AreEqual(
                    origValue,
                    simulatedValue,
                    $"Member {prop.Name} of simulated population not equal to original population."
                );
            }
        }
    }
}
