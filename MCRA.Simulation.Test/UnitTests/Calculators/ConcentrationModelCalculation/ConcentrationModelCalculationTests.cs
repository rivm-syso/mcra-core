﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ActiveSubstanceAllocation;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation;
using MCRA.Simulation.Calculators.SampleCompoundCollections;
using MCRA.Simulation.Calculators.SampleCompoundCollections.MissingValueImputation;
using MCRA.Simulation.Calculators.SampleCompoundCollections.NonDetectsImputation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ConcentrationModelCalculation {

    /// <summary>
    /// ConcentrationModelCalculation calculator
    /// </summary>
    [TestClass]
    public class ConcentrationModelCalculationTests {

        /// <summary>
        /// Creates concentration models
        /// </summary>
        [TestMethod]
        public void ConcentrationModelCalculation_TestCreate() {
            var outputPath = TestUtilities.CreateTestOutputPath("ConcentrationModelCalculationTests");
            var dataFolder = Path.Combine("Resources", "ConcentrationModelling");
            var targetFileName = Path.Combine(outputPath, "ConcentrationModelling.zip");
            var dataManager = TestUtilities.CompiledDataManagerFromFolder(dataFolder, targetFileName);
            var allSubstances = dataManager.GetAllCompounds().Values;
            var foods = dataManager.GetAllFoods().Values;
            var foodSamples = dataManager.GetAllFoodSamples().Values;

            var residueDefinitions = dataManager.GetAllSubstanceConversions();
            var concentrationUnit = ConcentrationUnit.mgPerKg;

            var measuredSubstances = residueDefinitions.Select(r => r.MeasuredSubstance).Distinct().ToList();
            var activeSubstances = residueDefinitions.Select(r => r.ActiveSubstance).ToHashSet();
            var sampleCompoundCollections = SampleCompoundCollectionsBuilder.Create(foods, measuredSubstances, foodSamples, concentrationUnit, null);

            var csvWriter = new SampleCompoundCollectionCsvWriter() {
                PrintLocation = false
            };
            csvWriter.WriteCsv(sampleCompoundCollections.Values, measuredSubstances, Path.Combine(outputPath, "SampleConcentrations-Raw.csv"), false, false);
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(allSubstances, sampleCompoundCollections);

            var settings = new MockConcentrationModelCalculationSettings() {
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByZero,
                DefaultConcentrationModel = ConcentrationModelType.Empirical,
                ConcentrationModelTypesPerFoodCompound = [],
                CorrelateImputedValueWithSamplePotency = true,
                RestrictLorImputationToAuthorisedUses = false
            };

            var concentrationModelsBuilder = new ConcentrationModelsBuilder(settings);
            var concentrationModels = concentrationModelsBuilder.Create(
                foods,
                measuredSubstances,
                compoundResidueCollections,
                null,
                null,
                null,
                null,
                ConcentrationUnit.mgPerKg
            );

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var randomSubstanceAllocator = new RandomActiveSubstanceAllocationCalculator(residueDefinitions, null, true);
            var activeSubstanceSampleCompoundCollections = randomSubstanceAllocator.Allocate(sampleCompoundCollections.Values, activeSubstances, random);
            csvWriter.WriteCsv(activeSubstanceSampleCompoundCollections, activeSubstances, Path.Combine(outputPath, "SampleConcentrations-ActiveSubstance.csv"), false, false);

            var nonDetectsImputationCalculator = new CensoredValuesImputationCalculator(settings);
            nonDetectsImputationCalculator.ReplaceCensoredValues(sampleCompoundCollections.Values, concentrationModels, 0);
            csvWriter.WriteCsv(sampleCompoundCollections.Values, measuredSubstances, Path.Combine(outputPath, "SampleConcentrations-ND-Replaced.csv"), true, false);

            var correctedRpfs = measuredSubstances.ToDictionary(r => r, r => 1D);
            var missingValuesImputationCalculator = new MissingvalueImputationCalculator(settings);
            missingValuesImputationCalculator.ImputeMissingValues(sampleCompoundCollections.Values, concentrationModels, correctedRpfs, 0);
            csvWriter.WriteCsv(sampleCompoundCollections.Values, measuredSubstances, Path.Combine(outputPath, "SampleConcentrations-MV-Replaced.csv"), true, true);
        }

        /// <summary>
        /// Creates concentration model charts
        /// </summary>
        [TestMethod]
        public void ConcentrationModelCalculation_TestCreateCharts() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var outputPath = TestUtilities.ConcatWithOutputPath("TestCreateConcentrationModels");
            if (Directory.Exists(outputPath)) {
                Directory.Delete(outputPath, true);
                Thread.Sleep(100);
            }
            Directory.CreateDirectory(outputPath);

            var sampleSizes = new int[] { 1, 2, 3 };
            var useFractions = new double[] { 0, 0.25, 0.6, 1 };
            var mus = new double[] { 1, 2 };
            var sigmas = new double[] { 0.1, 1 };
            var lors = new double[] { 0.1, 0.5, 1, 2 };
            var concentrationModelTypes = Enum.GetValues(typeof(ConcentrationModelType))
                .Cast<ConcentrationModelType>()
                .Where(cm => cm != ConcentrationModelType.LogNormal);

            var food = new Food("MyFood");
            var compound = new Compound("MyCompound");
            foreach (var sampleSize in sampleSizes) {
                foreach (var mu in mus) {
                    foreach (var sigma in sigmas) {
                        foreach (var useFraction in useFractions) {
                            foreach (var lor in lors) {
                                var concentrations = createConcentrations(mu, sigma, useFraction, sampleSize, random);
                                var compoundResidueCollection = createConcentrations(food, compound, concentrations, lor, random);
                                var settings = new MockConcentrationModelCalculationSettings() {
                                    NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                                    FractionOfLor = 1d,
                                };
                                var factory = new ConcentrationModelFactory(settings);
                                foreach (var modelType in concentrationModelTypes) {
                                    var occurrenceFraction = Math.Min(
                                        1D - compoundResidueCollection.FractionZeros,
                                        compoundResidueCollection.FractionPositives + .5 * compoundResidueCollection.FractionCensoredValues
                                    );
                                    var model = factory.CreateModelAndCalculateParameters(food, compound, modelType, compoundResidueCollection, null, null, occurrenceFraction, ConcentrationUnit.mgPerKg);
                                    Assert.IsNotNull(model);
                                    if (model.ModelType == modelType) {
                                        var record = new ConcentrationModelRecord();
                                        record.Summarize(food, compound, model, false);
                                        var chartCreator = new ConcentrationModelChartCreator(record, 300, 300, true);
                                        var id = $"{sampleSize}-{mu}-{sigma}-{lor}-{useFraction}-{modelType}".Replace(".", "p");
                                        var filename = Path.Combine(outputPath, $"{id}.png");
                                        chartCreator.CreateToPng(filename);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Test option to restrict censored value imputation with positive values (e.g., based on LOR) to authorised
        /// uses only. When this option is checked, then for unauthorised uses, the use-fraction should be equal to the
        /// observed fraction positives.
        /// </summary>
        [DataRow(true, true)]
        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataRow(false, false)]
        [TestMethod]
        public void ConcentrationModelCalculation_TestRestrictLorImputationToAuthorisedUses(
            bool authorised,
            bool restrictLorInmputationToAuthorisedUses
        ) {
            var foods = FakeFoodsGenerator.Create(1);
            var substances = FakeSubstancesGenerator.Create(1);
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator
                .Create(
                    foods,
                    substances,
                    mean: 0.1,
                    upper: 0.2,
                    lods: [0.05],
                    loqs: [0.1],
                    fractionZero: .2,
                    sampleSize: 100
                );

            var settings = new MockConcentrationModelCalculationSettings() {
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                DefaultConcentrationModel = ConcentrationModelType.Empirical,
                ConcentrationModelTypesPerFoodCompound = [],
                CorrelateImputedValueWithSamplePotency = true,
                RestrictLorImputationToAuthorisedUses = restrictLorInmputationToAuthorisedUses
            };

            var foodSubstances = foods.SelectMany(r => substances, (f, s) => (f, s)).ToList();
            var authorisations = authorised
                ? foodSubstances
                    .ToDictionary(r => r, r => new SubstanceAuthorisation() {
                        Food = r.f,
                        Substance = r.s
                    })
                : new Dictionary<(Food, Compound), SubstanceAuthorisation>();

            var concentrationModelsBuilder = new ConcentrationModelsBuilder(settings);
            var concentrationModels = concentrationModelsBuilder
                .Create(
                    foodSubstances,
                    compoundResidueCollections,
                    null,
                    null,
                    null,
                    authorisations,
                    ConcentrationUnit.mgPerKg
                );

            var observedFractionPositives = compoundResidueCollections.First().Value.FractionPositives;
            var computedUseFraction = concentrationModels.First().Value.WeightedAgriculturalUseFraction;
            var expectedUseFraction = authorised || !restrictLorInmputationToAuthorisedUses ? 1 : observedFractionPositives;
            Assert.AreEqual(expectedUseFraction, computedUseFraction);
        }

        /// <summary>
        /// Creates concentration model charts
        /// </summary>
        [TestMethod]
        public void ConcentrationModelCalculation_CreateChartsDocumentation() {
            var outputPath = TestUtilities.ConcatWithOutputPath("CreateChartsDocumentation");
            if (Directory.Exists(outputPath)) {
                Directory.Delete(outputPath, true);
                Thread.Sleep(100);
            }
            Directory.CreateDirectory(outputPath);

            var food = new Food("MyFood");
            var compound = new Compound("MyCompound");
            var mu = 2;
            var sigma = 1;
            var useFraction = 0.25;
            var lor = 2;
            var sampleSize = 200;
            var concentrationModelTypes = Enum.GetValues(typeof(ConcentrationModelType))
                .Cast<ConcentrationModelType>()
                .Where(cm => cm != ConcentrationModelType.LogNormal);

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var concentrations = createConcentrations(mu, sigma, useFraction, sampleSize, random);
            var compoundResidueCollection = createConcentrations(food, compound, concentrations, lor, random);
            var settings = new MockConcentrationModelCalculationSettings() {
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByZero,
            };
            var factory = new ConcentrationModelFactory(settings);
            foreach (var modelType in concentrationModelTypes) {
                var occurrenceFraction = Math.Min(
                    1D - compoundResidueCollection.FractionZeros,
                    compoundResidueCollection.FractionPositives + .5 * compoundResidueCollection.FractionCensoredValues
                );
                var model = factory.CreateModelAndCalculateParameters(food, compound, modelType, compoundResidueCollection, null, null, occurrenceFraction, ConcentrationUnit.mgPerKg);
                Assert.IsNotNull(model);
                if (model.ModelType == modelType) {
                    var record = new ConcentrationModelRecord();
                    record.Summarize(food, compound, model, false);
                    var chartCreator = new ConcentrationModelChartCreator(record, 300, 300, true);
                    var id = $"{modelType}";
                    var filename = Path.Combine(outputPath, $"{id}.svg");
                    chartCreator.CreateToSvg(filename);
                    chartCreator.CreateToPng(Path.Combine(outputPath, $"{id}.png"));
                }
            }
        }

        private List<double> createConcentrations(double mu, double sigma, double fractionZero, int n, IRandom random) {
            var positives = (int)(n - Math.Round(fractionZero * n));
            var zeros = n - positives;
            var x = Enumerable
                .Range(0, n)
                .Select(r => r < positives ? NormalDistribution.InvCDF(0, 1, random.NextDouble(0, 1)) * sigma + mu : 0D)
                .ToList();
            return x;
        }

        private CompoundResidueCollection createConcentrations(Food food, Compound compound, List<double> concentrations, double lor, IRandom random) {
            var positivesCount = concentrations.Count(r => r > 0);
            var zerosCount = concentrations.Count(r => r == 0);
            return new CompoundResidueCollection() {
                Food = food,
                Compound = compound,
                Positives = concentrations.Where(r => r >= lor).ToList(),
                CensoredValuesCollection = concentrations.Where(r => r < lor).Select(r => new CensoredValue() { LOD = lor, LOQ = lor }).ToList(),
                ZerosCount = zerosCount,
            };
        }
    }
}
