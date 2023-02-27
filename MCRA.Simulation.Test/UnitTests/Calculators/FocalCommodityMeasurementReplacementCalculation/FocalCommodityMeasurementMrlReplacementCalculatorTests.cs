using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.FocalCommodityMeasurementReplacementCalculation {

    /// <summary>
    /// Tests for the focal commodity measurement replacement calculator factory.
    /// </summary>
    [TestClass]
    public class FocalCommodityMeasurementMrlReplacementCalculatorTests {

        /// <summary>
        /// Test create measurement removal calculator.
        /// </summary>
        [TestMethod]
        public void FocalCommodityMrlMeasurementReplacementCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(5);
            var backgroundSampleCompoundCollection = MockSampleCompoundCollectionsGenerator
                .Create(foods, substances, random);
            var mrls = MockMaximumConcentrationLimitsGenerator.Create(foods, substances, random);
            var adjustmentFactor = 0.5;
            var model = new FocalCommodityMeasurementMrlReplacementCalculator(
                50,
                mrls,
                null,
                adjustmentFactor,
                ConcentrationUnit.ugPerKg
            );
            var focalCombinations = foods.Take(1).SelectMany(r => substances.Take(1), (f, s) => (Food: f, Substance: s)).ToList();
            var result = model.Compute(
                backgroundSampleCompoundCollection,
                focalCombinations,
                random
            );

            // Check result available
            Assert.IsNotNull(result);

            // Check replaced concentrations
            var concentrationCorrectionFactor = 1000; // Concentration correction mg/kg to ug/kg
            var residuesFocalCombination = result[foods.First()].SampleCompoundRecords.Select(r => r.SampleCompounds[substances.First()].Residue).Distinct().ToArray();
            CollectionAssert.AreEquivalent(new double[] { 0, concentrationCorrectionFactor * adjustmentFactor * mrls[(foods.First(), substances.First())].Limit }, residuesFocalCombination);
        }

        /// <summary>
        /// Test create measurement removal calculator.
        /// </summary>
        [TestMethod]
        public void FocalCommodityMrlMeasurementReplacementCalculator_TestSubstanceConversion() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            // Total three substances one only active, one only measured, and one both
            var substances = MockSubstancesGenerator.Create(3);
            var measuredSubstances = substances.Take(2).ToList();
            var activeSubstances = substances.Skip(1).ToList();
            var focalSubstance = measuredSubstances.First();
            var focalFood = foods.First();
            var substanceConversions = MockDeterministicSubstanceConversionFactorsGenerator
                .Create(measuredSubstances, activeSubstances, random);
            var backgroundSampleCompoundCollection = MockSampleCompoundCollectionsGenerator
                .Create(foods, activeSubstances, random);
            var mrls = MockMaximumConcentrationLimitsGenerator.Create(foods, measuredSubstances, random);
            var adjustmentFactor = 0.5;
            var model = new FocalCommodityMeasurementMrlReplacementCalculator(
                50,
                mrls,
                substanceConversions,
                adjustmentFactor,
                ConcentrationUnit.ugPerKg
            );
            var focalCombinations = new List<(Food Food, Compound Substance)>() { (Food: focalFood, Substance: focalSubstance) };
            var result = model.Compute(
                backgroundSampleCompoundCollection,
                focalCombinations,
                random
            );

            // Check result available
            Assert.IsNotNull(result);

            // Check replaced concentrations
            var concentrationCorrectionFactor = 1000; // Concentration correction mg/kg to ug/kg
            var substanceConversion = substanceConversions.First(r => r.MeasuredSubstance == focalSubstance);
            var expectedPositiveValue = concentrationCorrectionFactor * adjustmentFactor * mrls[(focalFood, focalSubstance)].Limit * substanceConversion.ConversionFactor;
            var residuesFocalCombination = result[focalFood].SampleCompoundRecords.Select(r => r.SampleCompounds[substanceConversion.ActiveSubstance].Residue).Distinct().ToArray();
            CollectionAssert.AreEquivalent(new double[] { 0, expectedPositiveValue }, residuesFocalCombination);
            Assert.IsFalse(result[focalFood].SampleCompoundRecords.First().SampleCompounds.ContainsKey(focalSubstance));
            CollectionAssert.AreEquivalent(substances.Skip(1).ToList(), result[focalFood].SampleCompoundRecords.First().SampleCompounds.Keys.ToList());
        }
    }
}
