using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
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

            var project = new ProjectDto();
            project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSamples;
            project.ConcentrationModelSettings.FocalCommodityScenarioOccurrencePercentage = 12;
            project.ConcentrationModelSettings.FocalCommodityConcentrationAdjustmentFactor = 1;
            var section = new FocalCommodityConcentrationScenarioSection();
            section.ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords;
            section.SummarizeConcentrationLimits(project, focalCommodityCombinations, maximumConcentrationLimits);
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
            var project = new ProjectDto();
            project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.AppendSamples;
            project.ConcentrationModelSettings.FocalCommodityScenarioOccurrencePercentage = 12;
            project.ConcentrationModelSettings.FocalCommodityConcentrationAdjustmentFactor = 2;
            var section = new FocalCommodityConcentrationScenarioSection();
            section.ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords;
            section.SummarizeConcentrationLimits(project, focalCommodityCombinations, maximumConcentrationLimits);
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
            var project = new ProjectDto();
            project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.MeasurementRemoval;
            project.ConcentrationModelSettings.FocalCommodityScenarioOccurrencePercentage = 12;
            project.ConcentrationModelSettings.FocalCommodityConcentrationAdjustmentFactor = 10;
            var section = new FocalCommodityConcentrationScenarioSection();
            section.ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords;
            section.SummarizeConcentrationLimits(project, focalCommodityCombinations, maximumConcentrationLimits);
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
            var project = new ProjectDto();
            project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue;
            project.ConcentrationModelSettings.FocalCommodityScenarioOccurrencePercentage = 12;
            project.ConcentrationModelSettings.FocalCommodityConcentrationAdjustmentFactor = 1;
            var section = new FocalCommodityConcentrationScenarioSection();
            section.ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords;
            section.SummarizeConcentrationLimits(project, focalCommodityCombinations, maximumConcentrationLimits);
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
            var project = new ProjectDto();
            project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstances;
            project.ConcentrationModelSettings.FocalCommodityScenarioOccurrencePercentage = 100;
            project.ConcentrationModelSettings.FocalCommodityConcentrationAdjustmentFactor = 2;
            var section = new FocalCommodityConcentrationScenarioSection();
            section.ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords;
            section.SummarizeConcentrationLimits(project, focalCommodityCombinations, maximumConcentrationLimits);
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
            var project = new ProjectDto();
            project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstances;
            project.ConcentrationModelSettings.FocalCommodityScenarioOccurrencePercentage = 100;
            project.ConcentrationModelSettings.FocalCommodityConcentrationAdjustmentFactor = 2;
            var section = new FocalCommodityConcentrationScenarioSection();
            section.ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords;
            section.SummarizeConcentrationLimits(project, focalCommodityCombinations, maximumConcentrationLimits);
            section.SummarizeReplaceSubstances(project, focalCommodityCombinations, sampleCompoundCollections.Values, ConcentrationUnit.mgPerKg);
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
            var project = new ProjectDto();
            project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstances;
            project.ConcentrationModelSettings.FocalCommodityScenarioOccurrencePercentage = 100;
            project.ConcentrationModelSettings.FocalCommodityConcentrationAdjustmentFactor = 2;
            var section = new FocalCommodityConcentrationScenarioSection();
            section.ConcentrationInputDataRecords = section1.ConcentrationInputDataRecords;
            section.SummarizeConcentrationLimits(project, focalCommodityCombinations, maximumConcentrationLimits);
            
            AssertIsValidView(section);
        }
    }
}
