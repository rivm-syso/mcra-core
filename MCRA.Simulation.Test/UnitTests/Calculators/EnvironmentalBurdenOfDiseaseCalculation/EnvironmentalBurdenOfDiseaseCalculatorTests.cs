using MCRA.General;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    [TestClass]
    public class EnvironmentalBurdenOfDiseaseCalculatorTests {

        [TestMethod]
        [DataRow(BodApproach.TopDown, PAFCalculationMethod.Levin, true)]
        [DataRow(BodApproach.TopDown, PAFCalculationMethod.Levin, false)]
        [DataRow(BodApproach.TopDown, PAFCalculationMethod.PersonWeightedAF, true)]
        [DataRow(BodApproach.TopDown, PAFCalculationMethod.PersonWeightedAF, false)]
        [DataRow(BodApproach.BottomUp, null, null)]
        public void EnvironmentalBurdenOfDiseaseCalculator_TestSimpleCalculation(
                BodApproach bodApproach,
                PAFCalculationMethod pafCalculationMetod,
                bool forcePafLimit
            ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = FakeEffectsGenerator.Create(2);
            var substances = FakeSubstancesGenerator.Create(3);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var exposureResponseFunctionModels = FakeExposureResponseFunctionsGenerator
                .FakeExposureResponseFunctionModel(
                    substances,
                    effects,
                    targetUnit.Target,
                    targetUnit.ExposureUnit,
                    random.Next()
                );

            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var populationCharacteristicTypes = exposureResponseFunctionModels
                .Select(r => r.PopulationCharacteristic)
                .Distinct()
                .ToList();
            var population = FakePopulationsGenerator
                .Create(1, populationCharacteristicTypes, seed: 1).First();
            var burdenOfDiseases = effects.Select(r => FakeBurdenOfDiseasesGenerator.Create(r, population)).ToList();
            var bodIndicatorModels = FakeBurdenOfDiseasesGenerator
                            .FakeBodIndicatorValueModel(burdenOfDiseases, population);

            var hbmIndividualConcentrations = FakeHbmDataGenerator
                            .FakeHbmIndividualConcentrations(individuals, substances);
            var hbmIndividualCollections = new Dictionary<ExposureTarget, (List<ITargetIndividualExposure> Exposures, TargetUnit Unit)> {
                { targetUnit.Target, (hbmIndividualConcentrations.Cast<ITargetIndividualExposure>().ToList(), targetUnit) }
            };

            var erCalculator = new ExposureResponseCalculator(
                ExposureGroupingMethod.CustomBins,
                [5.0, 50.0, 95.0],
                WithinBinExposureRepresentationMethod.BinMidpoint
            );
            var exposureResponseResults = erCalculator.ComputeFromTargetIndividualExposures(
                hbmIndividualCollections,
                exposureResponseFunctionModels,
                1
            );

            var ebdCalculator = new EnvironmentalBurdenOfDiseaseCalculator(
                bodApproach,
                EnvironmentalBodStandardisationMethod.PER100K,
                pafCalculationMetod,
                forcePafLimit
            );
            var environmentalBurdenOfDiseases = ebdCalculator.Compute(
                bodIndicatorModels,
                exposureResponseResults
            );

            // Assert: one result record per model - 3 substances, 2 effects
            Assert.HasCount(6, environmentalBurdenOfDiseases);
        }
    }
}