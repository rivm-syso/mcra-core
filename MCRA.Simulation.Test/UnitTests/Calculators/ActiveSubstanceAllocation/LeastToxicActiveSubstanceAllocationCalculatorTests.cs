using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ActiveSubstanceAllocation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ActiveSubstanceAllocation {
    /// <summary>
    /// Aggregate membership model calculator tests.
    /// </summary>
    [TestClass]
    public class LeastToxicActiveSubstanceAllocationCalculatorTests : ActiveSubstanceAllocationCalculatorTestsBase {

        [TestMethod]
        public void LeastToxicActiveSubstanceAllocationCalculator_TestAuthorised() {
            var foods = FakeFoodsGenerator.Create(1);

            // Substances and substance conversions
            var activeSubstances = FakeSubstancesGenerator.Create(["AS1", "AS2"]);
            var measuredSubstances = FakeSubstancesGenerator.Create(["MS1"]);

            //Note: for exclusive conversions the proportion parameter is NOT used.
            var substanceConversions = new List<SubstanceConversion>() {
                createSubstanceConversion(activeSubstances[0], measuredSubstances[0], 0.5, true, 100),
                createSubstanceConversion(activeSubstances[1], measuredSubstances[0], 0.5, true, 200)
            };

            // Create authorisations [both authorised]
            var autorisations = FakeSubstanceAuthorisationsGenerator.Create(
                (foods[0], activeSubstances[1]),
                (foods[0], activeSubstances[0])
            );

            // Create a sample substance collection (one sample and one measurement)
            var sampleSubstanceCollection = fakeSampleCompoundCollection(
                foods[0],
                fakeSampleSubstanceRecord(measuredSubstances, [0.1, double.NaN])
            );

            // AS2 substance least toxic
            var rpfs = new Dictionary<Compound, double>() {
                { activeSubstances[0], 2 },
                { activeSubstances[1], 1 },
            };

            // Create allocator and run
            var allocator = new ToxicityActiveSubstanceAllocationCalculator(
                SubstanceTranslationAllocationMethod.UseLeastToxic,
                substanceConversions,
                autorisations,
                true, false, rpfs, false
            );
            var result = allocator.Allocate(
                sampleSubstanceCollection,
                [.. activeSubstances],
                new McraRandomGenerator(1)
            );
            var sampleCompound = result.Single().SampleCompoundRecords.First();

            // Assert: concentration should be allocated to the AS2, which is authorised
            Assert.IsTrue(sampleCompound.AuthorisedUse);
            Assert.IsTrue(sampleCompound.SampleCompounds[activeSubstances[0]].IsZeroConcentration);
            Assert.AreEqual(0.5 * 0.1, sampleCompound.SampleCompounds[activeSubstances[1]].Residue);
        }

        [TestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public void LeastToxicActiveSubstanceAllocationCalculator_TestAuthorisedRawFood(
            params bool[] authorised
        ) {
            // Create a raw food and a processed food of the raw food
            var rawFoods = FakeFoodsGenerator.Create(1);
            var processingTypes = FakeProcessingTypesGenerator.Create(1);
            var processedFoods = FakeFoodsGenerator.CreateProcessedFoods(rawFoods, processingTypes);

            // Substances and substance conversions
            var measuredSubstances = FakeSubstancesGenerator.Create(["MS1"]);
            var activeSubstances = FakeSubstancesGenerator.Create(["AS1", "AS2"]);
            var substanceConversions = new List<SubstanceConversion>() {
                createSubstanceConversion(activeSubstances[0], measuredSubstances[0], 0.5, true, 0.5),
                createSubstanceConversion(activeSubstances[1], measuredSubstances[0], 0.5, true, 0.5)
            };

            // Authorisations at level of raw foods
            var tuples = authorised
                .Select((r, ix) => (r, ix))
                .Where(r => r.r)
                .Select(r => (rawFoods[0], activeSubstances[r.ix]))
                .ToArray();
            var autorisations = FakeSubstanceAuthorisationsGenerator.Create(
                tuples
            );

            // Sample concentrations at level of processed foods
            var sampleSubstanceCollection = fakeSampleCompoundCollection(
                processedFoods[0],
                fakeSampleSubstanceRecord(measuredSubstances, [0.1, double.NaN])
            );

            //AS1 substance most toxic, AS2 is least toxic
            var rpfs = new Dictionary<Compound, double>() {
                { activeSubstances[0], 2 },
                { activeSubstances[1], 1 },
            };

            // Create allocator and run
            var allocator = new ToxicityActiveSubstanceAllocationCalculator(
                SubstanceTranslationAllocationMethod.UseLeastToxic,
                substanceConversions,
                autorisations,
                true, false, rpfs, false
            );

            var result = allocator.Allocate(
                sampleSubstanceCollection,
                [.. activeSubstances],
                new McraRandomGenerator(1)
            );
            var sampleCompound = result.Single().SampleCompoundRecords.First();

            // One of the two substances should be allocated, the other not
            var allocatedSubstance = activeSubstances
                .Single(r => sampleCompound.SampleCompounds[r].IsPositiveResidue);

            // If one of the two active substances is authorised, the resulting sample substance
            // should also be authorised (and allocated to that substance)
            Assert.AreEqual(authorised[0] || authorised[1], sampleCompound.AuthorisedUse);

            // Residue should be allocated to the first active substance, which is authorised
            Assert.AreEqual(0.5 * 0.1, sampleCompound.SampleCompounds[allocatedSubstance].Residue);

            if (authorised[0] && !authorised[1]) {
                // The allocated active substance should be the authorised one
                Assert.AreEqual(activeSubstances[0], allocatedSubstance);
            } else if (!authorised[0] && authorised[1]) {
                // The allocated active substance should be the authorised one
                Assert.AreEqual(activeSubstances[1], allocatedSubstance);
            }
        }
    }
}
