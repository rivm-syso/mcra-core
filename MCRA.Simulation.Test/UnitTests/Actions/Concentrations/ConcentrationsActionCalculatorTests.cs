using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.RawDataProviders;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.Concentrations;
using MCRA.Simulation.Calculators.SampleCompoundCollections;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;


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
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var allFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50, seed: seed);
            var compiledData = new CompiledData() {
                AllFoodSamples = allFoodSamples.ToDictionary(c => c.Code)
            };

            var project = new ProjectDto();
            var config = project.ConcentrationsSettings;
            config.LocationSubsetDefinition = new() {
                LocationSubset = allFoodSamples.Select(c => c.Location).Distinct().ToList()
            };
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
            Assert.HasCount(3, data.ActiveSubstanceSampleCollections);
            Assert.HasCount(3, data.MeasuredSubstanceSampleCollections);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);

            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoadUnc");
        }

        /// <summary>
        /// Runs concentrations module as data.
        /// config.FocalCommodity = true;
        /// config.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.AppendSamples;
        /// project.FocalFoods = new ListFocalFoodDto() { new FocalFoodDto() { CodeFood = foods[0].Code } };
        /// </summary>
        [TestMethod]
        public void ConcentrationsActionCalculator_TestLoadFocal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var allFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50, seed: seed);
            var focalCommodityFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods.Take(1).ToList(), substances, numberOfSamples: 50, seed: seed + 1);
            var compiledData = new CompiledData() {
                AllFoodSamples = allFoodSamples.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            var config = project.ConcentrationsSettings;
            config.FocalCommodity = true;
            config.FocalFoods = [new() { CodeFood = foods[0].Code }];
            config.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstances;

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

            Assert.HasCount(50, data.ActiveSubstanceSampleCollections.Values.First().SampleCompoundRecords);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.FocalCommodityReplacement);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);

            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoadFocal");
        }

        /// <summary>
        ///                      Active substance (AS)  Compound (CMP)
        /// Background (BG)             x                   -
        /// Forground  (FG)             -                   x
        /// </summary>
        [DataRow(true, "AS", "Cmp", false)]
        [DataRow(false, "AS", "Cmp", true)]
        [TestMethod]
        public void ConcentrationsActionCalculator_TestProspective(
            bool useDeterministicSubstanceConversionsForFocalCommodity,
            string backGround,
            string foreGround,
            bool exception
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(1);
            var aSubst = new Compound() { Code = backGround };
            var mSubst = new Compound() { Code = foreGround };
            var substances = new List<Compound> { aSubst, mSubst };
            var bgSubstances = substances.Where(c => c.Code == backGround).ToList();
            var fgSubstances = substances.Where(c => c.Code == foreGround).ToList();

            var bgFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, bgSubstances, numberOfSamples: 10, seed: seed, mu: .5);
            var fgFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, fgSubstances, numberOfSamples: 1, seed: seed + 1, mu: 0.5);
            var compiledData = new CompiledData() {
                AllFoodSamples = bgFoodSamples.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            var concentrationsSettings = project.ConcentrationsSettings;
            concentrationsSettings.FocalCommodity = true;
            concentrationsSettings.UseComplexResidueDefinitions = true;
            concentrationsSettings.UseDeterministicSubstanceConversionsForFocalCommodity = useDeterministicSubstanceConversionsForFocalCommodity;
            var focalFoodConcentrationsSettings = project.FocalFoodConcentrationsSettings;
            focalFoodConcentrationsSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstances;
            focalFoodConcentrationsSettings.FocalFoods = new List<FocalFood>() { new() { CodeFood = foods.First().Code, CodeSubstance = mSubst.Code } };

            var substanceConversions = new List<SubstanceConversion>() { new() {
                ActiveSubstance = aSubst,
                MeasuredSubstance = mSubst,
                IsExclusive = true,
                ConversionFactor = 1 }
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var focalCommoditySubstanceSampleCollections = SampleCompoundCollectionsBuilder.Create(
                foods,
                fgSubstances,
                fgFoodSamples,
                ConcentrationUnit.mgPerL,
                null,
                null
            );

            var data = new ActionData {
                AllFoods = foods,
                AllFoodsByCode = foods.ToDictionary(r => r.Code),
                ActiveSubstances = bgSubstances,
                AllCompounds = bgSubstances.Union(fgSubstances).ToList(),
                FocalCommoditySamples = fgFoodSamples,
                FocalCommoditySubstanceSampleCollections = focalCommoditySubstanceSampleCollections.Values,
                SubstanceConversions = substanceConversions
            };

            var calculator = new ConcentrationsActionCalculator(project);
            if (exception) {
                Assert.ThrowsExactly<AggregateException>(() => TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadFocal0"));
            } else {
                TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadFocal0");
            }
        }

        /// <summary>
        /// Filter processed RPC's for focal commodity
        /// </summary>
        //[DataRow(true, "AS", "Cmp", false, 11)]
        [DataRow(true, "AS", "Cmp", true, 250)]
        [TestMethod]
        public void ConcentrationsActionCalculator_TestReplaceRpcProspective(
            bool useDeterministicSubstanceConversionsForFocalCommodity,
            string backGround,
            string foreGround,
            bool filterProcessedFocalCommoditySamples,
            int expected
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(25);
            var aSubst = new Compound() { Code = backGround };
            var mSubst = new Compound() { Code = foreGround };
            var substances = new List<Compound> { aSubst, mSubst };
            var bgSubstances = substances.Where(c => c.Code == backGround).ToList();
            var fgSubstances = substances.Where(c => c.Code == foreGround).ToList();

            var bgFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, bgSubstances, numberOfSamples: 10, seed: seed, mu: .5);
            var fgFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, fgSubstances, numberOfSamples: 1, seed: seed + 1, mu: 0.5);
            var foodCode = foods.First().Code;
            var processedRpc = new FoodSample() {
                Code = $"{foodCode}#processed",
                Food = new Food() {
                    Code = foodCode,
                    BaseFood = new Food() { Code = foodCode }
                },
                SampleAnalyses = bgFoodSamples.First().SampleAnalyses
            };
            bgFoodSamples.Add(processedRpc);

            var compiledData = new CompiledData() {
                AllFoodSamples = bgFoodSamples.ToDictionary(c => c.Code),
            };
            var project = new ProjectDto();
            var concentrationsSettings = project.ConcentrationsSettings;
            concentrationsSettings.FocalCommodity = true;
            concentrationsSettings.UseComplexResidueDefinitions = true;
            concentrationsSettings.UseDeterministicSubstanceConversionsForFocalCommodity = useDeterministicSubstanceConversionsForFocalCommodity;
            concentrationsSettings.FilterProcessedFocalCommoditySamples = filterProcessedFocalCommoditySamples;
            var focalFoodConcentrationsSettings = project.FocalFoodConcentrationsSettings;
            focalFoodConcentrationsSettings.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.ReplaceSubstances;
            focalFoodConcentrationsSettings.FocalFoods = new List<FocalFood>() { new() { CodeFood = foods.First().Code, CodeSubstance = mSubst.Code } };

            var substanceConversions = new List<SubstanceConversion>() {
                new() {
                    ActiveSubstance = aSubst,
                    MeasuredSubstance = mSubst,
                    IsExclusive = true,
                    ConversionFactor = 1
                }
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var focalCommoditySubstanceSampleCollections = SampleCompoundCollectionsBuilder.Create(
                foods,
                fgSubstances,
                fgFoodSamples,
                ConcentrationUnit.mgPerL,
                null,
                null
            );

            var data = new ActionData {
                AllFoods = foods,
                AllFoodsByCode = foods.ToDictionary(r => r.Code),
                ActiveSubstances = bgSubstances,
                AllCompounds = bgSubstances.Union(fgSubstances).ToList(),
                FocalCommoditySamples = fgFoodSamples,
                FocalCommoditySubstanceSampleCollections = focalCommoditySubstanceSampleCollections.Values,
                SubstanceConversions = substanceConversions
            };

            var calculator = new ConcentrationsActionCalculator(project);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000; i++) {
                TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadFocal0");
            }
            sw.Stop();
            System.Diagnostics.Debug.WriteLine(sw.Elapsed.TotalMilliseconds.ToString());
            System.Console.WriteLine(sw.Elapsed.TotalMilliseconds.ToString());

            Assert.AreEqual(expected, data.MeasuredSubstanceSampleCollections.SelectMany(c => c.Value.SampleCompoundRecords).Count());
        }

        /// <summary>
        /// focal food/substance functionality for MRL replacement, proposed MRLs are missing
        /// focal food/substance functionality for MRL replacement using a settings input field for the proposed MRL
        /// </summary>
        [DataRow("AS", "Cmp", FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue)]
        [DataRow("AS", "Cmp", FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByProposedLimitValue)]
        [TestMethod]
        public void ConcentrationsActionCalculator_TestProposedConcentrationLimit(
            string backGround,
            string foreGround,
            FocalCommodityReplacementMethod focalCommodityReplacementMethod
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(10);
            var aSubst = new Compound() { Code = backGround };
            var mSubst = new Compound() { Code = foreGround };
            var substances = new List<Compound> { aSubst, mSubst };
            var bgSubstances = substances.Where(c => c.Code == backGround).ToList();
            var fgSubstances = substances.Where(c => c.Code == foreGround).ToList();

            var bgFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, bgSubstances, numberOfSamples: 10, seed: seed, mu: .5);
            var fgFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, fgSubstances, numberOfSamples: 1, seed: seed + 1, mu: 0.5);

            var compiledData = new CompiledData() {
                AllFoodSamples = bgFoodSamples.ToDictionary(c => c.Code),
            };
            var project = new ProjectDto();
            var concentrationsSettings = project.ConcentrationsSettings;
            concentrationsSettings.FocalCommodity = true;
            concentrationsSettings.UseComplexResidueDefinitions = true;
            concentrationsSettings.UseDeterministicSubstanceConversionsForFocalCommodity = true;
            concentrationsSettings.FocalCommodityProposedConcentrationLimit = 0.10;
            var focalFoodConcentrationsSettings = project.FocalFoodConcentrationsSettings;
            focalFoodConcentrationsSettings.FocalCommodityReplacementMethod = focalCommodityReplacementMethod;
            focalFoodConcentrationsSettings.FocalFoods = new List<FocalFood>() { new() { CodeFood = foods.First().Code, CodeSubstance = mSubst.Code } };

            var substanceConversions = new List<SubstanceConversion>() {
                new() {
                    ActiveSubstance = aSubst,
                    MeasuredSubstance = mSubst,
                    IsExclusive = true,
                    ConversionFactor = 1
                }
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var focalCommoditySubstanceSampleCollections = SampleCompoundCollectionsBuilder.Create(
                foods,
                fgSubstances,
                fgFoodSamples,
                ConcentrationUnit.mgPerL,
                null,
                null
            );

            var data = new ActionData {
                AllFoods = foods,
                AllFoodsByCode = foods.ToDictionary(r => r.Code),
                ActiveSubstances = bgSubstances,
                AllCompounds = bgSubstances.Union(fgSubstances).ToList(),
                FocalCommoditySamples = fgFoodSamples,
                FocalCommoditySubstanceSampleCollections = focalCommoditySubstanceSampleCollections.Values,
                SubstanceConversions = substanceConversions
            };

            var calculator = new ConcentrationsActionCalculator(project);
            if (focalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue)
                Assert.ThrowsExactly<Exception>(() => TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadFocal2"));
            else {
                TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoadFocal2");
            }
        }

        /// <summary>
        /// Runs concentrations module as data.
        /// config.FocalCommodity = true;
        /// config.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.AppendSamples;
        /// project.FocalFoods = new ListFocalFoodDto() { new FocalFoodDto() { CodeFood = foods[0].Code } };
        /// </summary>
        [TestMethod]
        public void ConcentrationsActionCalculator_TestLoadFocalIsReplace() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var allFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50, seed: seed);
            var focalCommodityFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods.Take(1).ToList(), substances, numberOfSamples: 50, seed: seed + 1);

            var compiledData = new CompiledData() {
                AllFoodSamples = allFoodSamples.ToDictionary(c => c.Code),
            };

            var project = new ProjectDto();
            var config = project.ConcentrationsSettings;
            config.FocalCommodity = true;
            config.FocalCommodityReplacementMethod = FocalCommodityReplacementMethod.MeasurementRemoval;
            config.UseDeterministicSubstanceConversionsForFocalCommodity = true;
            config.FocalFoods = [new() { CodeFood = foods[0].Code }];

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

            Assert.HasCount(50, data.ActiveSubstanceSampleCollections.Values.First().SampleCompoundRecords);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.FocalCommodityReplacement);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);

            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoadFocal");
        }

        /// <summary>
        /// Test load and summarize concentrations with a focal food.
        /// config.FocalCommodity = true;
        /// project.FocalFoods.Add(new FocalFoodDto { CodeFood = "APPLE" });
        /// </summary>
        [TestMethod]
        public void ConcentrationsActionCalculator_TestLoadAndSummarizeFocalFood() {

            var rawDataProvider = new CsvRawDataProvider(@"Resources/Csv/");

            // Set base data source
            rawDataProvider.SetDataGroupsFromFolder(
                idDataSource: 1,
                folder: "_DataGroupsTest",
                tableGroups: [SourceTableGroup.Foods, SourceTableGroup.Compounds, SourceTableGroup.Concentrations]
            );

            // Set focal commodity data source
            rawDataProvider.SetDataTables(
                idDataSource: 2,
                tables: [
                    (ScopingType.FocalFoodAnalyticalMethods, @"Tabulated/AnalyticalMethods"),
                    (ScopingType.FocalFoodAnalyticalMethodCompounds, @"Tabulated/AnalyticalMethodCompounds"),
                    (ScopingType.FocalFoodSamples, @"Tabulated/FoodSamples"),
                    (ScopingType.FocalFoodSampleAnalyses, @"Tabulated/AnalysisSamples"),
                    (ScopingType.FocalFoodConcentrationsPerSample, @"Tabulated/ConcentrationsPerSample")
                ]);

            var compiledDataManager = new CompiledDataManager(rawDataProvider);

            var project = new ProjectDto();
            var config = project.ConcentrationsSettings;
            config.FocalCommodity = true;
            config.FocalFoods.Add(new() { CodeFood = "APPLE" });

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
            Assert.HasCount(12, groupedSamples["APPLE"].SampleCompoundRecords);
            Assert.HasCount(5, groupedSamples["BANANAS"].SampleCompoundRecords);
            Assert.HasCount(10, groupedSamples["PINEAPPLE"].SampleCompoundRecords);
        }

        /// <summary>
        /// Test load and summarize concentrations with a focal food.
        /// config.FocalCommodity = true;
        /// project.FocalFoods.Add(new FocalFoodDto { CodeFood = "APPLE" });
        /// </summary>
        [TestMethod]
        public void ConcentrationsActionCalculator_TestLoadAndSummarizeFocalFoodUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var rawDataProvider = new CsvRawDataProvider(@"Resources/Csv/");

            // Set base data source
            rawDataProvider.SetDataGroupsFromFolder(
                idDataSource: 1,
                folder: "_DataGroupsTest",
                tableGroups: [SourceTableGroup.Foods, SourceTableGroup.Compounds, SourceTableGroup.Concentrations]
            );

            // Set focal commodity data source
            rawDataProvider.SetDataTables(
                idDataSource: 2,
                tables: [
                    (ScopingType.FocalFoodAnalyticalMethods, @"Tabulated/AnalyticalMethods"),
                    (ScopingType.FocalFoodAnalyticalMethodCompounds, @"Tabulated/AnalyticalMethodCompounds"),
                    (ScopingType.FocalFoodSamples, @"Tabulated/FoodSamples"),
                    (ScopingType.FocalFoodSampleAnalyses, @"Tabulated/AnalysisSamples"),
                    (ScopingType.FocalFoodConcentrationsPerSample, @"Tabulated/ConcentrationsPerSample")
                ]);

            var compiledDataManager = new CompiledDataManager(rawDataProvider);

            var project = new ProjectDto();
            var config = project.ConcentrationsSettings;
            config.FocalCommodity = true;
            config.FocalFoods.Add(new() { CodeFood = "APPLE" });

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
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var allFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50, seed: seed);

            var compiledData = new CompiledData() {
                AllFoodSamples = allFoodSamples.ToDictionary(c => c.Code),
            };

            var config = new ConcentrationsModuleConfig {
                SampleSubsetSelection = true,
                LocationSubsetDefinition = new LocationSubsetDefinition {
                    AlignSubsetWithPopulation = alignSubsetWithPopulation,
                    LocationSubset = ["Location1"]
                },
                SamplesSubsetDefinitions = [
                    new () {
                        AlignSubsetWithPopulation = alignSubsetWithPopulation,
                        PropertyName = propertyName,
                        KeyWords = ["ProductionMethod", "Location1"]
                    }
                ],
                PeriodSubsetDefinition = new() {
                    AlignSampleDateSubsetWithPopulation = alignSampleDateSubsetWithPopulation,
                    AlignSampleSeasonSubsetWithPopulation = true,
                    YearsSubset = ["2022"],
                    MonthsSubset = [1, 2, 3, 4, 5, 6]
                }
            };
            var project = new ProjectDto(config);

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var selectedPopulation = FakePopulationsGenerator.Create(1).First();
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
            var foods = FakeFoodsGenerator.Create(nrOfFoods);
            var substances = FakeSubstancesGenerator.Create(7);
            var substanceApprovals = FakeSubstanceApprovalsGenerator.Create(substances).ToList();
            var allFoodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50, seed: seed);
            var compiledData = new CompiledData() {
                AllFoodSamples = allFoodSamples.ToDictionary(c => c.Code)
            };
            foods.Add(new Food { Code = "Water", Name = "Water", Properties = new FoodProperty { UnitWeight = 100 } });

            var config = new ConcentrationsModuleConfig {
                ImputeWaterConcentrations = true,
                RestrictWaterImputationToApprovedSubstances = true,
                CodeWater = "Water",
                LocationSubsetDefinition = new()
            };
            config.LocationSubsetDefinition.LocationSubset = allFoodSamples.Select(c => c.Location).Distinct().ToList();
            var project = new ProjectDto(config);

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
            Assert.HasCount(nrOfFoods + 1, data.ActiveSubstanceSampleCollections);        // +1 for water
            Assert.HasCount(nrOfFoods, data.MeasuredSubstanceSampleCollections);

            var random = new McraRandomGenerator(seed);
            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);

            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoadUnc");
        }
    }
}
