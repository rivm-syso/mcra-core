using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Compression;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    /// <summary>
    /// Tests CSV writing
    /// </summary>
    [TestClass]
    public class CompiledDataManagerZipFileTests {

        private string _tempZipFileName;

        /// <summary>
        /// Before every test that is run
        /// </summary>
        [TestInitialize]
        public void TestInitialize() {
            _tempZipFileName = Path.Combine(Path.GetTempPath(), $"TestCDM{Guid.NewGuid()}.zip");
        }

        /// <summary>
        /// Test cleanup, delete any created file
        /// </summary>
        [TestCleanup]
        public void TestCleanup() {
            if (File.Exists(_tempZipFileName)) {
                File.Delete(_tempZipFileName);
            }
        }

        /// <summary>
        /// CompiledDataManager_WriteEmptyZipFileTest
        /// </summary>
        [TestMethod()]
        public void CompiledDataManager_WriteEmptyZipFileTest() {
            CompiledDataManager.WriteDataToZippedCsv(new CompiledData(), _tempZipFileName);
            Assert.IsTrue(File.Exists(_tempZipFileName));

            using (var zf = ZipFile.Open(_tempZipFileName, ZipArchiveMode.Read)) {
                Assert.AreEqual(0, zf.Entries.Count);
            }
        }

        /// <summary>
        /// CompiledDataManager_WriteEmptyZipFileTest
        /// </summary>
        [TestMethod()]
        public void CompiledDataManager_WriteSingleDataObjectZipFileTest() {
            var data = new CompiledData() {
                AllFoods = new Dictionary<string, Food> { { "A", new Food("A") } }
            };
            CompiledDataManager.WriteDataToZippedCsv(data, _tempZipFileName);
            Assert.IsTrue(File.Exists(_tempZipFileName));

            using (var zf = ZipFile.Open(_tempZipFileName, ZipArchiveMode.Read)) {
                Assert.AreEqual(1, zf.Entries.Count);
                Assert.AreEqual("Foods.csv", zf.Entries[0].Name, true);
            }
        }

        [TestMethod]
        public void CompiledDataManager_WriteAllEmptyDataObjectsZipFileTest() {
            var data = createBasicCompiledDataInstance();
            CompiledDataManager.WriteDataToZippedCsv(data, _tempZipFileName);
            Assert.IsTrue(File.Exists(_tempZipFileName));
            using (var zf = ZipFile.Open(_tempZipFileName, ZipArchiveMode.Read)) {
                Assert.AreEqual(0, zf.Entries.Count);
            }
        }

        private static CompiledData createShallowCompiledDataInstance() {
            var aFood = new Food("A") { Properties = new FoodProperty() };
            var bFood = new Food("B") { Properties = new FoodProperty() };
            var cFood = new Food("C") { Properties = new FoodProperty() };
            cFood.Parent = aFood;
            aFood.Children.Add(cFood);
            var aFacet = new Facet { Code = "A" };
            var aFacetDescriptor = new FacetDescriptor { Code = "D" };
            var aFoodFacet = new FoodFacet { Facet = aFacet, FacetDescriptor = aFacetDescriptor };
            aFood.FoodFacets.Add(aFoodFacet);

            var aQuantification = new FoodConsumptionQuantification { Food = aFood, UnitCode = "U" };
            aFood.FoodConsumptionQuantifications.Add("U", aQuantification);
            var aTranslation = new FoodTranslation { FoodFrom = aFood, FoodTo = cFood };

            var aFoodunitWt = new FoodUnitWeight { Food = aFood, Location = "NL", Value = 0.1D, ValueTypeString = UnitWeightValueType.UnitWeightEp.ToString() };
            aFood.FoodUnitWeights.Add(aFoodunitWt);
            var bFoodUnitWt = new FoodUnitWeight { Food = bFood, ValueTypeString = UnitWeightValueType.UnitWeightRac.ToString() };
            bFood.DefaultUnitWeightRac = bFoodUnitWt;

            var aCompound = new Compound("A");
            var anIndividual = new Individual(1) { Code = "A" };
            var anIndividualDay = new IndividualDay() { IdDay = "A", Individual = anIndividual };
            anIndividual.IndividualDays["A"] = anIndividualDay;

            var anEffect = new Effect { Code = "A", Name = "A" };
            var aResponse = new Response { Code = "A" };
            var aTestSystem = new TestSystem { Code = "A" };

            var anAnalyticalMethod = new AnalyticalMethod { Code = "A" };
            var anAnalyticalMethodCompound = new AnalyticalMethodCompound { Compound = aCompound, AnalyticalMethod = anAnalyticalMethod };
            anAnalyticalMethod.AnalyticalMethodCompounds.Add(aCompound, anAnalyticalMethodCompound);

            var aSample = new SampleAnalysis { Code = "A",  AnalyticalMethod = anAnalyticalMethod };
            aSample.Concentrations.Add(aCompound, new ConcentrationPerSample { Compound = aCompound, Sample = aSample });
            var aSampleProperty = new SampleProperty() { Name = "A" };
            var aSamplePropertyValue = new SamplePropertyValue() { SampleProperty = aSampleProperty, TextValue = "A" };
            var sampleProperty = new Dictionary<SampleProperty, SamplePropertyValue> {
                { aSampleProperty, aSamplePropertyValue }
            };

            var aFoodSample = new FoodSample() { 
                SampleAnalyses = new List<SampleAnalysis>() { aSample },
                SampleProperties = sampleProperty,
                Food = aFood,
            };

            var aHmSample = new HumanMonitoringSample { Code = "A", Individual = anIndividual };
            var aHmSampleAnalysis = new SampleAnalysis { AnalyticalMethod = anAnalyticalMethod, Code = "A" };
            aHmSampleAnalysis.Concentrations[aCompound] = new ConcentrationPerSample() { Compound = aCompound, Concentration = 0.1D, ResType = ResType.VAL, Sample = aHmSampleAnalysis };
            aHmSample.SampleAnalyses = new List<SampleAnalysis> { aHmSampleAnalysis };

            var aProcessingType = new ProcessingType { Code = "A" };
            var kineticModelParameters = new Dictionary<string, KineticModelInstanceParameter> {
                { "A", new KineticModelInstanceParameter { IdModelInstance = "A" } }
            };

            var aDietaryExposureModel = new DietaryExposureModel { Code = "A", Compound = aCompound };
            var aDietaryExposurePercentile = new DietaryExposurePercentile { Percentage = 1D };
            aDietaryExposurePercentile.ExposureUncertainties = new List<double> { 1D };
            aDietaryExposureModel.DietaryExposurePercentiles = new Dictionary<double, DietaryExposurePercentile> { { 1D, aDietaryExposurePercentile } };

            var aDoseResponseModel = new DoseResponseModel { IdExperiment = "A" };
            var aBenchmarkDose = new DoseResponseModelBenchmarkDose { IdDoseResponseModel = "A", Substance = aCompound };
            aBenchmarkDose.DoseResponseModelBenchmarkDoseUncertains = new List<DoseResponseModelBenchmarkDoseUncertain> {
                new DoseResponseModelBenchmarkDoseUncertain { Substance = aCompound }
            };
            aDoseResponseModel.DoseResponseModelBenchmarkDoses = new Dictionary<string, DoseResponseModelBenchmarkDose> { { "A", aBenchmarkDose } };

            var aDoseResponseExperimentMeasurement = new DoseResponseExperimentMeasurement { IdExperiment = "A", idResponse = "A" };
            var anExperimentalUnit = new ExperimentalUnit {
                Code = "A",
                DesignFactors = new Dictionary<string, string> { { "B", "B" } },
                Doses = new Dictionary<Compound, double> { { aCompound, 1D } },
                Responses = new Dictionary<Response, DoseResponseExperimentMeasurement> { { aResponse, aDoseResponseExperimentMeasurement } },
                Covariates = new Dictionary<string, string> { { "C", "C" } }
            };

            var aDoseResponseExperiment = new DoseResponseExperiment { Code = "A" };
            aDoseResponseExperiment.ExperimentalUnits = new List<ExperimentalUnit> { anExperimentalUnit };

            var anAopNetwork = new AdverseOutcomePathwayNetwork { Code = "A", AdverseOutcome = anEffect };
            var anEffectRelation = new EffectRelationship { AdverseOutcomePathwayNetwork = anAopNetwork, DownstreamKeyEvent = anEffect, UpstreamKeyEvent = new Effect { Code = "B" } };
            anAopNetwork.EffectRelations = new List<EffectRelationship> { anEffectRelation };

            var aNdSurvey = new NonDietarySurvey { Code = "A", NonDietarySurveyProperties = new List<NonDietarySurveyProperty> {
                new NonDietarySurveyProperty {IndividualProperty = new IndividualProperty() }
            } };
            var aNdExposureSet = new NonDietaryExposureSet { Code = "A", NonDietarySurvey = aNdSurvey };
            var aNdExposureSet2 = new NonDietaryExposureSet { NonDietarySurvey = aNdSurvey };
            var aNdExposure = new NonDietaryExposure { NonDietarySetCode = "A", Compound = aCompound };
            var aNdExposureUnc = new NonDietaryExposure { Compound = new Compound("B") };
            aNdExposureSet.NonDietaryExposures = new List<NonDietaryExposure> { aNdExposure, aNdExposureUnc };
            aNdExposureSet2.NonDietaryExposures = new List<NonDietaryExposure> { aNdExposure, aNdExposureUnc };

            var data = new CompiledData {
                AllAdverseOutcomePathwayNetworks = new Dictionary<string, AdverseOutcomePathwayNetwork> { { "A", anAopNetwork } },
                AllOccurrencePatterns = new List<OccurrencePattern> { new OccurrencePattern { Code = "A", Food = aFood, Compounds = new HashSet<Compound> { aCompound } } },
                AllAnalyticalMethods = new Dictionary<string, AnalyticalMethod> { { "A", anAnalyticalMethod } },
                AllActiveSubstanceModels = new Dictionary<string, ActiveSubstanceModel> {
                    { "A", new ActiveSubstanceModel {
                        Code = "A",
                        Effect = anEffect,
                        MembershipProbabilities = new Dictionary<Compound, double> { { aCompound, 0.1D } } }
                    }
                },
                AllSubstanceAuthorisations = new List<SubstanceAuthorisation> { new SubstanceAuthorisation { Reference = "A" } },
                AllSubstances = new Dictionary<string, Compound> { { "A", new Compound { Code = "A" } } },
                AllConcentrationDistributions = new List<ConcentrationDistribution> { new ConcentrationDistribution { Food = aFood, Compound = aCompound } },
                AllConcentrationSingleValues = new List<ConcentrationSingleValue> { new ConcentrationSingleValue { Food = aFood, Substance = aCompound } },
                AllDeterministicSubstanceConversionFactors = new List<DeterministicSubstanceConversionFactor>() { new DeterministicSubstanceConversionFactor { ActiveSubstance = aCompound } },
                AllDoseResponseExperiments = new Dictionary<string, DoseResponseExperiment> { { "A", aDoseResponseExperiment } },
                AllDoseResponseModels = new Dictionary<string, DoseResponseModel> { { "A", aDoseResponseModel } },
                AllDietaryExposureModels = new Dictionary<string, DietaryExposureModel> { { "A", aDietaryExposureModel } },
                AllDustAdherenceAmounts = new List<DustAdherenceAmount>(),
                AllDustBodyExposureFractions = new List<DustBodyExposureFraction>(),
                AllDustConcentrationDistributions = new List<DustConcentrationDistribution>(),
                AllDustIngestions = new List<DustIngestion>(),
                AllDustAvailabilityFractions = new List<DustAvailabilityFraction>(),
                AllEffectRepresentations = new List<EffectRepresentation> { new EffectRepresentation { Effect = anEffect, Response = aResponse } },
                AllEffects = new Dictionary<string, Effect> { { "A", new Effect { Code = "A" } } },
                AllFacetDescriptors = new Dictionary<string, FacetDescriptor> { { "A", new FacetDescriptor() { Code = "A" } } },
                AllFacets = new Dictionary<string, Facet> { { "A", new Facet { Code = "A" } } },
                AllFocalCommodityFoods = new Dictionary<string, Food> { { "A", aFood } },
                AllFocalFoodAnalyticalMethods = new Dictionary<string, AnalyticalMethod> { { "A", new AnalyticalMethod { Code = "A" } } },
                AllFocalFoodSamples = new Dictionary<string, FoodSample> { { "A", aFoodSample } },
                AllFoodConsumptions = new List<FoodConsumption> { new FoodConsumption { idMeal = "A", IndividualDay = anIndividualDay, Food = aFood } },
                AllFoodExtrapolations = new Dictionary<Food, ICollection<Food>> { { aFood, new List<Food> { bFood } } },
                AllFoods = new Dictionary<string, Food> { { "A", aFood }, { "B", bFood }, { "C", cFood } },
                AllFoodSurveys = new Dictionary<string, FoodSurvey> { { "A", new FoodSurvey { Code = "A" } } },
                AllFoodTranslations = new List<FoodTranslation> { aTranslation },
                AllPointsOfDeparture = new List<Compiled.Objects.PointOfDeparture> {
                    new Compiled.Objects.PointOfDeparture { Code = "A", Effect = anEffect, Compound = aCompound,
                        PointOfDepartureUncertains = new List<PointOfDepartureUncertain> {
                            new PointOfDepartureUncertain { Compound = aCompound, Effect = anEffect }
                        }
                    } },
                AllHazardCharacterisations = new List<HazardCharacterisation> { new HazardCharacterisation { Code = "A", Effect = anEffect, Substance = aCompound } },
                AllHumanMonitoringIndividuals = new Dictionary<string, Individual> { { "A", new Individual(0) { Code = "A" } } },
                AllHumanMonitoringAnalyticalMethods = new Dictionary<string, AnalyticalMethod> { { "A", new AnalyticalMethod { Code = "A" } } },
                AllHumanMonitoringSamples = new Dictionary<string, HumanMonitoringSample> { { "A", aHmSample } },
                AllHumanMonitoringSurveys = new Dictionary<string, HumanMonitoringSurvey> { { "A", new HumanMonitoringSurvey { Code = "A" } } },
                AllIestiSpecialCases = new List<IestiSpecialCase> { new IestiSpecialCase { Food = aFood, Substance = aCompound } },
                AllDietaryIndividualProperties = new Dictionary<string, IndividualProperty> { { "A", new IndividualProperty { Name = "A" } } },
                AllIndividuals = new Dictionary<string, Individual> { { "A", anIndividual } },
                AllInterSpeciesFactors = new List<InterSpeciesFactor> { new InterSpeciesFactor { Species = "A" } },
                AllIntraSpeciesFactors = new List<IntraSpeciesFactor> { new IntraSpeciesFactor { IdPopulation = "A" } },
                AllAbsorptionFactors = new List<SimpleAbsorptionFactor> { new SimpleAbsorptionFactor { } },
                AllKineticModelInstances = new List<KineticModelInstance> { new KineticModelInstance { IdModelInstance = "A", KineticModelInstanceParameters = kineticModelParameters } },
                AllMarketShares = new List<MarketShare> { new MarketShare { Food = new Food { Code = "A" } } },
                AllMaximumConcentrationLimits = new List<ConcentrationLimit> { new ConcentrationLimit { Food = aFood, Compound = aCompound } },
                AllMolecularDockingModels = new Dictionary<string, MolecularDockingModel> {
                    { "A", new MolecularDockingModel {
                        Code = "A",
                        BindingEnergies = new Dictionary<Compound, double>() { { aCompound, 1D } }
                    } } },
                AllPopulations = new Dictionary<string, Population> { { "A", new Population { Code = "A" } } },
                AllPopulationConsumptionSingleValues = new List<PopulationConsumptionSingleValue> { new PopulationConsumptionSingleValue { Population = new Population { Code = "A" }, Food = aFood } },
                AllOccurrenceFrequencies = new List<OccurrenceFrequency> { new OccurrenceFrequency { Food = aFood, Substance = aCompound } },
                AllProcessingFactors = new List<ProcessingFactor> { new ProcessingFactor { FoodProcessed = aFood, FoodUnprocessed = bFood, ProcessingType = aProcessingType } },
                AllProcessingTypes = new Dictionary<string, ProcessingType> { { "A", aProcessingType } },
                AllQsarMembershipModels = new Dictionary<string, QsarMembershipModel> {
                    { "A", new QsarMembershipModel {
                        Code = "A",
                        MembershipScores = new Dictionary<Compound, double>() { { aCompound, 1D } }
                    } } },
                AllRelativePotencyFactors = new Dictionary<string, List<RelativePotencyFactor>> {
                    { "A", new List<RelativePotencyFactor> {
                        new RelativePotencyFactor {
                            Compound = aCompound, Effect = anEffect,
                            RelativePotencyFactorsUncertains = new List<RelativePotencyFactorUncertain> {
                                new RelativePotencyFactorUncertain { idUncertaintySet = "A", RPF = 1D }
                            }
                    } } } },
                AllSubstanceConversions = new List<SubstanceConversion> { new SubstanceConversion { ActiveSubstance = aCompound } },
                AllResponses = new Dictionary<string, Response> { { "A", new Response { Code = "A", TestSystem = aTestSystem } } },
                AllRiskModels = new Dictionary<string, RiskModel>() {
                    { "A", new RiskModel() {
                            RiskPercentiles = new Dictionary<double, RiskPercentile>() {
                                {
                                    95,
                                    new RiskPercentile() { RiskUncertainties = new List<double>() { 1D } }
                                }
                            }
                        }
                    }
                },
                AllAdditionalSampleProperties = new Dictionary<string, SampleProperty> { { "A", aSampleProperty } },
                AllSampleLocations = new List<string> { "A" },
                AllSampleRegions = new List<string> { "A" },
                AllSampleProductionMethods = new List<string> { "A" },
                AllFoodSamples = new Dictionary<string, FoodSample> { { "A", aFoodSample } },
                AllSampleYears = new List<int> { 1 },
                AllTargetExposureModels = new Dictionary<string, TargetExposureModel>() {
                    { "A", new TargetExposureModel() {
                        TargetExposurePercentiles = new Dictionary<double, TargetExposurePercentile>() {
                                {
                                    95,
                                    new TargetExposurePercentile() { ExposureUncertainties = new List<double>() { 1D } }
                                }
                            }
                        }
                    }
                },
                AllTDSFoodSampleCompositions = new List<TDSFoodSampleComposition> { new TDSFoodSampleComposition { Food = aFood, TDSFood = bFood } },
                AllTestSystems = new Dictionary<string, TestSystem> { { "A", aTestSystem } },
                AllUnitVariabilityFactors = new List<UnitVariabilityFactor> { new UnitVariabilityFactor { Food = aFood, Compound = aCompound, ProcessingType = aProcessingType } },
                DefaultProcessingFactors = new List<ProcessingFactor> { new ProcessingFactor { Compound = aCompound, FoodProcessed = aFood, FoodUnprocessed = bFood } },
                HumanMonitoringSamplingMethods = new List<HumanMonitoringSamplingMethod> { new HumanMonitoringSamplingMethod { BiologicalMatrix = BiologicalMatrix.Blood } },
                NonDietaryExposureSets = new List<NonDietaryExposureSet> { aNdExposureSet, aNdExposureSet2 },
                Scope = new Dictionary<SourceTableGroup, HashSet<string>>()
            };

            //Assert all members are not null now
            //use reflection and enumerate all members
            foreach (var prop in data.GetType().GetProperties()) {
                Assert.IsNotNull(prop.GetValue(data));
            }

            return data;
        }

        private static CompiledData createBasicCompiledDataInstance() {
            var data = new CompiledData {
                AllAdverseOutcomePathwayNetworks = new Dictionary<string, AdverseOutcomePathwayNetwork>(),
                AllAdditionalSampleProperties = new Dictionary<string, SampleProperty>(),
                AllAnalyticalMethods = new Dictionary<string, AnalyticalMethod>(),
                AllActiveSubstanceModels = new Dictionary<string, ActiveSubstanceModel>(),
                AllConcentrationDistributions = new List<ConcentrationDistribution>(),
                AllConcentrationSingleValues = new List<ConcentrationSingleValue>(),
                AllDeterministicSubstanceConversionFactors = new List<DeterministicSubstanceConversionFactor>(),
                AllDietaryIndividualProperties = new Dictionary<string, IndividualProperty>(),
                AllDietaryExposureModels = new Dictionary<string, DietaryExposureModel>(),
                AllDoseResponseExperiments = new Dictionary<string, DoseResponseExperiment>(),
                AllDustAdherenceAmounts = new List<DustAdherenceAmount>(),
                AllDustAvailabilityFractions = new List<DustAvailabilityFraction>(),
                AllDoseResponseModels = new Dictionary<string, DoseResponseModel>(),
                AllDustBodyExposureFractions = new List<DustBodyExposureFraction>(),
                AllDustConcentrationDistributions = new List<DustConcentrationDistribution>(),
                AllDustIngestions = new List<DustIngestion>(),
                AllEffectRepresentations = new List<EffectRepresentation>(),
                AllEffects = new Dictionary<string, Effect>(),
                AllFacetDescriptors = new Dictionary<string, FacetDescriptor>(),
                AllFacets = new Dictionary<string, Facet>(),
                AllFocalCommodityFoods = new Dictionary<string, Food>(),
                AllFocalFoodSamples = new Dictionary<string, FoodSample>(),
                AllFocalFoodAnalyticalMethods = new Dictionary<string, AnalyticalMethod>(),
                AllFoodConsumptions = new List<FoodConsumption>(),
                AllFoodExtrapolations = new Dictionary<Food, ICollection<Food>>(),
                AllFoodFacets = new Dictionary<string, FoodFacet>(),
                AllFoods = new Dictionary<string, Food>(),
                AllFoodSurveys = new Dictionary<string, FoodSurvey>(),
                AllFoodTranslations = new List<FoodTranslation>(),
                AllFoodSamples = new Dictionary<string, FoodSample>(),
                AllHazardCharacterisations = new List<HazardCharacterisation>(),
                AllHumanMonitoringIndividuals = new Dictionary<string, Individual>(),
                AllHumanMonitoringAnalyticalMethods = new Dictionary<string, AnalyticalMethod>(),
                AllHumanMonitoringSamples = new Dictionary<string, HumanMonitoringSample>(),
                AllHumanMonitoringSurveys = new Dictionary<string, HumanMonitoringSurvey>(),
                AllIestiSpecialCases = new List<IestiSpecialCase>(),
                AllIndividuals = new Dictionary<string, Individual>(),
                AllInterSpeciesFactors = new List<InterSpeciesFactor>(),
                AllIntraSpeciesFactors = new List<IntraSpeciesFactor>(),
                AllAbsorptionFactors = new List<SimpleAbsorptionFactor>(),
                AllKineticConversionFactors = new List<KineticConversionFactor>(),
                AllKineticModelInstances = new List<KineticModelInstance>(),
                AllMarketShares = new List<MarketShare>(),
                AllMaximumConcentrationLimits = new List<ConcentrationLimit>(),
                AllMolecularDockingModels = new Dictionary<string, MolecularDockingModel>(),
                AllNonDietaryExposureSources = new Dictionary<string, NonDietaryExposureSource>(),
                AllOccurrenceFrequencies = new List<OccurrenceFrequency>(),
                AllOccurrencePatterns = new List<OccurrencePattern>(),
                AllPopulations = new Dictionary<string, Population>(),
                AllPointsOfDeparture = new List<Compiled.Objects.PointOfDeparture>(),
                AllPopulationIndividualProperties = new Dictionary<string, IndividualProperty>(),
                AllPopulationConsumptionSingleValues = new List<PopulationConsumptionSingleValue>(),
                AllProcessingFactors = new List<ProcessingFactor>(),
                AllProcessingTypes = new Dictionary<string, ProcessingType>(),
                AllQsarMembershipModels = new Dictionary<string, QsarMembershipModel>(),
                AllRelativePotencyFactors = new Dictionary<string, List<RelativePotencyFactor>>(),
                AllResponses = new Dictionary<string, Response>(),
                AllRiskModels = new Dictionary<string, RiskModel>(),
                AllSampleLocations = new List<string>(),
                AllSampleProductionMethods = new List<string>(),
                AllSampleRegions = new List<string>(),
                AllSampleYears = new List<int>(),
                AllSubstanceConversions = new List<SubstanceConversion>(),
                AllSubstances = new Dictionary<string, Compound>(),
                AllSubstanceAuthorisations = new List<SubstanceAuthorisation>(),
                AllSubstanceApprovals = new List<SubstanceApproval>(),                
                AllTargetExposureModels = new Dictionary<string, TargetExposureModel>(),
                AllTDSFoodSampleCompositions = new List<TDSFoodSampleComposition>(),
                AllTestSystems = new Dictionary<string, TestSystem>(),
                AllUnitVariabilityFactors = new List<UnitVariabilityFactor>(),
                DefaultProcessingFactors = new List<ProcessingFactor>(),
                AllHumanMonitoringIndividualProperties = new Dictionary<string, IndividualProperty>(),
                HumanMonitoringSamplingMethods = new List<HumanMonitoringSamplingMethod>(),
                NonDietaryExposureSets = new List<NonDietaryExposureSet>(),
                AllExposureBiomarkerConversions = new List<ExposureBiomarkerConversion>(),
                AllSingleValueNonDietaryExposureScenarios = new Dictionary<string, ExposureScenario>(),
                AllSingleValueNonDietaryExposureDeterminantCombinations = new Dictionary<string, ExposureDeterminantCombination>(),
                AllSingleValueNonDietaryExposureEstimates = new List<ExposureEstimate>(),
                Scope = new Dictionary<SourceTableGroup, HashSet<string>>()
            };

            //Assert all members are not null now
            //use reflection and enumerate all members
            foreach (var prop in data.GetType().GetProperties()) {
                Assert.IsNotNull(prop.GetValue(data));
            }

            return data;
        }
    }
}
