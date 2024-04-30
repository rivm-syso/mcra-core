using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.FocalCommodityCombinationsBuilder;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Concentrations {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Concentrations
    /// </summary>
    [TestClass]
    public class FocalCommodityConcentrationScenarioSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize and test AnalyticalMethodsSummarySection view
        /// </summary>
        [TestMethod]
        public void FocalCommodityConcentrationScenarioSection_TestFocalCommodityReplacementMethodReplaceSamples() {
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var focalFood = new FocalFood() { CodeFood = foods[0].Code, CodeSubstance = substances[0].Code };
            var focalCommodityCombinations = FocalCommodityCombinationsBuilder
                    .Create(
                        new List<FocalFood>() { focalFood },
                        foods.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase),
                        substances.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase)
                    )
                    .ToList();
            var maximumConcentrationLimits = new Dictionary<(Food, Compound), ConcentrationLimit> {
                [(foods[0], substances[0])] = new ConcentrationLimit() {
                    Compound = substances[0],
                    Food = foods[0],
                    Limit = 1,
                }
            };

            var concentrationModels = MockConcentrationsModelsGenerator.Create(
                foods: foods,
                substances: substances,
                modelType: ConcentrationModelType.Empirical,
                mu: -.3,
                sigma: .2,
                useFraction: 0,
                lor: 0,
                sampleSize: 100
            );
            var sampleCompoundCollections = MockSampleCompoundCollectionsGenerator.Create(foods, substances, concentrationModels);
            var section1 = new SamplesByFoodSubstanceSection();
            section1.Summarize(sampleCompoundCollections.Values, null, 2.5, 97.5);

            var config = new ConcentrationsModuleConfig {
                FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSamples,
                FocalCommodityScenarioOccurrencePercentage = 12,
                FocalCommodityConcentrationAdjustmentFactor = 1
            };
            var section = new FocalCommodityConcentrationScenarioSection {
                ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords
            };
            section.SummarizeConcentrationLimits(config, focalCommodityCombinations, maximumConcentrationLimits);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize and test AnalyticalMethodsSummarySection view
        /// </summary>
        [TestMethod]
        public void FocalCommodityConcentrationScenarioSection_TestFocalCommodityReplacementMethodAppendSamples() {
            var foods = MockFoodsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(1);
            var focalFood = new FocalFood() { CodeFood = foods[0].Code, CodeSubstance = substances[0].Code };
            var focalCommodityCombinations = FocalCommodityCombinationsBuilder
                    .Create(
                        new List<FocalFood>() { focalFood },
                        foods.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase),
                        substances.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase)
                    )
                    .ToList();
            var maximumConcentrationLimits = new Dictionary<(Food, Compound), ConcentrationLimit> {
                [(foods[0], substances[0])] = new ConcentrationLimit() {
                    Compound = substances[0],
                    Food = foods[0],
                    Limit = 1,
                }
            };
            var concentrationModels = MockConcentrationsModelsGenerator.Create(foods, substances, ConcentrationModelType.Empirical, -.3, .2, 1, 0, 100);
            var sampleCompoundCollections = MockSampleCompoundCollectionsGenerator.Create(foods, substances, concentrationModels);
            var section1 = new SamplesByFoodSubstanceSection();
            section1.Summarize(sampleCompoundCollections.Values, null, 2.5, 97.5);
            var config = new ConcentrationsModuleConfig {
                FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.AppendSamples,
                FocalCommodityScenarioOccurrencePercentage = 12,
                FocalCommodityConcentrationAdjustmentFactor = 2
            };
            var section = new FocalCommodityConcentrationScenarioSection {
                ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords
            };
            section.SummarizeConcentrationLimits(config, focalCommodityCombinations, maximumConcentrationLimits);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize and test AnalyticalMethodsSummarySection view
        /// </summary>
        [TestMethod]
        public void FocalCommodityConcentrationScenarioSection_TestFocalCommodityReplacementMethodMeasurementRemoval() {
            var foods = MockFoodsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(1);
            var focalFood = new FocalFood() { CodeFood = foods[0].Code, CodeSubstance = substances[0].Code };
            var focalCommodityCombinations = FocalCommodityCombinationsBuilder
                    .Create(
                        new List<FocalFood>() { focalFood },
                        foods.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase),
                        substances.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase)
                    )
                    .ToList();
            var maximumConcentrationLimits = new Dictionary<(Food, Compound), ConcentrationLimit> {
                [(foods[0], substances[0])] = new ConcentrationLimit() {
                    Compound = substances[0],
                    Food = foods[0],
                    Limit = 1,
                }
            };
            var concentrationModels = MockConcentrationsModelsGenerator.Create(foods, substances, ConcentrationModelType.Empirical, -.3, .2, 1, 0, 100);
            var sampleCompoundCollections = MockSampleCompoundCollectionsGenerator.Create(foods, substances, concentrationModels);
            var section1 = new SamplesByFoodSubstanceSection();
            section1.Summarize(sampleCompoundCollections.Values, null, 2.5, 97.5);
            var config = new ConcentrationsModuleConfig {
                FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.MeasurementRemoval,
                FocalCommodityScenarioOccurrencePercentage = 12,
                FocalCommodityConcentrationAdjustmentFactor = 10
            };
            var section = new FocalCommodityConcentrationScenarioSection {
                ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords
            };
            section.SummarizeConcentrationLimits(config, focalCommodityCombinations, maximumConcentrationLimits);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize and test AnalyticalMethodsSummarySection view
        /// </summary>
        [TestMethod]
        public void FocalCommodityConcentrationScenarioSection_TestFocalCommodityReplacementMethodReplaceSubstanceConcentrationsByLimitValue() {
            var foods = MockFoodsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(1);
            var focalFood = new FocalFood() { CodeFood = foods[0].Code, CodeSubstance = substances[0].Code };
            var focalCommodityCombinations = FocalCommodityCombinationsBuilder
                    .Create(
                        new List<FocalFood>() { focalFood },
                        foods.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase),
                        substances.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase)
                    )
                    .ToList();
            var maximumConcentrationLimits = new Dictionary<(Food, Compound), ConcentrationLimit> {
                [(foods[0], substances[0])] = new ConcentrationLimit() {
                    Compound = substances[0],
                    Food = foods[0],
                    Limit = 1,
                }
            };
            var concentrationModels = MockConcentrationsModelsGenerator.Create(foods, substances, ConcentrationModelType.Empirical, -.3, .2, 1, 0, 100);
            var sampleCompoundCollections = MockSampleCompoundCollectionsGenerator.Create(foods, substances, concentrationModels);
            var section1 = new SamplesByFoodSubstanceSection();
            section1.Summarize(sampleCompoundCollections.Values, null, 2.5, 97.5);
            var config = new ConcentrationsModuleConfig {
                FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue,
                FocalCommodityScenarioOccurrencePercentage = 12,
                FocalCommodityConcentrationAdjustmentFactor = 1
            };
            var section = new FocalCommodityConcentrationScenarioSection {
                ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords
            };
            section.SummarizeConcentrationLimits(config, focalCommodityCombinations, maximumConcentrationLimits);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize and test AnalyticalMethodsSummarySection view
        /// </summary>
        [TestMethod]
        public void FocalCommodityConcentrationScenarioSection_TestFocalCommodityReplacementMethodReplaceSubstances100() {
            var foods = MockFoodsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(1);
            var focalFood = new FocalFood() { CodeFood = foods[0].Code, CodeSubstance = substances[0].Code };
            var focalCommodityCombinations = FocalCommodityCombinationsBuilder
                    .Create(
                        new List<FocalFood>() { focalFood },
                        foods.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase),
                        substances.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase)
                    )
                    .ToList();
            var maximumConcentrationLimits = new Dictionary<(Food, Compound), ConcentrationLimit> {
                [(foods[0], substances[0])] = new ConcentrationLimit() {
                    Compound = substances[0],
                    Food = foods[0],
                    Limit = 1,
                }
            };
            var concentrationModels = MockConcentrationsModelsGenerator.Create(foods, substances, ConcentrationModelType.Empirical, -.3, .2, 0, 0, 100);
            var sampleCompoundCollections = MockSampleCompoundCollectionsGenerator.Create(foods, substances, concentrationModels);
            var section1 = new SamplesByFoodSubstanceSection();
            section1.Summarize(sampleCompoundCollections.Values, null, 2.5, 97.5);
            var config = new ConcentrationsModuleConfig {
                FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstances,
                FocalCommodityScenarioOccurrencePercentage = 100,
                FocalCommodityConcentrationAdjustmentFactor = 2
            };
            var section = new FocalCommodityConcentrationScenarioSection {
                ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords
            };
            section.SummarizeConcentrationLimits(config, focalCommodityCombinations, maximumConcentrationLimits);
            AssertIsValidView(section);
        }

        /// <summary>
        /// Summarize and test AnalyticalMethodsSummarySection view
        /// </summary>
        [TestMethod]
        public void FocalCommodityConcentrationScenarioSection_TestFocalCommodityReplacementMethodReplaceSubstances2() {
            var foods = MockFoodsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(1);
            var focalFood = new FocalFood() { CodeFood = foods[0].Code, CodeSubstance = substances[0].Code };
            var focalCommodityCombinations = FocalCommodityCombinationsBuilder
                    .Create(
                        new List<FocalFood>() { focalFood },
                        foods.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase),
                        substances.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase)
                    )
                    .ToList();
            var maximumConcentrationLimits = new Dictionary<(Food, Compound), ConcentrationLimit> {
                [(foods[0], substances[0])] = new ConcentrationLimit() {
                    Compound = substances[0],
                    Food = foods[0],
                    Limit = 1,
                }
            };
            var concentrationModels = MockConcentrationsModelsGenerator.Create(foods, substances, ConcentrationModelType.Empirical, -.3, .2, 0, 0, 1);
            var sampleCompoundCollections = MockSampleCompoundCollectionsGenerator.Create(foods, substances, concentrationModels);
            var section1 = new SamplesByFoodSubstanceSection();
            section1.Summarize(sampleCompoundCollections.Values, null, 2.5, 97.5);
            var config = new ConcentrationsModuleConfig {
                FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstances,
                FocalCommodityScenarioOccurrencePercentage = 100,
                FocalCommodityConcentrationAdjustmentFactor = 2
            };
            var section = new FocalCommodityConcentrationScenarioSection {
                ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords
            };
            section.SummarizeConcentrationLimits(config, focalCommodityCombinations, maximumConcentrationLimits);
            section.SummarizeReplaceSubstances(config, focalCommodityCombinations, sampleCompoundCollections.Values, ConcentrationUnit.mgPerKg);
            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize and test AnalyticalMethodsSummarySection view
        /// </summary>
        [TestMethod]
        public void FocalCommodityConcentrationScenarioSection_TestFocalCommodityReplacementMethodReplaceSubstances1() {
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var focalFood = new FocalFood() { CodeFood = foods[0].Code, CodeSubstance = substances[0].Code };
            var focalCommodityCombinations = FocalCommodityCombinationsBuilder
                    .Create(
                        new List<FocalFood>() { focalFood },
                        foods.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase),
                        substances.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase)
                    )
                    .ToList();
            var maximumConcentrationLimits = new Dictionary<(Food, Compound), ConcentrationLimit> {
                [(foods[0], substances[0])] = new ConcentrationLimit() {
                    Compound = substances[0],
                    Food = foods[0],
                    Limit = 1,
                }
            };
            var concentrationModels = MockConcentrationsModelsGenerator.Create(foods, substances, ConcentrationModelType.Empirical, -.3, .2, 0, 0, 1);
            var sampleCompoundCollections = MockSampleCompoundCollectionsGenerator.Create(foods, substances, concentrationModels);
            var section1 = new SamplesByFoodSubstanceSection();
            section1.Summarize(sampleCompoundCollections.Values, null, 2.5, 97.5);
            var config = new ConcentrationsModuleConfig {
                FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstances,
                FocalCommodityScenarioOccurrencePercentage = 100,
                FocalCommodityConcentrationAdjustmentFactor = 2
            };
            var section = new FocalCommodityConcentrationScenarioSection {
                ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords
            };
            section.SummarizeConcentrationLimits(config, focalCommodityCombinations, maximumConcentrationLimits);
            
            AssertIsValidView(section);
        }
    }
}
