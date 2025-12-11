using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.BodIndicatorModels;

namespace MCRA.Simulation.Test.UnitTests.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    [TestClass]
    public class BodConversionsCalculatorTests {

        [TestMethod]
        public void BodConversionsCalculator_TestSimpleConversion() {
            var bods = fakeBods((BodIndicator.DALY, 2));
            var conversions = fakeConverions(
                (BodIndicator.DALY, BodIndicator.Costs, 3)
            );
            var derivedBodIndicatorModels = computeDerivedIndicators(conversions, bods);

            // Assert: one derived BoD indicator model for costs
            Assert.HasCount(1, derivedBodIndicatorModels);

            // Derived costs indicator (from DALY)
            var modelCosts = derivedBodIndicatorModels.Single();
            CollectionAssert.AreEquivalent(
                modelCosts.Conversions.Select(r => r.ToIndicator).ToList(),
                new[] { BodIndicator.Costs }
            );
            Assert.AreEqual(6, modelCosts.GetBodIndicatorValue());
        }

        [TestMethod]
        public void BodConversionsCalculator_TestIndirectConversions() {
            var bods = fakeBods((BodIndicator.DALY, 2));
            var conversions = fakeConverions(
                (BodIndicator.DALY, BodIndicator.Costs, 3),
                (BodIndicator.Costs, BodIndicator.Deaths, 4)
            );
            var derivedBodIndicatorModels = computeDerivedIndicators(conversions, bods);

            // Expect two derived models (costs and deaths)
            Assert.HasCount(2, derivedBodIndicatorModels);

            // Derived costs indicator (from DALY)
            var modelCosts = derivedBodIndicatorModels.First(r => r.BodIndicator == BodIndicator.Costs);
            CollectionAssert.AreEquivalent(
                modelCosts.Conversions.Select(r => r.ToIndicator).ToList(),
                new[] { BodIndicator.Costs }
            );
            Assert.AreEqual(6, modelCosts.GetBodIndicatorValue());

            // Derived deaths indicator (via costs)
            var modelDeaths = derivedBodIndicatorModels.First(r => r.BodIndicator == BodIndicator.Deaths);
            CollectionAssert.AreEquivalent(
                modelDeaths.Conversions.Select(r => r.ToIndicator).ToList(),
                new[] { BodIndicator.Costs, BodIndicator.Deaths }
            );
            Assert.AreEqual(24, modelDeaths.GetBodIndicatorValue());
        }

        [TestMethod]
        public void BodConversionsCalculator_TestExistingConversion() {
            var bods = fakeBods((BodIndicator.DALY, 1), (BodIndicator.Deaths, 2));
            var conversions = fakeConverions(
                (BodIndicator.DALY, BodIndicator.Costs, 2),
                (BodIndicator.Costs, BodIndicator.Deaths, 3)
            );
            var derivedBodIndicatorModels = computeDerivedIndicators(conversions, bods);

            // Expect two derived models (costs and deaths)
            Assert.HasCount(2, derivedBodIndicatorModels);

            // Derived costs indicator (from DALY)
            var modelCosts = derivedBodIndicatorModels.First(r => r.BodIndicator == BodIndicator.Costs);
            CollectionAssert.AreEquivalent(
                modelCosts.Conversions.Select(r => r.ToIndicator).ToList(),
                new[] { BodIndicator.Costs }
            );
            Assert.AreEqual(2, modelCosts.GetBodIndicatorValue());

            // Derived deaths indicator (via costs)
            var modelDeaths = derivedBodIndicatorModels.First(r => r.BodIndicator == BodIndicator.Deaths);
            CollectionAssert.AreEquivalent(
                modelDeaths.Conversions.Select(r => r.ToIndicator).ToList(),
                new[] { BodIndicator.Costs, BodIndicator.Deaths }
            );
            Assert.AreEqual(6, modelDeaths.GetBodIndicatorValue());
        }

        [TestMethod]
        public void BodConversionsCalculator_TestNoMatchingConversions() {
            var bods = fakeBods((BodIndicator.Prevalence, 1));
            var conversions = fakeConverions(
                (BodIndicator.DALY, BodIndicator.Costs, 1)
            );
            var derivedBodIndicatorModels = computeDerivedIndicators(conversions, bods);

            // No conversions from prevalence to other indicators: expect empty
            Assert.IsEmpty(derivedBodIndicatorModels);
        }

        [TestMethod]
        public void BodConversionsCalculator_TestParallelConversions() {
            var bods = fakeBods((BodIndicator.DALY, 1), (BodIndicator.Costs, 2));
            var conversions = fakeConverions(
                (BodIndicator.DALY, BodIndicator.Deaths, 2),
                (BodIndicator.Costs, BodIndicator.Deaths, 3)
            );
            var derivedBodIndicatorModels = computeDerivedIndicators(conversions, bods);

            // Expect two derived models for deaths indicator:
            // - 1: DALY -> Deaths
            // - 2: Cost -> Deaths
            Assert.HasCount(2, derivedBodIndicatorModels);
            var derivedModelsDeaths = derivedBodIndicatorModels
                .Where(r => r.BodIndicator == BodIndicator.Deaths);
            Assert.HasCount(2, derivedModelsDeaths);
            CollectionAssert.AreEquivalent(
                derivedModelsDeaths.Select(r => r.GetBodIndicatorValue()).ToList(),
                new[] { 2D, 6D }
            );
        }

        [TestMethod]
        public void BodConversionsCalculator_TestExistingIntermediateConversion() {
            var bods = fakeBods((BodIndicator.DALY, 1), (BodIndicator.Costs, 3));
            var conversions = fakeConverions(
                (BodIndicator.DALY, BodIndicator.Costs, 2),
                (BodIndicator.Costs, BodIndicator.Deaths, 2)
            );
            var derivedBodIndicatorModels = computeDerivedIndicators(conversions, bods);
            Assert.HasCount(3, derivedBodIndicatorModels);

            // One derived model for costs indicator (from DALY)
            var derivedModelCosts = derivedBodIndicatorModels
                .Single(r => r.BodIndicator == BodIndicator.Costs);
            Assert.AreEqual(2, derivedModelCosts.GetBodIndicatorValue());

            // Two derived models for deaths indicator:
            // - 1: Cost -> Deaths
            // - 2: DALY -> Cost -> Deaths
            var derivedModelsDeaths = derivedBodIndicatorModels
                .Where(r => r.BodIndicator == BodIndicator.Deaths);
            Assert.HasCount(2, derivedModelsDeaths);
            CollectionAssert.AreEquivalent(
                derivedModelsDeaths.Select(r => r.GetBodIndicatorValue()).ToList(),
                new[] { 4D, 6D }
            );
        }

        private static List<BodIndicatorConversion> fakeConverions(
            params (BodIndicator From, BodIndicator To, double Val)[] conversions
        ) {
            var result = conversions
                .Select(r => new BodIndicatorConversion() {
                    FromIndicator = r.From,
                    ToIndicator = r.To,
                    Value = r.Val
                })
                .ToList();
            return result;
        }

        private static List<BurdenOfDisease> fakeBods(
            params (BodIndicator BodIndicator, double value)[] bodTypes
        ) {
            var result = bodTypes
                .Select(r => new BurdenOfDisease() {
                    BodIndicator = r.BodIndicator,
                    Value = r.value
                })
                .ToList();
            return result;
        }

        private static List<DerivedBodIndicatorModel> computeDerivedIndicators(
            List<BodIndicatorConversion> conversions,
            List<BurdenOfDisease> bods
        ) {
            var bodIndicatorModels = bods.Select(BodIndicatorModelFactory.Create).ToList();
            var bodIndicatorConversionsCalculator = new BodConversionsCalculator();
            var result = bodIndicatorConversionsCalculator
                .Compute(
                    bodIndicatorModels,
                    conversions.ToLookup(r => r.FromIndicator)
                );
            return result;
        }
    }
}
