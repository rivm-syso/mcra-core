using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.RawDataProviders;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.Concentrations;
using MCRA.Simulation.Calculators.SampleCompoundCollections;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Tests for the concentrations action calculator.
    /// </summary>
    [TestClass]
    public class ConcentrationsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs concentrations module as data.
        /// </summary>
        [TestMethod]
        public void ConcentrationsActionCalculator_TestLoad() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var allFoodSamples = MockSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50, seed: seed);
            var compiledData = new CompiledData() {
                AllFoodSamples = allFoodSamples.ToDictionary(c => c.Code)
            };

            var project = new ProjectDto();
            project.LocationSubsetDefinition.LocationSubset = allFoodSamples.Select(c => c.Location).Distinct().ToList();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData {
                AllFoods = foods,
                AllFoodsByCode = foods.ToDictionary(r => r.Code),
                ActiveSubstances = substances,
                AllCompounds = substances
            };

            var calculator = new ConcentrationsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            Assert.IsNotNull(data.MeasuredSubstanceSampleCollections);
            Assert.IsNotNull(data.ActiveSubstanceSampleCollections);
            Assert.AreEqual(3, data.ActiveSubstanceSampleCollections.Count);
            Assert.AreEqual(3, data.MeasuredSubstanceSampleCollections.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);

            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoadUnc");
        }

        /// <summary>
        /// Runs concentrations module as data.
        /// project.AssessmentSettings.FocalCommodity = true;
        /// project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.AppendSamples;
        /// project.FocalFoods = new ListFocalFoodDto() { new FocalFoodDto() { CodeFood = foods[0].Code } };
        /// </summary>
        [TestMethod]
        public void ConcentrationsActionCalculator_TestLoadFocal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var allFoodSamples = MockSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50, seed: seed);
            var focalCommodityFoodSamples = MockSamplesGenerator.CreateFoodSamples(foods.Take(1).ToList(), substances, numberOfSamples: 50, seed: seed + 1);
            var compiledData = new CompiledData() {
                AllFoodSamples = allFoodSamples.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            project.AssessmentSettings.FocalCommodity = true;
            project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.AppendSamples;
            project.FocalFoods = new List<FocalFoodDto>() { new FocalFoodDto() { CodeFood = foods[0].Code } };
            project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstances;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var focalCommoditySubstanceSampleCollections = SampleCompoundCollectionsBuilder.Create(
                foods,
                substances,
                allFoodSamples,
                ConcentrationUnit.mgPerL,
                null,
                null
            );
  
            var data = new ActionData {
                AllFoods = foods,
                AllFoodsByCode = foods.ToDictionary(r => r.Code),
                ActiveSubstances = substances,
                AllCompounds = substances,
                FocalCommoditySamples = focalCommodityFoodSamples,
                FocalCommoditySubstanceSampleCollections = focalCommoditySubstanceSampleCollections.Values,
            };

            var calculator = new ConcentrationsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadFocal0");

            Assert.AreEqual(50, data.ActiveSubstanceSampleCollections.Values.First().SampleCompoundRecords.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.FocalCommodityReplacement);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);

            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoadFocal");
        }

        /// <summary>
        /// Runs concentrations module as data.
        /// project.AssessmentSettings.FocalCommodity = true;
        /// project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.AppendSamples;
        /// project.FocalFoods = new ListFocalFoodDto() { new FocalFoodDto() { CodeFood = foods[0].Code } };
        /// </summary>
        [TestMethod]
        public void ConcentrationsActionCalculator_TestLoadFocalIsReplace() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var allFoodSamples = MockSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50, seed: seed);
            var focalCommodityFoodSamples = MockSamplesGenerator.CreateFoodSamples(foods.Take(1).ToList(), substances, numberOfSamples: 50, seed: seed + 1);

            var compiledData = new CompiledData() {
                AllFoodSamples = allFoodSamples.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            project.AssessmentSettings.FocalCommodity = true;
            project.ConcentrationModelSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.MeasurementRemoval;
            project.ConcentrationModelSettings.UseDeterministicSubstanceConversionsForFocalCommodity = true;
            project.FocalFoods = new List<FocalFoodDto>() { new FocalFoodDto() { CodeFood = foods[0].Code } };

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData {
                AllFoods = foods,
                AllFoodsByCode = foods.ToDictionary(r => r.Code),
                ActiveSubstances = substances,
                AllCompounds = substances,
                FocalCommoditySamples = focalCommodityFoodSamples
            };

            var calculator = new ConcentrationsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadFocal");

            Assert.AreEqual(50, data.ActiveSubstanceSampleCollections.Values.First().SampleCompoundRecords.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.FocalCommodityReplacement);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);

            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoadFocal");
        }

        /// <summary>
        /// Test load and summarize concentrations with a focal food.
        /// project.AssessmentSettings.FocalCommodity = true;
        /// project.FocalFoods.Add(new FocalFoodDto { CodeFood = "APPLE" });
        /// </summary>
        [TestMethod]
        public void ConcentrationsActionCalculator_TestLoadAndSummarizeFocalFood() {

            var rawDataProvider = new CsvRawDataProvider(@"Resources\Csv\");

            // Set base data source
            rawDataProvider.SetDataGroupsFromFolder(
                idDataSource: 1,
                folder: "_DataGroupsTest",
                tableGroups: new[] { SourceTableGroup.Foods, SourceTableGroup.Compounds, SourceTableGroup.Concentrations }
            );

            // Set focal commodity data source
            rawDataProvider.SetDataTables(
                idDataSource: 2,
                tables: new[] {
                    (ScopingType.FocalFoodAnalyticalMethods, @"Tabulated\AnalyticalMethods"),
                    (ScopingType.FocalFoodAnalyticalMethodCompounds, @"Tabulated\AnalyticalMethodCompounds"),
                    (ScopingType.FocalFoodSamples, @"Tabulated\FoodSamples"),
                    (ScopingType.FocalFoodSampleAnalyses, @"Tabulated\AnalysisSamples"),
                    (ScopingType.FocalFoodConcentrationsPerSample, @"Tabulated\ConcentrationsPerSample")
                });

            var compiledDataManager = new CompiledDataManager(rawDataProvider);

            var project = new ProjectDto();
            project.AssessmentSettings.FocalCommodity = true;
            project.FocalFoods.Add(new FocalFoodDto { CodeFood = "APPLE" });

            var subsetManager = new SubsetManager(compiledDataManager, project);
            var data = new ActionData() {
                AllFoods = compiledDataManager.GetAllFoods().Values,
                AllFoodsByCode = compiledDataManager.GetAllFoods(),
                AllCompounds = compiledDataManager.GetAllCompounds().Values,
                FocalCommoditySamples = subsetManager.SelectedFocalCommoditySamples,
            };

            var calculator = new ConcentrationsActionCalculator(project);

            _ = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadAndSummarizeFocalFood");

            var groupedSamples = data.MeasuredSubstanceSampleCollections.Values.ToDictionary(r => r.Food.Code);
            Assert.AreEqual(12, groupedSamples["APPLE"].SampleCompoundRecords.Count);
            Assert.AreEqual(5, groupedSamples["BANANAS"].SampleCompoundRecords.Count);
            Assert.AreEqual(10, groupedSamples["PINEAPPLE"].SampleCompoundRecords.Count);
        }

        /// <summary>
        /// Test load and summarize concentrations with a focal food.
        /// project.AssessmentSettings.FocalCommodity = true;
        /// project.FocalFoods.Add(new FocalFoodDto { CodeFood = "APPLE" });
        /// </summary>
        [TestMethod]
        public void ConcentrationsActionCalculator_TestLoadAndSummarizeFocalFoodUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var rawDataProvider = new CsvRawDataProvider(@"Resources\Csv\");

            // Set base data source
            rawDataProvider.SetDataGroupsFromFolder(
                idDataSource: 1,
                folder: "_DataGroupsTest",
                tableGroups: new[] { SourceTableGroup.Foods, SourceTableGroup.Compounds, SourceTableGroup.Concentrations }
            );

            // Set focal commodity data source
            rawDataProvider.SetDataTables(
                idDataSource: 2,
                tables: new[] {
                    (ScopingType.FocalFoodAnalyticalMethods, @"Tabulated\AnalyticalMethods"),
                    (ScopingType.FocalFoodAnalyticalMethodCompounds, @"Tabulated\AnalyticalMethodCompounds"),
                    (ScopingType.FocalFoodSamples, @"Tabulated\FoodSamples"),
                    (ScopingType.FocalFoodSampleAnalyses, @"Tabulated\AnalysisSamples"),
                    (ScopingType.FocalFoodConcentrationsPerSample, @"Tabulated\ConcentrationsPerSample")
                });

            var compiledDataManager = new CompiledDataManager(rawDataProvider);

            var project = new ProjectDto();
            project.AssessmentSettings.FocalCommodity = true;
            project.FocalFoods.Add(new FocalFoodDto { CodeFood = "APPLE" });

            var subsetManager = new SubsetManager(compiledDataManager, project);
            var data = new ActionData() {
                AllFoods = compiledDataManager.GetAllFoods().Values,
                AllFoodsByCode = compiledDataManager.GetAllFoods(),
                AllCompounds = compiledDataManager.GetAllCompounds().Values,
                FocalCommoditySamples = subsetManager.SelectedFocalCommoditySamples,
            };

            var calculator = new ConcentrationsActionCalculator(project);

            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadAndSummarizeFocalFoodUncertain");

            for (int i = 0; i < 10; i++) {
                var dataUncertain = data.Copy();
                var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations);
                var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => new McraRandomGenerator(seed + i) as IRandom);
                TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoadAndSummarizeFocalFoodUncertain");
            }
        }


        /// <summary>
        /// Runs concentrations module as data. Tests filters.
        /// </summary>
        [DataRow(true, true, "Region")]
        [DataRow(true, false, "Region")]
        [DataRow(false, false, "ProductionMethod")]
        [TestMethod]
        public void ConcentrationsActionCalculator_TestLoad_IsSamplePropertySubset(bool alignSampleDateSubsetWithPopulation, bool alignSubsetWithPopulation, string propertyName) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var allFoodSamples = MockSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50, seed: seed);

            var compiledData = new CompiledData() {
                AllFoodSamples = allFoodSamples.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();

            project.SubsetSettings.SampleSubsetSelection = true;
            project.LocationSubsetDefinition.AlignSubsetWithPopulation = alignSubsetWithPopulation;
            project.LocationSubsetDefinition.LocationSubset = new List<string> { "Location1" };

            project.SamplesSubsetDefinitions = new List<SamplesSubsetDefinitionDto> {
                new SamplesSubsetDefinitionDto() {
                    AlignSubsetWithPopulation = alignSubsetWithPopulation,
                    PropertyName = propertyName,
                    KeyWords = new HashSet<string> { "ProductionMethod", "Location1"}
                }
            };
            project.PeriodSubsetDefinition = new PeriodSubsetDefinitionDto() {
                AlignSampleDateSubsetWithPopulation = alignSampleDateSubsetWithPopulation,
                AlignSampleSeasonSubsetWithPopulation = true,
                YearsSubset = new List<string> { "2022" },
                MonthsSubset = new List<int> { 1, 2, 3, 4, 5, 6 }
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var selectedPopulation = MockPopulationsGenerator.Create(1).First();
            var populationIndividualPropertyValues = new Dictionary<string, PopulationIndividualPropertyValue> {
                ["Month"] = new PopulationIndividualPropertyValue() {
                    Value = "1,2,3,4,5,6,7,8,9,10"
                },
                ["Region"] = new PopulationIndividualPropertyValue() {
                    Value = "Location1"
                }
            };
            selectedPopulation.StartDate = new DateTime(2022, 1, 1);
            selectedPopulation.EndDate = new DateTime(2022, 12, 31);
            selectedPopulation.PopulationIndividualPropertyValues = populationIndividualPropertyValues;
            selectedPopulation.Location = "Location1";

            var data = new ActionData {
                AllFoods = foods,
                AllFoodsByCode = foods.ToDictionary(r => r.Code),
                ActiveSubstances = substances,
                AllCompounds = substances,
                SelectedPopulation = selectedPopulation,
            };

            var calculator = new ConcentrationsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestFilters");
            var count = data.FoodSamples.SelectMany(c => c).Count();
            Assert.AreEqual(49, count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);

            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoadFocal");

        }

        [TestMethod]
        public void UseWaterImputation_RestrictBySubstanceApprovals() {
            var seed = 1;
            var nrOfFoods = 3;
            var foods = MockFoodsGenerator.Create(nrOfFoods);
            var substances = MockSubstancesGenerator.Create(7);
            var substanceApprovals = MockSubstanceApprovalsGenerator.Create(substances).ToList();
            var allFoodSamples = MockSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50, seed: seed);
            var compiledData = new CompiledData() {
                AllFoodSamples = allFoodSamples.ToDictionary(c => c.Code)
            };

            var project = new ProjectDto();
            project.ConcentrationModelSettings.ImputeWaterConcentrations = true;
            project.ConcentrationModelSettings.RestrictWaterImputationToApprovedSubstances= true;
            project.LocationSubsetDefinition.LocationSubset = allFoodSamples.Select(c => c.Location).Distinct().ToList();
            project.ConcentrationModelSettings.CodeWater = "Water";

            foods.Add(new Food { Code = "Water", Name = "Water", Properties = new FoodProperty { UnitWeight = 100 } });

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var rpfs = substances
               .Select((r, ix) => new {
                   Substance = r,
                   Rpf = (double)ix + 1
               })
               .ToDictionary(c => c.Substance, c => c.Rpf);

            var data = new ActionData {
                AllFoods = foods,
                AllFoodsByCode = foods.ToDictionary(r => r.Code),
                ActiveSubstances = substances,
                AllCompounds = substances,
                SubstanceApprovals = substanceApprovals.ToDictionary(s => s.Substance),
                CorrectedRelativePotencyFactors = rpfs
            };

            var calculator = new ConcentrationsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            Assert.IsNotNull(data.MeasuredSubstanceSampleCollections);
            Assert.IsNotNull(data.ActiveSubstanceSampleCollections);
            Assert.AreEqual(nrOfFoods + 1, data.ActiveSubstanceSampleCollections.Count);        // +1 for water
            Assert.AreEqual(nrOfFoods, data.MeasuredSubstanceSampleCollections.Count);

            var random = new McraRandomGenerator(seed);
            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);

            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoadUnc");
        }
    }
}

