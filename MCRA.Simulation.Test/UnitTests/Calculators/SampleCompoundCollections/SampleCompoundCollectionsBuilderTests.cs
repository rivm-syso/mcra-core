using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.RawDataProviders;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.OccurrenceFrequenciesCalculation;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Simulation.Calculators.SampleCompoundCollections;
using MCRA.Simulation.Calculators.SampleOriginCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Calculators.SampleCompoundCollections {

    /// <summary>
    /// ConcentrationModelCalculation calculator
    /// </summary>
    [TestClass]
    public class SampleCompoundCollectionsBuilderTests {

        /// <summary>
        /// Test build for one food and substance. Check all sample substance records are
        /// authorised when food/substance combination is authorised.
        /// </summary>
        [TestMethod]
        public void SampleCompoundCollectionsBuilder_TestBuild_Authorised() {
            var foods = MockFoodsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(new[] { "S1" });
            var samples = MockSamplesGenerator.CreateFoodSamples(foods, substances);
            var autorisations = MockSubstanceAuthorisationsGenerator.Create((foods[0], substances[0]));
            var scc = SampleCompoundCollectionsBuilder.Create(
                foods,
                substances,
                samples,
                ConcentrationUnit.mgPerKg,
                autorisations
            );
            Assert.IsTrue(scc.All(r => r.SampleCompoundRecords.All(scr => scr.AuthorisedUse)));
        }

        /// <summary>
        /// Test build for one food and substance. Check that all sample substance records 
        /// with positive substance concentration measurements are not authorised when 
        /// food/substance combination is not authorised.
        /// </summary>
        [TestMethod]
        public void SampleCompoundCollectionsBuilder_TestBuild_NotAuthorised() {
            var foods = MockFoodsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(new[] { "S1" });
            var samples = MockSamplesGenerator.CreateFoodSamples(foods, substances, lod: 0);
            var autorisations = MockSubstanceAuthorisationsGenerator.Create();
            var scc = SampleCompoundCollectionsBuilder.Create(
                foods,
                substances,
                samples,
                ConcentrationUnit.mgPerKg,
                autorisations
            );
            Assert.IsTrue(scc.All(r => r.SampleCompoundRecords.All(scr => !scr.AuthorisedUse)));
        }

        /// <summary>
        /// Test build for one food and substance. Check all sample substance records are
        /// authorised when food/substance combination is authorised.
        /// </summary>
        [TestMethod]
        public void SampleCompoundCollectionsBuilder_TestBuild_AuthorisationsNull() {
            var foods = MockFoodsGenerator.Create(1);
            var substances = MockSubstancesGenerator.Create(new[] { "S1" });
            var samples = MockSamplesGenerator.CreateFoodSamples(foods, substances);
            var scc = SampleCompoundCollectionsBuilder.Create(
                foods,
                substances,
                samples,
                ConcentrationUnit.mgPerKg,
                null
            );
            Assert.IsTrue(scc.All(r => r.SampleCompoundRecords.All(scr => scr.AuthorisedUse)));
        }

        /// <summary>
        /// Test build for one food and substance. Check all sample substance records are
        /// authorised when base-food/substance combination is authorised.
        /// </summary>
        [TestMethod]
        public void SampleCompoundCollectionsBuilder_TestBuild_BaseFoodAuthorised() {
            var rawFoods = MockFoodsGenerator.Create(1);
            var processingTypes = MockProcessingTypesGenerator.Create(1);
            var processedFoods = MockFoodsGenerator.CreateProcessedFoods(rawFoods, processingTypes);
            var substances = MockSubstancesGenerator.Create(new[] { "S1" });
            var samples = MockSamplesGenerator.CreateFoodSamples(processedFoods, substances);
            var autorisations = MockSubstanceAuthorisationsGenerator.Create((rawFoods[0], substances[0]));
            var scc = SampleCompoundCollectionsBuilder.Create(
                processedFoods,
                substances,
                samples,
                ConcentrationUnit.mgPerKg,
                autorisations
            );
            Assert.IsTrue(scc.All(r => r.SampleCompoundRecords.All(scr => scr.AuthorisedUse)));
        }

        /// <summary>
        /// Test creation of sample substance records for the foods apple, pineapple, and 
        /// bananas and the substances A, B, C, and D. Verification by counting per 
        /// food/substance combination, the number of missing values, censored values, and 
        /// positives.
        /// </summary>
        [TestMethod]
        public void SampleCompoundCollectionsBuilder_TestBuild1() {
            var provider = new CsvRawDataProvider(@"Resources\Csv\");
            provider.SetDataGroupsFromFolder(
                idDataSource: 1,
                folder: "_DataGroupsTest",
                tableGroups: new[] { SourceTableGroup.Foods, SourceTableGroup.Compounds, SourceTableGroup.Concentrations }
            );

            var project = new ProjectDto();
            var compiledDataManager = new CompiledDataManager(provider);
            var subsetManager = new SubsetManager(compiledDataManager, project);

            var foods = compiledDataManager.GetAllFoods();
            var foodApple = foods["APPLE"];
            var foodPineapple = foods["PINEAPPLE"];
            var foodBananas = foods["BANANAS"];

            // Get substance residue collections
            var sampleCompoundCollections = SampleCompoundCollectionsBuilder.Create(
                subsetManager.AllModelledFoods,
                subsetManager.AllCompounds,
                subsetManager.SelectedFoodSamples,
                ConcentrationUnit.mgPerKg,
                null);

            //-------------------------------------------------------------------------------
            // Tests Apple
            // ------------------------------------------------------------------------------

            var sampleCompoundRecordsApple = sampleCompoundCollections.Single(scr => scr.Food.Code == "APPLE").SampleCompoundRecords;
            Assert.AreEqual(5, sampleCompoundRecordsApple.Count());

            var missingValuesAppleCompoundA = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundA" && sc.Value.IsMissingValue));
            var missingValuesAppleCompoundB = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundB" && sc.Value.IsMissingValue));
            var missingValuesAppleCompoundC = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundC" && sc.Value.IsMissingValue));
            var missingValuesAppleCompoundD = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundD" && sc.Value.IsMissingValue));
            Assert.AreEqual(0, missingValuesAppleCompoundA.Count());
            Assert.AreEqual(0, missingValuesAppleCompoundB.Count());
            Assert.AreEqual(0, missingValuesAppleCompoundC.Count());
            Assert.AreEqual(0, missingValuesAppleCompoundD.Count());

            var nonDetectsAppleCompoundA = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundA" && sc.Value.IsCensoredValue));
            var nonDetectsAppleCompoundB = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundB" && sc.Value.IsCensoredValue));
            var nonDetectsAppleCompoundC = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundC" && sc.Value.IsCensoredValue));
            var nonDetectsAppleCompoundD = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundD" && sc.Value.IsCensoredValue));
            Assert.AreEqual(0, nonDetectsAppleCompoundA.Count());
            Assert.AreEqual(0, nonDetectsAppleCompoundB.Count());
            Assert.AreEqual(0, nonDetectsAppleCompoundC.Count());
            Assert.AreEqual(5, nonDetectsAppleCompoundD.Count());

            var positivesAppleCompoundA = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundA" && !double.IsNaN(sc.Value.Residue)));
            var positivesAppleCompoundB = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundB" && !double.IsNaN(sc.Value.Residue)));
            var positivesAppleCompoundC = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundC" && !double.IsNaN(sc.Value.Residue)));
            var positivesAppleCompoundD = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundD" && !double.IsNaN(sc.Value.Residue)));
            Assert.AreEqual(5, positivesAppleCompoundA.Count());
            Assert.AreEqual(5, positivesAppleCompoundB.Count());
            Assert.AreEqual(5, positivesAppleCompoundC.Count());
            Assert.AreEqual(0, positivesAppleCompoundD.Count());

            //-------------------------------------------------------------------------------
            // Tests Bananas
            // ------------------------------------------------------------------------------

            var sampleCompoundRecordsBananas = sampleCompoundCollections.Single(scr => scr.Food.Code == "BANANAS").SampleCompoundRecords;
            Assert.AreEqual(5, sampleCompoundRecordsBananas.Count());

            var missingValuesBananasCompoundA = sampleCompoundRecordsBananas.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundA" && sc.Value.IsMissingValue));
            var missingValuesBananasCompoundB = sampleCompoundRecordsBananas.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundB" && sc.Value.IsMissingValue));
            var missingValuesBananasCompoundC = sampleCompoundRecordsBananas.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundC" && sc.Value.IsMissingValue));
            var missingValuesBananasCompoundD = sampleCompoundRecordsBananas.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundD" && sc.Value.IsMissingValue));
            Assert.AreEqual(0, missingValuesBananasCompoundA.Count());
            Assert.AreEqual(0, missingValuesBananasCompoundB.Count());
            Assert.AreEqual(5, missingValuesBananasCompoundC.Count());
            Assert.AreEqual(5, missingValuesBananasCompoundD.Count());

            var nonDetectsBananasCompoundA = sampleCompoundRecordsBananas.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundA" && sc.Value.IsCensoredValue));
            var nonDetectsBananasCompoundB = sampleCompoundRecordsBananas.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundB" && sc.Value.IsCensoredValue));
            var nonDetectsBananasCompoundC = sampleCompoundRecordsBananas.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundC" && sc.Value.IsCensoredValue));
            var nonDetectsBananasCompoundD = sampleCompoundRecordsBananas.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundD" && sc.Value.IsCensoredValue));
            Assert.AreEqual(2, nonDetectsBananasCompoundA.Count());
            Assert.AreEqual(2, nonDetectsBananasCompoundB.Count());
            Assert.AreEqual(0, nonDetectsBananasCompoundC.Count());
            Assert.AreEqual(0, nonDetectsBananasCompoundD.Count());

            var positivesBananasCompoundA = sampleCompoundRecordsBananas.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundA" && !double.IsNaN(sc.Value.Residue)));
            var positivesBananasCompoundB = sampleCompoundRecordsBananas.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundB" && !double.IsNaN(sc.Value.Residue)));
            var positivesBananasCompoundC = sampleCompoundRecordsBananas.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundC" && !double.IsNaN(sc.Value.Residue)));
            var positivesBananasCompoundD = sampleCompoundRecordsBananas.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundD" && !double.IsNaN(sc.Value.Residue)));
            Assert.AreEqual(3, positivesBananasCompoundA.Count());
            Assert.AreEqual(3, positivesBananasCompoundB.Count());
            Assert.AreEqual(0, positivesBananasCompoundC.Count());
            Assert.AreEqual(0, positivesBananasCompoundD.Count());

            //-------------------------------------------------------------------------------
            // Tests Pineapple
            // ------------------------------------------------------------------------------

            var sampleCompoundRecordsPineapple = sampleCompoundCollections.Single(scr => scr.Food.Code == "PINEAPPLE").SampleCompoundRecords;
            Assert.AreEqual(10, sampleCompoundRecordsPineapple.Count());

            var missingValuesPineappleCompoundA = sampleCompoundRecordsPineapple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundA" && sc.Value.IsMissingValue));
            var missingValuesPineappleCompoundB = sampleCompoundRecordsPineapple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundB" && sc.Value.IsMissingValue));
            var missingValuesPineappleCompoundC = sampleCompoundRecordsPineapple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundC" && sc.Value.IsMissingValue));
            var missingValuesPineappleCompoundD = sampleCompoundRecordsPineapple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundD" && sc.Value.IsMissingValue));
            Assert.AreEqual(0, missingValuesPineappleCompoundA.Count());
            Assert.AreEqual(10, missingValuesPineappleCompoundB.Count());
            Assert.AreEqual(9, missingValuesPineappleCompoundC.Count());
            Assert.AreEqual(3, missingValuesPineappleCompoundD.Count());

            var nonDetectsPineappleCompoundA = sampleCompoundRecordsPineapple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundA" && sc.Value.IsCensoredValue));
            var nonDetectsPineappleCompoundB = sampleCompoundRecordsPineapple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundB" && sc.Value.IsCensoredValue));
            var nonDetectsPineappleCompoundC = sampleCompoundRecordsPineapple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundC" && sc.Value.IsCensoredValue));
            var nonDetectsPineappleCompoundD = sampleCompoundRecordsPineapple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundD" && sc.Value.IsCensoredValue));
            Assert.AreEqual(7, nonDetectsPineappleCompoundA.Count());
            Assert.AreEqual(0, nonDetectsPineappleCompoundB.Count());
            Assert.AreEqual(0, nonDetectsPineappleCompoundC.Count());
            Assert.AreEqual(0, nonDetectsPineappleCompoundD.Count());

            var positivesPineappleCompoundA = sampleCompoundRecordsPineapple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundA" && !double.IsNaN(sc.Value.Residue)));
            var positivesPineappleCompoundB = sampleCompoundRecordsPineapple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundB" && !double.IsNaN(sc.Value.Residue)));
            var positivesPineappleCompoundC = sampleCompoundRecordsPineapple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundC" && !double.IsNaN(sc.Value.Residue)));
            var positivesPineappleCompoundD = sampleCompoundRecordsPineapple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundD" && !double.IsNaN(sc.Value.Residue)));
            Assert.AreEqual(3, positivesPineappleCompoundA.Count());
            Assert.AreEqual(0, positivesPineappleCompoundB.Count());
            Assert.AreEqual(1, positivesPineappleCompoundC.Count());
            Assert.AreEqual(7, positivesPineappleCompoundD.Count());
        }

        /// <summary>
        /// Test creation of substance residue collections for the foods apple, pineapple, and
        /// bananas and the substances A, B, C, and D. Verification by checking per food/substance
        /// combination, the number of residues, the number of censored values, the number of positives,
        /// and the fraction of positives.
        /// </summary>
        [TestMethod]
        public void SampleCompoundCollectionsBuilder_TestBuild2() {
            var provider = new CsvRawDataProvider(@"Resources\Csv\");
            provider.SetDataGroupsFromFolder(
                idDataSource: 1,
                folder: "_DataGroupsTest",
                tableGroups: new[] { SourceTableGroup.Foods, SourceTableGroup.Compounds, SourceTableGroup.Concentrations }
            );

            var project = new ProjectDto();
            var compiledDataManager = new CompiledDataManager(provider);
            var subsetManager = new SubsetManager(compiledDataManager, project);

            var foods = compiledDataManager.GetAllFoods();
            var foodApple = foods["APPLE"];
            var foodPineapple = foods["PINEAPPLE"];
            var foodBananas = foods["BANANAS"];

            var compounds = compiledDataManager.GetAllCompounds();
            var compoundA = compounds["CompoundA"];
            var compoundB = compounds["CompoundB"];
            var compoundC = compounds["CompoundC"];
            var compoundD = compounds["CompoundD"];

            // Get substance residue collections
            var sampleOrigins = SampleOriginCalculator.Calculate(subsetManager.SelectedFoodSamples.ToLookup(r => r.Food));
            var settings = new OccurrenceFractionsCalculatorSettings(new AgriculturalUseSettingsDto() {
                SetMissingAgriculturalUseAsUnauthorized = true,
                UseAgriculturalUsePercentage = true
            });
            var agriculturalUseCalculator = new OccurrenceFractionsCalculator(settings);
            var agriculturalUseInfo = agriculturalUseCalculator.ComputeLocationBased(
                subsetManager.AllModelledFoods,
                subsetManager.AllCompounds,
                subsetManager.AllOccurrencePatterns,
                sampleOrigins
            );
            var sampleCompoundCollections = SampleCompoundCollectionsBuilder.Create(
                subsetManager.AllModelledFoods,
                subsetManager.AllCompounds,
                subsetManager.SelectedFoodSamples,
                ConcentrationUnit.mgPerKg,
                null
            );
            var compoundResidueCollectionsBuilder = new CompoundResidueCollectionsBuilder(false);
            var compoundResidueCollections = compoundResidueCollectionsBuilder.Create(
                subsetManager.AllCompounds,
                sampleCompoundCollections,
                agriculturalUseInfo,
                null);

            //-------------------------------------------------------------------------------
            // Apple
            // ------------------------------------------------------------------------------

            var compoundResidueCollectionAppleCompoundA = compoundResidueCollections[(foodApple, compoundA)];
            var compoundResidueCollectionAppleCompoundB = compoundResidueCollections[(foodApple, compoundB)];
            var compoundResidueCollectionAppleCompoundC = compoundResidueCollections[(foodApple, compoundC)];
            var compoundResidueCollectionAppleCompoundD = compoundResidueCollections[(foodApple, compoundD)];

            Assert.AreEqual(5, compoundResidueCollectionAppleCompoundA.NumberOfResidues);
            Assert.AreEqual(5, compoundResidueCollectionAppleCompoundB.NumberOfResidues);
            Assert.AreEqual(5, compoundResidueCollectionAppleCompoundC.NumberOfResidues);
            Assert.AreEqual(5, compoundResidueCollectionAppleCompoundD.NumberOfResidues);

            Assert.AreEqual(5, compoundResidueCollectionAppleCompoundA.Positives.Count);
            Assert.AreEqual(5, compoundResidueCollectionAppleCompoundB.Positives.Count);
            Assert.AreEqual(5, compoundResidueCollectionAppleCompoundC.Positives.Count);
            Assert.AreEqual(0, compoundResidueCollectionAppleCompoundD.Positives.Count);

            Assert.AreEqual(0, compoundResidueCollectionAppleCompoundA.CensoredValues.Count);
            Assert.AreEqual(0, compoundResidueCollectionAppleCompoundB.CensoredValues.Count);
            Assert.AreEqual(0, compoundResidueCollectionAppleCompoundC.CensoredValues.Count);
            Assert.AreEqual(5, compoundResidueCollectionAppleCompoundD.CensoredValues.Count);

            Assert.AreEqual(1, compoundResidueCollectionAppleCompoundA.FractionPositives);
            Assert.AreEqual(1, compoundResidueCollectionAppleCompoundB.FractionPositives);
            Assert.AreEqual(1, compoundResidueCollectionAppleCompoundC.FractionPositives);
            Assert.AreEqual(0, compoundResidueCollectionAppleCompoundD.FractionPositives);

            //-------------------------------------------------------------------------------
            // Tests Bananas
            // ------------------------------------------------------------------------------

            var compoundResidueCollectionBananasCompoundA = compoundResidueCollections[(foodBananas, compoundA)];
            var compoundResidueCollectionBananasCompoundB = compoundResidueCollections[(foodBananas, compoundB)];
            var compoundResidueCollectionBananasCompoundC = compoundResidueCollections[(foodBananas, compoundC)];
            var compoundResidueCollectionBananasCompoundD = compoundResidueCollections[(foodBananas, compoundD)];

            Assert.AreEqual(5, compoundResidueCollectionBananasCompoundA.NumberOfResidues);
            Assert.AreEqual(5, compoundResidueCollectionBananasCompoundB.NumberOfResidues);
            Assert.AreEqual(0, compoundResidueCollectionBananasCompoundC.NumberOfResidues);
            Assert.AreEqual(0, compoundResidueCollectionBananasCompoundD.NumberOfResidues);

            Assert.AreEqual(3, compoundResidueCollectionBananasCompoundA.Positives.Count);
            Assert.AreEqual(3, compoundResidueCollectionBananasCompoundB.Positives.Count);
            Assert.AreEqual(0, compoundResidueCollectionBananasCompoundC.Positives.Count);
            Assert.AreEqual(0, compoundResidueCollectionBananasCompoundD.Positives.Count);

            Assert.AreEqual(2, compoundResidueCollectionBananasCompoundA.CensoredValues.Count);
            Assert.AreEqual(2, compoundResidueCollectionBananasCompoundB.CensoredValues.Count);
            Assert.AreEqual(0, compoundResidueCollectionBananasCompoundC.CensoredValues.Count);
            Assert.AreEqual(0, compoundResidueCollectionBananasCompoundD.CensoredValues.Count);

            Assert.AreEqual(.6, compoundResidueCollectionBananasCompoundA.FractionPositives);
            Assert.AreEqual(.6, compoundResidueCollectionBananasCompoundB.FractionPositives);
            Assert.IsTrue(double.IsNaN(compoundResidueCollectionBananasCompoundC.FractionPositives));
            Assert.IsTrue(double.IsNaN(compoundResidueCollectionBananasCompoundD.FractionPositives));

            //-------------------------------------------------------------------------------
            // Tests Pineapple
            // ------------------------------------------------------------------------------

            var compoundResidueCollectionPineappleCompoundA = compoundResidueCollections[(foodPineapple, compoundA)];
            var compoundResidueCollectionPineappleCompoundB = compoundResidueCollections[(foodPineapple, compoundB)];
            var compoundResidueCollectionPineappleCompoundC = compoundResidueCollections[(foodPineapple, compoundC)];
            var compoundResidueCollectionPineappleCompoundD = compoundResidueCollections[(foodPineapple, compoundD)];

            Assert.AreEqual(10, compoundResidueCollectionPineappleCompoundA.NumberOfResidues);
            Assert.AreEqual(0, compoundResidueCollectionPineappleCompoundB.NumberOfResidues);
            Assert.AreEqual(1, compoundResidueCollectionPineappleCompoundC.NumberOfResidues);
            Assert.AreEqual(7, compoundResidueCollectionPineappleCompoundD.NumberOfResidues);

            Assert.AreEqual(3, compoundResidueCollectionPineappleCompoundA.Positives.Count);
            Assert.AreEqual(0, compoundResidueCollectionPineappleCompoundB.Positives.Count);
            Assert.AreEqual(1, compoundResidueCollectionPineappleCompoundC.Positives.Count);
            Assert.AreEqual(7, compoundResidueCollectionPineappleCompoundD.Positives.Count);

            Assert.AreEqual(7, compoundResidueCollectionPineappleCompoundA.CensoredValues.Count);
            Assert.AreEqual(0, compoundResidueCollectionPineappleCompoundB.CensoredValues.Count);
            Assert.AreEqual(0, compoundResidueCollectionPineappleCompoundC.CensoredValues.Count);
            Assert.AreEqual(0, compoundResidueCollectionPineappleCompoundD.CensoredValues.Count);

            Assert.AreEqual(.3, compoundResidueCollectionPineappleCompoundA.FractionPositives);
            Assert.IsTrue(double.IsNaN(compoundResidueCollectionPineappleCompoundB.FractionPositives));
            Assert.AreEqual(1, compoundResidueCollectionPineappleCompoundC.FractionPositives);
            Assert.AreEqual(1, compoundResidueCollectionPineappleCompoundD.FractionPositives);
        }

        /// <summary>
        /// Test substance residue collections and sample substance collections generated for the 
        /// food apple and the substances A, B, C, and D. For the substance residue collections,
        /// validate the number of residues, the number of positives, and the number of
        /// censored values. For the sample substance records, test the missing value counts.
        /// </summary>
        [TestMethod]
        [TestCategory("Integration Tests")]
        [TestCategory("Compiled Data Source Tests")]
        [TestCategory("Tabulated Concentrations Data Tests")]
        public void SampleCompoundCollectionsBuilder_TestBuild3() {

            var provider = new CsvRawDataProvider(@"Resources\Csv\");

            // Set foods, substances, and agricultural uses data
            provider.SetDataGroupsFromFolder(
                idDataSource: 1,
                folder: "_DataGroupsTest",
                tableGroups: new[] { SourceTableGroup.Foods, SourceTableGroup.Compounds, SourceTableGroup.AgriculturalUse }
            );

            // Set concentration data
            provider.SetDataTables(
                idDataSource: 1,
                tables: new[] {
                    (ScopingType.AnalyticalMethods, @"Tabulated\AnalyticalMethods"),
                    (ScopingType.AnalyticalMethodCompounds, @"Tabulated\AnalyticalMethodCompounds"),
                    (ScopingType.FoodSamples, @"Tabulated\FoodSamples"),
                    (ScopingType.SampleAnalyses, @"Tabulated\AnalysisSamples"),
                    (ScopingType.ConcentrationsPerSample, @"Tabulated\ConcentrationsPerSample")
                });

            var project = new ProjectDto();
            var compiledDataManager = new CompiledDataManager(provider);
            var subsetManager = new SubsetManager(compiledDataManager, project);

            var foodApple = subsetManager.AllFoodsByCode["APPLE"];

            var compounds = subsetManager.AllCompoundsByCode;
            var compoundA = compounds["CompoundA"];
            var compoundB = compounds["CompoundB"];
            var compoundC = compounds["CompoundC"];
            var compoundD = compounds["CompoundD"];

            // Get substance residue collections
            var sampleOrigins = SampleOriginCalculator.Calculate(subsetManager.SelectedFoodSamples.ToLookup(r => r.Food));
            var settings = new OccurrenceFractionsCalculatorSettings(new AgriculturalUseSettingsDto() {
                SetMissingAgriculturalUseAsUnauthorized = true,
                UseAgriculturalUsePercentage = true
            });
            var agriculturalUseCalculator = new OccurrenceFractionsCalculator(settings);

            var agriculturalUseInfo = agriculturalUseCalculator.ComputeLocationBased(
                subsetManager.AllModelledFoods,
                subsetManager.AllCompounds,
                subsetManager.AllOccurrencePatterns,
                sampleOrigins
            );
            var sampleCompoundCollections = SampleCompoundCollectionsBuilder.Create(
                subsetManager.AllModelledFoods,
                subsetManager.AllCompounds,
                subsetManager.SelectedFoodSamples,
                ConcentrationUnit.mgPerKg,
                null
            );
            var compoundResidueCollectionsBuilder = new CompoundResidueCollectionsBuilder(false);
            var compoundResidueCollections = compoundResidueCollectionsBuilder.Create(
                subsetManager.AllCompounds, sampleCompoundCollections, agriculturalUseInfo, null);

            //-------------------------------------------------------------------------------
            // Tests CompoundResidueCollections
            // ------------------------------------------------------------------------------

            var compoundResidueCollectionAppleCompoundA = compoundResidueCollections[(foodApple, compoundA)];
            var compoundResidueCollectionAppleCompoundB = compoundResidueCollections[(foodApple, compoundB)];
            var compoundResidueCollectionAppleCompoundC = compoundResidueCollections[(foodApple, compoundC)];
            var compoundResidueCollectionAppleCompoundD = compoundResidueCollections[(foodApple, compoundD)];

            Assert.AreEqual(3, compoundResidueCollectionAppleCompoundA.NumberOfResidues);
            Assert.AreEqual(3, compoundResidueCollectionAppleCompoundB.NumberOfResidues);
            Assert.AreEqual(6, compoundResidueCollectionAppleCompoundC.NumberOfResidues);
            Assert.AreEqual(0, compoundResidueCollectionAppleCompoundD.NumberOfResidues);

            Assert.AreEqual(2, compoundResidueCollectionAppleCompoundA.Positives.Count);
            Assert.AreEqual(0, compoundResidueCollectionAppleCompoundB.Positives.Count);
            Assert.AreEqual(3, compoundResidueCollectionAppleCompoundC.Positives.Count);
            Assert.AreEqual(0, compoundResidueCollectionAppleCompoundD.Positives.Count);

            Assert.AreEqual(1, compoundResidueCollectionAppleCompoundA.CensoredValues.Count);
            Assert.AreEqual(3, compoundResidueCollectionAppleCompoundB.CensoredValues.Count);
            Assert.AreEqual(3, compoundResidueCollectionAppleCompoundC.CensoredValues.Count);
            Assert.AreEqual(0, compoundResidueCollectionAppleCompoundD.CensoredValues.Count);

            //-------------------------------------------------------------------------------
            // Tests SampleCompoundCollections
            // ------------------------------------------------------------------------------

            var sampleCompoundRecordsApple = sampleCompoundCollections.Single(scr => scr.Food == foodApple).SampleCompoundRecords;
            Assert.AreEqual(12, sampleCompoundRecordsApple.Count());

            var missingValuesAppleCompoundA = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundA" && sc.Value.IsMissingValue));
            var missingValuesAppleCompoundB = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundB" && sc.Value.IsMissingValue));
            var missingValuesAppleCompoundC = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundC" && sc.Value.IsMissingValue));
            var missingValuesAppleCompoundD = sampleCompoundRecordsApple.SelectMany(scr => scr.SampleCompounds.Where(sc => sc.Key.Code == "CompoundD" && sc.Value.IsMissingValue));

            Assert.AreEqual(9, missingValuesAppleCompoundA.Count());
            Assert.AreEqual(9, missingValuesAppleCompoundB.Count());
            Assert.AreEqual(6, missingValuesAppleCompoundC.Count());
            Assert.AreEqual(12, missingValuesAppleCompoundD.Count());

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var compoundResidueCollectionsResample = CompoundResidueCollectionsBuilder.Resample (
                compoundResidueCollections, random, null);

            var resampleCompoundResidueCollectionAppleCompoundA = compoundResidueCollectionsResample[(foodApple, compoundA)];
            var resampleCompoundResidueCollectionAppleCompoundB = compoundResidueCollectionsResample[(foodApple, compoundB)];
            var resampleCompoundResidueCollectionAppleCompoundC = compoundResidueCollectionsResample[(foodApple, compoundC)];
            var resampleCompoundResidueCollectionAppleCompoundD = compoundResidueCollectionsResample[(foodApple, compoundD)];

            Assert.AreEqual(3, resampleCompoundResidueCollectionAppleCompoundA.NumberOfResidues);
            Assert.AreEqual(3, resampleCompoundResidueCollectionAppleCompoundB.NumberOfResidues);
            Assert.AreEqual(6, resampleCompoundResidueCollectionAppleCompoundC.NumberOfResidues);
            Assert.AreEqual(0, resampleCompoundResidueCollectionAppleCompoundD.NumberOfResidues);
        }
    }
}
