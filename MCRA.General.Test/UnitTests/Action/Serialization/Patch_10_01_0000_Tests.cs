﻿using System.Globalization;
using MCRA.General.Action.Serialization;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.Action.Serialization {
    [TestClass]
    public class Patch_10_01_0000_Tests : ProjectSettingsSerializerTestsBase {

        private const string XmlResourceFolder = "Patch_10_01_0000";

        //Test: Action
        [TestMethod]
        public void Patch_10_01_0000_ActionModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "ActionModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<ActionModuleConfig>();

            Assert.AreEqual(11223344, modSettings.RandomSeed);
            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.IsTrue(modSettings.Cumulative);
            Assert.IsTrue(modSettings.DoUncertaintyAnalysis);
            Assert.IsTrue(modSettings.DoUncertaintyFactorial);
            Assert.AreEqual(4.5678D, modSettings.UncertaintyUpperBound);
            Assert.AreEqual(3.4567D, modSettings.UncertaintyLowerBound);
            Assert.AreEqual(88.884D, modSettings.UpperPercentage);
            Assert.AreEqual(1.2345D, modSettings.LowerPercentage);
            Assert.AreEqual(75.544D, modSettings.PercentageForUpperTail);
            Assert.AreEqual(49999, modSettings.NumberOfResampleCycles);
            Assert.AreEqual(559999, modSettings.NumberOfIterationsPerResampledSet);
            Assert.AreEqual("A B C D", string.Join(' ', modSettings.OutputSections));
            Assert.AreEqual(OutputSectionSelectionMethod.OptOut, modSettings.OutputSectionSelectionMethod);
            Assert.IsTrue(modSettings.SkipPrivacySensitiveOutputs);
        }


        //Test: ActiveSubstances
        [TestMethod]
        public void Patch_10_01_0000_ActiveSubstancesModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "ActiveSubstancesModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<ActiveSubstancesModuleConfig>();

            Assert.IsTrue(modSettings.MultipleEffects);
            Assert.IsTrue(modSettings.IncludeAopNetwork);
            Assert.IsTrue(modSettings.FilterByCertainAssessmentGroupMembership);
            Assert.IsTrue(modSettings.FilterByAvailableHazardDose);
            Assert.IsTrue(modSettings.FilterByAvailableHazardCharacterisation);
            Assert.IsTrue(modSettings.UseQsarModels);
            Assert.IsTrue(modSettings.UseMolecularDockingModels);
            Assert.IsTrue(modSettings.IncludeSubstancesWithUnknowMemberships);
            Assert.AreEqual(1, (int)modSettings.CombinationMethodMembershipInfoAndPodPresence);
            Assert.AreEqual(1, (int)modSettings.AssessmentGroupMembershipCalculationMethod);
            Assert.AreEqual(1.2345D, modSettings.PriorMembershipProbability);
            Assert.IsTrue(modSettings.UseProbabilisticMemberships);
            Assert.IsTrue(modSettings.ReSampleAssessmentGroupMemberships);
        }


        //Test: AOPNetworks
        [TestMethod]
        public void Patch_10_01_0000_AOPNetworksModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "AOPNetworksModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<AOPNetworksModuleConfig>();

            Assert.AreEqual("ADBADF", modSettings.CodeAopNetwork);
            Assert.IsTrue(modSettings.RestrictAopByFocalUpstreamEffect);
            Assert.AreEqual("DIDOISJF", modSettings.CodeFocalUpstreamEffect);
        }


        //Test: BiologicalMatrixConcentrationComparisons
        [TestMethod]
        public void Patch_10_01_0000_BiologicalMatrixConcentrationComparisonsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "BiologicalMatrixConcentrationComparisonsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<BiologicalMatrixConcentrationComparisonsModuleConfig>();

            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.IsTrue(modSettings.CorrelateTargetConcentrations);
            Assert.AreEqual(3.3333D, modSettings.LowerPercentage);
            Assert.AreEqual(88.888D, modSettings.UpperPercentage);
            Assert.IsTrue(modSettings.StoreIndividualDayIntakes);
        }


        //Test: ConcentrationModels
        [TestMethod]
        public void Patch_10_01_0000_ConcentrationModelsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "ConcentrationModelsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<ConcentrationModelsModuleConfig>();

            Assert.AreEqual(11223344, modSettings.RandomSeed);
            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.AreEqual(SettingsTemplateType.ComTier1, modSettings.ConcentrationModelChoice);
            Assert.AreEqual(ConcentrationModelType.NonDetectSpikeLogNormal, modSettings.DefaultConcentrationModel);
            Assert.IsTrue(modSettings.IsFallbackMrl);
            Assert.IsTrue(modSettings.RestrictLorImputationToAuthorisedUses);
            Assert.AreEqual(NonDetectsHandlingMethod.ReplaceByLOR, modSettings.NonDetectsHandlingMethod);
            Assert.AreEqual(1.2345D, modSettings.FractionOfLOR);
            Assert.AreEqual(2.3456D, modSettings.FractionOfMrl);
            Assert.IsTrue(modSettings.IsSampleBased);
            Assert.IsTrue(modSettings.ImputeMissingValues);
            Assert.IsTrue(modSettings.CorrelateImputedValueWithSamplePotency);
            Assert.IsTrue(modSettings.UseAgriculturalUseTable);
            Assert.IsTrue(modSettings.TotalDietStudy);
            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.Cumulative);
            Assert.IsTrue(modSettings.IsParametric);
            Assert.IsTrue(modSettings.ReSampleConcentrations);

            Assert.AreEqual(2, modSettings.ConcentrationModelTypesFoodSubstance.Count);
            Assert.AreEqual("Aa", modSettings.ConcentrationModelTypesFoodSubstance[0].FoodCode);
            Assert.AreEqual("Bb", modSettings.ConcentrationModelTypesFoodSubstance[0].SubstanceCode);
            Assert.AreEqual(ConcentrationModelType.SummaryStatistics, modSettings.ConcentrationModelTypesFoodSubstance[0].ModelType);
            Assert.AreEqual("Cc", modSettings.ConcentrationModelTypesFoodSubstance[1].FoodCode);
            Assert.AreEqual("Dd", modSettings.ConcentrationModelTypesFoodSubstance[1].SubstanceCode);
            Assert.AreEqual(ConcentrationModelType.ZeroSpikeCensoredLogNormal, modSettings.ConcentrationModelTypesFoodSubstance[1].ModelType);
        }


        //Test: Concentrations
        [TestMethod]
        public void Patch_10_01_0000_ConcentrationsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "ConcentrationsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<ConcentrationsModuleConfig>();

            Assert.AreEqual(11223344, modSettings.RandomSeed);
            Assert.AreEqual(SettingsTemplateType.EfsaOptimistic, modSettings.ConcentrationsTier);
            Assert.IsTrue(modSettings.FilterConcentrationLimitExceedingSamples);
            Assert.AreEqual(0.1234D, modSettings.ConcentrationLimitFilterFractionExceedanceThreshold);
            Assert.IsTrue(modSettings.UseComplexResidueDefinitions);
            Assert.AreEqual(SubstanceTranslationAllocationMethod.UseMostToxic, modSettings.SubstanceTranslationAllocationMethod);
            Assert.IsTrue(modSettings.RetainAllAllocatedSubstancesAfterAllocation);
            Assert.IsTrue(modSettings.ConsiderAuthorisationsForSubstanceConversion);
            Assert.IsTrue(modSettings.TryFixDuplicateAllocationInconsistencies);
            Assert.IsTrue(modSettings.ExtrapolateConcentrations);
            Assert.AreEqual(456, modSettings.ThresholdForExtrapolation);
            Assert.IsTrue(modSettings.ConsiderMrlForExtrapolations);
            Assert.IsTrue(modSettings.ConsiderAuthorisationsForExtrapolations);
            Assert.IsTrue(modSettings.ImputeWaterConcentrations);
            Assert.AreEqual("WOAH", modSettings.CodeWater);
            Assert.AreEqual(2.3456D, modSettings.WaterConcentrationValue);
            Assert.IsTrue(modSettings.RestrictWaterImputationToMostPotentSubstances);
            Assert.IsTrue(modSettings.RestrictWaterImputationToAuthorisedUses);
            Assert.IsTrue(modSettings.RestrictWaterImputationToApprovedSubstances);
            Assert.IsTrue(modSettings.FocalCommodity);
            Assert.AreEqual(FocalCommodityReplacementMethod.MeasurementRemoval, modSettings.FocalCommodityReplacementMethod);
            Assert.AreEqual(99.1234D, modSettings.FocalCommodityScenarioOccurrencePercentage);
            Assert.AreEqual(12.345D, modSettings.FocalCommodityConcentrationAdjustmentFactor);
            Assert.IsTrue(modSettings.UseDeterministicSubstanceConversionsForFocalCommodity);
            Assert.IsTrue(modSettings.SampleSubsetSelection);
            Assert.IsTrue(modSettings.RestrictToModelledFoodSubset);
            Assert.IsTrue(modSettings.AlignSampleLocationSubsetWithPopulation);
            Assert.IsTrue(modSettings.IncludeMissingLocationRecords);
            Assert.IsTrue(modSettings.AlignSampleDateSubsetWithPopulation);
            Assert.IsTrue(modSettings.AlignSampleSeasonSubsetWithPopulation);
            Assert.IsTrue(modSettings.IncludeMissingDateValueRecords);
            Assert.IsTrue(modSettings.FilterSamplesByLocation);
            Assert.IsTrue(modSettings.FilterSamplesByYear);
            Assert.IsTrue(modSettings.FilterSamplesByMonth);
            Assert.IsTrue(modSettings.ReSampleConcentrations);
            Assert.AreEqual(3.3333D, modSettings.LowerPercentage);
            Assert.AreEqual(88.888D, modSettings.UpperPercentage);

            Assert.AreEqual(2, modSettings.FocalFoods.Count);
            Assert.AreEqual("Aa", modSettings.FocalFoods[0].CodeFood);
            Assert.AreEqual("Bb", modSettings.FocalFoods[0].CodeSubstance);
            Assert.AreEqual("Cc", modSettings.FocalFoods[1].CodeFood);
            Assert.AreEqual("Dd", modSettings.FocalFoods[1].CodeSubstance);

            Assert.AreEqual(3, modSettings.SamplesSubsetDefinitions.Count);
            var subsetDef = modSettings.SamplesSubsetDefinitions[0];
            Assert.AreEqual("Aa,B b,C  c", string.Join(",", subsetDef.KeyWords));
            Assert.IsTrue(subsetDef.IsProductionMethodSubset);
            Assert.IsFalse(subsetDef.IsRegionSubset);
            Assert.IsTrue(subsetDef.AlignSubsetWithPopulation);
            Assert.IsTrue(subsetDef.IncludeMissingValueRecords);
            Assert.AreEqual("ProductionMethod", subsetDef.PropertyName);
            subsetDef = modSettings.SamplesSubsetDefinitions[1];
            Assert.AreEqual("Dd,E e,F  f", string.Join(",", subsetDef.KeyWords));
            Assert.IsFalse(subsetDef.IsProductionMethodSubset);
            Assert.IsTrue(subsetDef.IsRegionSubset);
            Assert.IsTrue(subsetDef.AlignSubsetWithPopulation);
            Assert.IsTrue(subsetDef.IncludeMissingValueRecords);
            Assert.AreEqual("Region", subsetDef.PropertyName);
            subsetDef = modSettings.SamplesSubsetDefinitions[2];
            Assert.AreEqual("Gg,H h,I  j", string.Join(",", subsetDef.KeyWords));
            Assert.IsFalse(subsetDef.IsProductionMethodSubset);
            Assert.IsFalse(subsetDef.IsRegionSubset);
            Assert.IsTrue(subsetDef.AlignSubsetWithPopulation);
            Assert.IsTrue(subsetDef.IncludeMissingValueRecords);
            Assert.AreEqual("Custom something", subsetDef.PropertyName);

            Assert.IsTrue(modSettings.LocationSubsetDefinition.IncludeMissingValueRecords);
            Assert.IsTrue(modSettings.LocationSubsetDefinition.AlignSubsetWithPopulation);
            Assert.AreEqual("NL,DE", string.Join(",", modSettings.LocationSubsetDefinition.LocationSubset));

            Assert.IsTrue(modSettings.PeriodSubsetDefinition.AlignSampleDateSubsetWithPopulation);
            Assert.IsTrue(modSettings.PeriodSubsetDefinition.AlignSampleSeasonSubsetWithPopulation);
            Assert.IsTrue(modSettings.PeriodSubsetDefinition.IncludeMissingValueRecords);
            Assert.AreEqual("2,3", string.Join(",", modSettings.PeriodSubsetDefinition.MonthsSubset));
            Assert.AreEqual("2020,2023", string.Join(",", modSettings.PeriodSubsetDefinition.YearsSubset));
        }


        //Test: Consumptions
        [TestMethod]
        public void Patch_10_01_0000_ConsumptionsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "ConsumptionsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<ConsumptionsModuleConfig>();

            Assert.AreEqual(SettingsTemplateType.Efsa2022DietaryCraAcuteTier1, modSettings.ConsumptionsTier);
            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.AreEqual("ADBADF", modSettings.CodeFoodSurvey);
            Assert.IsTrue(modSettings.ConsumerDaysOnly);
            Assert.IsTrue(modSettings.RestrictPopulationByFoodAsEatenSubset);
            Assert.AreEqual("E F G H", string.Join(' ', modSettings.FocalFoodAsEatenSubset));
            Assert.IsTrue(modSettings.RestrictConsumptionsByFoodAsEatenSubset);
            Assert.AreEqual("A B C D", string.Join(' ', modSettings.FoodAsEatenSubset));
            Assert.AreEqual(IndividualSubsetType.IgnorePopulationDefinition, modSettings.MatchIndividualSubsetWithPopulation);
            Assert.AreEqual("J K L M", string.Join(' ', modSettings.SelectedFoodSurveySubsetProperties));
            Assert.IsTrue(modSettings.IsDefaultSamplingWeight);
            Assert.IsTrue(modSettings.ExcludeIndividualsWithLessThanNDays);
            Assert.AreEqual(9999, modSettings.MinimumNumberOfDays);
            Assert.AreEqual("DEF", modSettings.NameCofactor);
            Assert.AreEqual("Abc", modSettings.NameCovariable);
            Assert.IsTrue(modSettings.PopulationSubsetSelection);
            Assert.IsTrue(modSettings.ResampleIndividuals);
            Assert.IsTrue(modSettings.ReSamplePortions);
            Assert.AreEqual(3.3333D, modSettings.LowerPercentage);
            Assert.AreEqual(88.888D, modSettings.UpperPercentage);

            Assert.AreEqual("Idsds", modSettings.IndividualDaySubsetDefinition.NameIndividualProperty);
            Assert.IsTrue(modSettings.IndividualDaySubsetDefinition.IncludeMissingValueRecords);
            Assert.AreEqual("1 4 12", string.Join(" ", modSettings.IndividualDaySubsetDefinition.MonthsSubset));

            var idv = modSettings.IndividualsSubsetDefinitions[0];
            Assert.AreEqual("DoubleRange", idv.NameIndividualProperty);
            Assert.AreEqual("61-93", idv.IndividualPropertyQuery);
            Assert.AreEqual(QueryDefinitionType.Range, idv.GetQueryDefinitionType());
            Assert.AreEqual(61, idv.GetRangeMin());
            Assert.AreEqual(93, idv.GetRangeMax());
            idv = modSettings.IndividualsSubsetDefinitions[1];
            Assert.AreEqual("Keywords", idv.NameIndividualProperty);
            Assert.AreEqual("'a b','sf','99'", idv.IndividualPropertyQuery);
            Assert.AreEqual(QueryDefinitionType.ValueList, idv.GetQueryDefinitionType());
            Assert.AreEqual("a b|sf|99", string.Join('|', idv.GetQueryKeywords()));
            idv = modSettings.IndividualsSubsetDefinitions[2];
            Assert.AreEqual("Empty", idv.NameIndividualProperty);
            Assert.AreEqual("-", idv.IndividualPropertyQuery);
            Assert.AreEqual(QueryDefinitionType.Empty, idv.GetQueryDefinitionType());
        }


        //Test: ConsumptionsByModelledFood
        [TestMethod]
        public void Patch_10_01_0000_ConsumptionsByModelledFoodModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "ConsumptionsByModelledFoodModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<ConsumptionsByModelledFoodModuleConfig>();

            Assert.IsTrue(modSettings.ModelledFoodsConsumerDaysOnly);
            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.IsTrue(modSettings.RestrictPopulationByModelledFoodSubset);
            Assert.AreEqual("A B C D", string.Join(' ', modSettings.FocalFoodAsMeasuredSubset));
            Assert.IsTrue(modSettings.IsProcessing);
            Assert.AreEqual(3.3333D, modSettings.LowerPercentage);
            Assert.AreEqual(88.888D, modSettings.UpperPercentage);
        }


        //Test: DietaryExposures
        [TestMethod]
        public void Patch_10_01_0000_DietaryExposuresModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "DietaryExposuresModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<DietaryExposuresModuleConfig>();

            Assert.AreEqual("SsAf-1,SSAF002,Ss A f 3", string.Join(',', modSettings.ScenarioAnalysisFoods));
            Assert.AreEqual(SettingsTemplateType.Ec2018DietaryCraChronicTier1, modSettings.DietaryIntakeCalculationTier);
            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.IsTrue(modSettings.TotalDietStudy);
            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.Cumulative);
            Assert.IsTrue(modSettings.IsSampleBased);
            Assert.IsTrue(modSettings.IsSingleSamplePerDay);
            Assert.IsTrue(modSettings.IsCorrelation);
            Assert.IsTrue(modSettings.IsProcessing);
            Assert.IsTrue(modSettings.IsDistribution);
            Assert.IsTrue(modSettings.AllowHigherThanOne);
            Assert.IsTrue(modSettings.UseUnitVariability);
            Assert.AreEqual(UnitVariabilityModelType.BernoulliDistribution, modSettings.UnitVariabilityModel);
            Assert.AreEqual(EstimatesNature.Conservative, modSettings.EstimatesNature);
            Assert.AreEqual(UnitVariabilityType.VariabilityFactor, modSettings.UnitVariabilityType);
            Assert.AreEqual(MeanValueCorrectionType.Biased, modSettings.MeanValueCorrectionType);
            Assert.AreEqual(123, modSettings.DefaultFactorLow);
            Assert.AreEqual(UnitVariabilityCorrelationType.FullCorrelation, modSettings.CorrelationType);
            Assert.AreEqual(456, modSettings.DefaultFactorMid);
            Assert.AreEqual(IntakeModelType.BBN, modSettings.IntakeModelType);
            Assert.IsTrue(modSettings.FirstModelThenAdd);
            Assert.IsTrue(modSettings.CovariateModelling);
            Assert.AreEqual(TransformType.Power, modSettings.TransformType);
            Assert.AreEqual(20, modSettings.GridPrecision);
            Assert.AreEqual(5, modSettings.NumberOfIterations);
            Assert.IsTrue(modSettings.SplineFit);
            Assert.AreEqual(0.023847, modSettings.Dispersion);
            Assert.AreEqual(43, modSettings.VarianceRatio);
            //intake models
            Assert.AreEqual(3, modSettings.IntakeModelsPerCategory.Count);
            var itm = modSettings.IntakeModelsPerCategory[0];
            Assert.AreEqual(IntakeModelType.LNN0, itm.ModelType);
            Assert.AreEqual(TransformType.NoTransform, itm.TransformType);
            Assert.AreEqual("10,26", string.Join(",", itm.FoodsAsMeasured));
            itm = modSettings.IntakeModelsPerCategory[1];
            Assert.AreEqual(IntakeModelType.ISUF, itm.ModelType);
            Assert.AreEqual(TransformType.Power, itm.TransformType);
            Assert.AreEqual("11,18", string.Join(",", itm.FoodsAsMeasured));
            itm = modSettings.IntakeModelsPerCategory[2];
            Assert.AreEqual(IntakeModelType.BBN, itm.ModelType);
            Assert.AreEqual(TransformType.Power, itm.TransformType);
            Assert.AreEqual("1,3", string.Join(",", itm.FoodsAsMeasured));

            Assert.AreEqual(CovariateModelType.Cofactor, modSettings.CovariateModelType);
            Assert.AreEqual(FunctionType.Spline, modSettings.FunctionType);
            Assert.AreEqual(2.3456D, modSettings.TestingLevel);
            Assert.AreEqual(TestingMethodType.Forward, modSettings.TestingMethod);
            Assert.AreEqual(34567, modSettings.MaxDegreesOfFreedom);
            Assert.AreEqual(45678, modSettings.MinDegreesOfFreedom);
            Assert.AreEqual(CovariateModelType.Covariable, modSettings.FrequencyModelCovariateModelType);
            Assert.AreEqual(FunctionType.Spline, modSettings.FrequencyModelFunctionType);
            Assert.AreEqual(23.456D, modSettings.FrequencyModelTestingLevel);
            Assert.AreEqual(TestingMethodType.Forward, modSettings.FrequencyModelTestingMethod);
            Assert.AreEqual(6789, modSettings.FrequencyModelMinDegreesOfFreedom);
            Assert.AreEqual(5678, modSettings.FrequencyModelMaxDegreesOfFreedom);
            Assert.IsTrue(modSettings.UseOccurrencePatternsForResidueGeneration);
            Assert.IsTrue(modSettings.SetMissingAgriculturalUseAsUnauthorized);
            Assert.AreEqual(DietaryExposuresDetailsLevel.Full, modSettings.DietaryExposuresDetailsLevel);
            Assert.IsTrue(modSettings.IsSurveySampling);
            Assert.AreEqual(9999, modSettings.NumberOfMonteCarloIterations);
            Assert.IsTrue(modSettings.ImputeExposureDistributions);
            Assert.IsTrue(modSettings.VariabilityDiagnosticsAnalysis);
            Assert.AreEqual("Co Factor", modSettings.NameCofactor);
            Assert.AreEqual("CoVAR", modSettings.NameCovariable);
            Assert.IsTrue(modSettings.UseReadAcrossFoodTranslations);
            Assert.AreEqual(NonDetectsHandlingMethod.ReplaceByLODLOQSystem, modSettings.NonDetectsHandlingMethod);
            Assert.AreEqual(ConcentrationModelType.NonDetectSpikeLogNormal, modSettings.DefaultConcentrationModel);
            Assert.IsTrue(modSettings.AnalyseMcr);
            Assert.AreEqual(ExposureApproachType.UnweightedExposures, modSettings.ExposureApproachType);
            Assert.IsTrue(modSettings.UseScenario);
            Assert.AreEqual(98.765D, modSettings.MixtureSelectionRatioCutOff);
            Assert.AreEqual(87.654D, modSettings.MixtureSelectionTotalExposureCutOff);
            Assert.AreEqual(55.555D, modSettings.MaximumCumulativeRatioCutOff);
            Assert.AreEqual("95 99 99.55", string.Join(' ', modSettings.MaximumCumulativeRatioPercentiles.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(44.444D, modSettings.MaximumCumulativeRatioMinimumPercentage);
            Assert.AreEqual(TargetLevelType.Internal, modSettings.TargetDoseLevelType);
            Assert.AreEqual(987654, modSettings.RandomSeed);
            Assert.IsTrue(modSettings.ReSampleImputationExposureDistributions);
            Assert.IsTrue(modSettings.ReSamplePortions);
            Assert.IsTrue(modSettings.DoUncertaintyAnalysis);
            Assert.AreEqual(49999, modSettings.NumberOfResampleCycles);
            Assert.AreEqual(555, modSettings.NumberOfIterationsPerResampledSet);
            Assert.AreEqual(3.4567D, modSettings.UncertaintyLowerBound);
            Assert.AreEqual(4.5678D, modSettings.UncertaintyUpperBound);
            Assert.IsTrue(modSettings.SkipPrivacySensitiveOutputs);
            Assert.IsTrue(modSettings.IsDetailedOutput);
            Assert.AreEqual("40 44 45.66 88.88", string.Join(' ', modSettings.SelectedPercentiles.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(98.765D, modSettings.PercentageForDrilldown);
            Assert.AreEqual(87.654D, modSettings.PercentageForUpperTail);
            Assert.AreEqual(ExposureMethod.Automatic, modSettings.ExposureMethod);
            Assert.AreEqual("1 10.07 50", string.Join(' ', modSettings.ExposureLevels.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(444.444D, modSettings.Intervals);
            Assert.AreEqual("11.11 22.22 33.33", string.Join(' ', modSettings.ExtraPredictionLevels.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(25.25D, modSettings.LowerPercentage);
            Assert.AreEqual(75.75D, modSettings.UpperPercentage);
            Assert.IsTrue(modSettings.IsPerPerson);
        }


        //Test: DoseResponseData
        [TestMethod]
        public void Patch_10_01_0000_DoseResponseDataModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "DoseResponseDataModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<DoseResponseDataModuleConfig>();

            Assert.IsTrue(modSettings.MergeDoseResponseExperimentsData);
        }


        //Test: DoseResponseModels
        [TestMethod]
        public void Patch_10_01_0000_DoseResponseModelsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "DoseResponseModelsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<DoseResponseModelsModuleConfig>();

            Assert.AreEqual("RF-00000011-VET", modSettings.CodeReferenceSubstance);
            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.CalculateParametricConfidenceInterval);
            Assert.IsTrue(modSettings.DoUncertaintyAnalysis);
            Assert.IsTrue(modSettings.ReSampleRPFs);
            Assert.AreEqual(987, modSettings.NumberOfResampleCycles);
        }


        //Test: EffectRepresentations
        [TestMethod]
        public void Patch_10_01_0000_EffectRepresentationsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "EffectRepresentationsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<EffectRepresentationsModuleConfig>();

            Assert.IsTrue(modSettings.MultipleEffects);
            Assert.IsTrue(modSettings.IncludeAopNetwork);
        }


        //Test: Effects
        [TestMethod]
        public void Patch_10_01_0000_EffectsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "EffectsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<EffectsModuleConfig>();

            Assert.IsTrue(modSettings.MultipleEffects);
            Assert.AreEqual("ADBADF", modSettings.CodeFocalEffect);
            Assert.IsTrue(modSettings.IncludeAopNetwork);
        }

        //Test: ExposureBiomarkerConversions
        [TestMethod]
        public void Patch_10_01_0000_ExposureBiomarkerConversionsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "ExposureBiomarkerConversionsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<ExposureBiomarkerConversionsModuleConfig>();

            Assert.IsTrue(modSettings.EBCSubgroupDependent);
        }


        //Test: ExposureMixtures
        [TestMethod]
        public void Patch_10_01_0000_ExposureMixturesModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "ExposureMixturesModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<ExposureMixturesModuleConfig>();

            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.AreEqual(TargetLevelType.Internal, modSettings.TargetDoseLevelType);
            Assert.AreEqual(ExposureCalculationMethod.MonitoringConcentration, modSettings.ExposureCalculationMethod);
            Assert.AreEqual(ExposureApproachType.UnweightedExposures, modSettings.ExposureApproachType);
            Assert.AreEqual(1.2345D, modSettings.MixtureSelectionSparsenessConstraint);
            Assert.AreEqual(1234, modSettings.NumberOfMixtures);
            Assert.AreEqual(3456, modSettings.MixtureSelectionIterations);
            Assert.AreEqual(10203040, modSettings.MixtureSelectionRandomSeed);
            Assert.AreEqual(4.5678D, modSettings.MixtureSelectionConvergenceCriterium);
            Assert.AreEqual(5.6789D, modSettings.MixtureSelectionRatioCutOff);
            Assert.AreEqual(6.7891D, modSettings.MixtureSelectionTotalExposureCutOff);
            Assert.AreEqual(7.8912D, modSettings.MaximumCumulativeRatioCutOff);
            Assert.AreEqual("1.1 2.22 3.333 4.4444", string.Join(' ', modSettings.MaximumCumulativeRatioPercentiles.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(1.11111D, modSettings.MaximumCumulativeRatioMinimumPercentage);
            Assert.AreEqual(2345, modSettings.NumberOfClusters);
            Assert.AreEqual(ClusterMethodType.Hierarchical, modSettings.ClusterMethodType);
            Assert.IsTrue(modSettings.AutomaticallyDeterminationOfClusters);
            Assert.AreEqual(NetworkAnalysisType.NetworkAnalysis, modSettings.NetworkAnalysisType);
            Assert.IsTrue(modSettings.IsLogTransform);
            Assert.IsTrue(modSettings.IsPerPerson);
            Assert.AreEqual("Jj Kk Ll", string.Join(' ', modSettings.CodesHumanMonitoringSamplingMethods));
        }


        //Test: FocalFoodConcentrations
        [TestMethod]
        public void Patch_10_01_0000_FocalFoodConcentrationsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "FocalFoodConcentrationsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<FocalFoodConcentrationsModuleConfig>();

            Assert.AreEqual(2, modSettings.FocalFoods.Count);
            Assert.AreEqual("Aa", modSettings.FocalFoods[0].CodeFood);
            Assert.AreEqual("Bb", modSettings.FocalFoods[0].CodeSubstance);
            Assert.AreEqual("Cc", modSettings.FocalFoods[1].CodeFood);
            Assert.AreEqual("Dd", modSettings.FocalFoods[1].CodeSubstance);
            Assert.AreEqual(FocalCommodityReplacementMethod.ReplaceSubstances, modSettings.FocalCommodityReplacementMethod);
            Assert.AreEqual(25.25D, modSettings.LowerPercentage);
            Assert.AreEqual(75.75D, modSettings.UpperPercentage);
        }


        //Test: FoodConversions
        [TestMethod]
        public void Patch_10_01_0000_FoodConversionsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "FoodConversionsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<FoodConversionsModuleConfig>();

            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.UseProcessing);
            Assert.IsTrue(modSettings.UseComposition);
            Assert.IsTrue(modSettings.TotalDietStudy);
            Assert.IsTrue(modSettings.UseReadAcrossFoodTranslations);
            Assert.IsTrue(modSettings.UseMarketShares);
            Assert.IsTrue(modSettings.UseSubTypes);
            Assert.IsTrue(modSettings.UseSuperTypes);
            Assert.IsTrue(modSettings.UseDefaultProcessingFactor);
            Assert.IsTrue(modSettings.SubstanceIndependent);
            Assert.IsTrue(modSettings.UseWorstCaseValues);
            Assert.IsTrue(modSettings.FoodIncludeNonDetects);
            Assert.IsTrue(modSettings.CompoundIncludeNonDetects);
            Assert.IsTrue(modSettings.CompoundIncludeNoMeasurements);
        }


        //Test: HazardCharacterisations
        [TestMethod]
        public void Patch_10_01_0000_HazardCharacterisationsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "HazardCharacterisationsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<HazardCharacterisationsModuleConfig>();

            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.IsTrue(modSettings.MultipleEffects);
            Assert.AreEqual(TargetLevelType.Internal, modSettings.TargetDoseLevelType);
            Assert.IsTrue(modSettings.RestrictToCriticalEffect);
            Assert.IsTrue(modSettings.FilterByAvailableHazardCharacterisation);
            Assert.IsTrue(modSettings.IncludeAopNetwork);
            Assert.AreEqual(11223344, modSettings.RandomSeed);
            Assert.AreEqual("ADBADF", modSettings.CodeReferenceSubstance);
            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.AreEqual(TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms, modSettings.TargetDosesCalculationMethod);
            Assert.AreEqual(PointOfDeparture.NOAEL, modSettings.PointOfDeparture);
            Assert.AreEqual(TargetDoseSelectionMethod.Draw, modSettings.TargetDoseSelectionMethod);
            Assert.IsTrue(modSettings.ImputeMissingHazardDoses);
            Assert.AreEqual(HazardDoseImputationMethodType.HazardDosesUnbiased, modSettings.HazardDoseImputationMethod);
            Assert.IsTrue(modSettings.UseDoseResponseModels);
            Assert.IsTrue(modSettings.UseBMDL);
            Assert.IsTrue(modSettings.UseInterSpeciesConversionFactors);
            Assert.IsTrue(modSettings.UseIntraSpeciesConversionFactors);
            Assert.AreEqual(1.2345D, modSettings.AdditionalAssessmentFactor);
            Assert.IsTrue(modSettings.UseAdditionalAssessmentFactor);
            Assert.IsTrue(modSettings.Aggregate);
            Assert.IsTrue(modSettings.HazardCharacterisationsConvertToSingleTargetMatrix);
            Assert.AreEqual(BiologicalMatrix.Lung, modSettings.TargetMatrix);
            Assert.IsTrue(modSettings.HCSubgroupDependent);
            Assert.IsTrue(modSettings.ReSampleIntraSpecies);
            Assert.IsTrue(modSettings.ReSampleRPFs);
            Assert.AreEqual(3.4567D, modSettings.UncertaintyLowerBound);
            Assert.AreEqual(4.5678D, modSettings.UncertaintyUpperBound);
        }


        //Test: HighExposureFoodSubstanceCombinations
        [TestMethod]
        public void Patch_10_01_0000_HighExposureFoodSubstanceCombinationsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "HighExposureFoodSubstanceCombinationsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<HighExposureFoodSubstanceCombinationsModuleConfig>();

            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.IsTrue(modSettings.Cumulative);
            Assert.AreEqual(89.8484D, modSettings.CriticalExposurePercentage);
            Assert.AreEqual(94.4394D, modSettings.CumulativeSelectionPercentage);
            Assert.AreEqual(34.343242D, modSettings.ImportanceLor);
            Assert.IsTrue(modSettings.IsPerPerson);
        }


        //Test: HumanMonitoringAnalysis
        [TestMethod]
        public void Patch_10_01_0000_HumanMonitoringAnalysisModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "HumanMonitoringAnalysisModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<HumanMonitoringAnalysisModuleConfig>();

            Assert.AreEqual(11223344, modSettings.RandomSeed);
            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.Cumulative);
            Assert.IsTrue(modSettings.IsPerPerson);
            Assert.AreEqual("dA cB bC aD", string.Join(' ', modSettings.CodesHumanMonitoringSamplingMethods));
            Assert.AreEqual(NonDetectsHandlingMethod.ReplaceByLOR, modSettings.HumanMonitoringNonDetectsHandlingMethod);
            Assert.AreEqual(1.2345D, modSettings.HumanMonitoringFractionOfLor);
            Assert.AreEqual(NonDetectImputationMethod.CensoredLogNormal, modSettings.NonDetectImputationMethod);
            Assert.AreEqual(MissingValueImputationMethod.ImputeFromData, modSettings.MissingValueImputationMethod);
            Assert.AreEqual("Muscle", modSettings.CodeCompartment);
            Assert.IsTrue(modSettings.ApplyKineticConversions);
            Assert.IsTrue(modSettings.HbmConvertToSingleTargetMatrix);
            Assert.AreEqual(TargetLevelType.Internal, modSettings.HbmTargetSurfaceLevel);
            Assert.AreEqual(3.4567D, modSettings.MissingValueCutOff);
            Assert.IsTrue(modSettings.StandardiseBlood);
            Assert.AreEqual(StandardiseBloodMethod.BernertMethod, modSettings.StandardiseBloodMethod);
            Assert.IsTrue(modSettings.StandardiseBloodExcludeSubstances);
            Assert.AreEqual("Aa Bb Cc Dd", string.Join(' ', modSettings.StandardiseBloodExcludedSubstancesSubset));
            Assert.IsTrue(modSettings.StandardiseUrine);
            Assert.AreEqual(StandardiseUrineMethod.CreatinineStandardisation, modSettings.StandardiseUrineMethod);
            Assert.IsTrue(modSettings.StandardiseUrineExcludeSubstances);
            Assert.AreEqual("Ee Ff Gg Hh", string.Join(' ', modSettings.StandardiseUrineExcludedSubstancesSubset));
            Assert.AreEqual(ExposureApproachType.UnweightedExposures, modSettings.ExposureApproachType);
            Assert.IsTrue(modSettings.AnalyseMcr);
            Assert.AreEqual(4.5678D, modSettings.MaximumCumulativeRatioCutOff);
            Assert.AreEqual("1.1 2.22 3.333 4.4444", string.Join(' ', modSettings.MaximumCumulativeRatioPercentiles.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(5.6789D, modSettings.MaximumCumulativeRatioMinimumPercentage);
            Assert.AreEqual(31.882D, modSettings.MixtureSelectionRatioCutOff);
            Assert.AreEqual(44.321D, modSettings.MixtureSelectionTotalExposureCutOff);
            Assert.IsTrue(modSettings.ApplyExposureBiomarkerConversions);
            Assert.AreEqual(BiologicalMatrix.Saliva, modSettings.TargetMatrix);
            Assert.AreEqual(321, modSettings.NumberOfMonteCarloIterations);
            Assert.AreEqual(4.5667D, modSettings.SpecificGravityConversionFactor);
            Assert.AreEqual(10.10111D, modSettings.UncertaintyLowerBound);
            Assert.AreEqual(11.22222D, modSettings.UncertaintyUpperBound);
            Assert.AreEqual(654, modSettings.NumberOfIterationsPerResampledSet);
            Assert.IsTrue(modSettings.ResampleHBMIndividuals);
            Assert.IsTrue(modSettings.SkipPrivacySensitiveOutputs);
            Assert.AreEqual(1.1112D, modSettings.LowerPercentage);
            Assert.AreEqual(78.434D, modSettings.UpperPercentage);
            Assert.AreEqual(88.883D, modSettings.PercentageForUpperTail);
            Assert.IsTrue(modSettings.StoreIndividualDayIntakes);
        }


        //Test: HumanMonitoringData
        [TestMethod]
        public void Patch_10_01_0000_HumanMonitoringDataModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "HumanMonitoringDataModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<HumanMonitoringDataModuleConfig>();

            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.AreEqual("Jj Kk Ll", string.Join(' ', modSettings.CodesHumanMonitoringSamplingMethods));
            Assert.AreEqual(IndividualSubsetType.IgnorePopulationDefinition, modSettings.MatchHbmIndividualSubsetWithPopulation);
            Assert.AreEqual("Aa Bb Cc Dd", string.Join(' ', modSettings.SelectedHbmSurveySubsetProperties));
            Assert.IsTrue(modSettings.UseHbmSamplingWeights);
            Assert.IsTrue(modSettings.UseCompleteAnalysedSamples);
            Assert.IsTrue(modSettings.ExcludeSubstancesFromSamplingMethod);
            Assert.IsTrue(modSettings.FilterRepeatedMeasurements);
            Assert.IsTrue(modSettings.PopulationSubsetSelection);
            Assert.IsTrue(modSettings.ResampleHBMIndividuals);
            Assert.IsTrue(modSettings.SkipPrivacySensitiveOutputs);
            Assert.AreEqual(1.1112D, modSettings.LowerPercentage);
            Assert.AreEqual(78.434D, modSettings.UpperPercentage);

            Assert.AreEqual("Idsds", modSettings.IndividualDaySubsetDefinition.NameIndividualProperty);
            Assert.IsTrue(modSettings.IndividualDaySubsetDefinition.IncludeMissingValueRecords);
            Assert.AreEqual("1 4 12", string.Join(" ", modSettings.IndividualDaySubsetDefinition.MonthsSubset));
            Assert.AreEqual("A BB cCc", string.Join(" ", modSettings.RepeatedMeasurementTimepointCodes));

            var exs = modSettings.ExcludedSubstancesFromSamplingMethodSubset[0];
            Assert.AreEqual("ABcd", exs.SamplingMethodCode);
            Assert.AreEqual("ZzZz", exs.SubstanceCode);
            exs = modSettings.ExcludedSubstancesFromSamplingMethodSubset[1];
            Assert.AreEqual("ABcd", exs.SamplingMethodCode);
            Assert.AreEqual("YyYy", exs.SubstanceCode);
            exs = modSettings.ExcludedSubstancesFromSamplingMethodSubset[2];
            Assert.AreEqual("Defg", exs.SamplingMethodCode);
            Assert.AreEqual("XxXx", exs.SubstanceCode);

            var idv = modSettings.IndividualsSubsetDefinitions[0];
            Assert.AreEqual("DoubleRange", idv.NameIndividualProperty);
            Assert.AreEqual("61-93", idv.IndividualPropertyQuery);
            Assert.AreEqual(QueryDefinitionType.Range, idv.GetQueryDefinitionType());
            Assert.AreEqual(61, idv.GetRangeMin());
            Assert.AreEqual(93, idv.GetRangeMax());
            idv = modSettings.IndividualsSubsetDefinitions[1];
            Assert.AreEqual("Keywords", idv.NameIndividualProperty);
            Assert.AreEqual("'a b','sf','99'", idv.IndividualPropertyQuery);
            Assert.AreEqual(QueryDefinitionType.ValueList, idv.GetQueryDefinitionType());
            Assert.AreEqual("a b|sf|99", string.Join('|', idv.GetQueryKeywords()));
            idv = modSettings.IndividualsSubsetDefinitions[2];
            Assert.AreEqual("Empty", idv.NameIndividualProperty);
            Assert.AreEqual("-", idv.IndividualPropertyQuery);
            Assert.AreEqual(QueryDefinitionType.Empty, idv.GetQueryDefinitionType());
        }


        //Test: InterSpeciesConversions
        [TestMethod]
        public void Patch_10_01_0000_InterSpeciesConversionsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "InterSpeciesConversionsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<InterSpeciesConversionsModuleConfig>();

            Assert.AreEqual(1.2345D, modSettings.DefaultInterSpeciesFactorGeometricMean);
            Assert.AreEqual(2.3456D, modSettings.DefaultInterSpeciesFactorGeometricStandardDeviation);
            Assert.IsTrue(modSettings.FilterByAvailableHazardCharacterisation);
            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.UseInterSpeciesConversionFactors);
            Assert.IsTrue(modSettings.ReSampleInterspecies);
        }


        //Test: IntraSpeciesFactors
        [TestMethod]
        public void Patch_10_01_0000_IntraSpeciesFactorsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "IntraSpeciesFactorsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<IntraSpeciesFactorsModuleConfig>();

            Assert.AreEqual(1.2345D, modSettings.DefaultIntraSpeciesFactor);
            Assert.IsTrue(modSettings.FilterByAvailableHazardCharacterisation);
            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.ReSampleIntraSpecies);
        }


        //Test: KineticModels
        [TestMethod]
        public void Patch_10_01_0000_KineticModelsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "KineticModelsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<KineticModelsModuleConfig>();

            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.Aggregate);
            Assert.IsTrue(modSettings.FilterByAvailableHazardCharacterisation);
            Assert.AreEqual(1.2345D, modSettings.OralAbsorptionFactor);
            Assert.AreEqual(2.3456D, modSettings.OralAbsorptionFactorForDietaryExposure);
            Assert.AreEqual(3.4567D, modSettings.DermalAbsorptionFactor);
            Assert.AreEqual(4.5678D, modSettings.InhalationAbsorptionFactor);
            Assert.AreEqual(129, modSettings.NumberOfDays);
            Assert.AreEqual(238, modSettings.NumberOfDosesPerDay);
            Assert.AreEqual(347, modSettings.NonStationaryPeriod);
            Assert.AreEqual("ADBADF", modSettings.CodeKineticModel);
            Assert.IsTrue(modSettings.UseParameterVariability);
            Assert.AreEqual(InternalModelType.ConversionFactorModel, modSettings.InternalModelType);
            Assert.AreEqual("ArterialBlood", modSettings.CodeCompartment);
            Assert.AreEqual(123, modSettings.NumberOfDosesPerDayNonDietaryDermal);
            Assert.AreEqual(456, modSettings.NumberOfDosesPerDayNonDietaryInhalation);
            Assert.AreEqual(789, modSettings.NumberOfDosesPerDayNonDietaryOral);
            Assert.AreEqual("1 22 333 4444 55555", string.Join(' ', modSettings.SelectedEvents));
            Assert.IsTrue(modSettings.SpecifyEvents);
            Assert.IsTrue(modSettings.KCFSubgroupDependent);
            Assert.IsTrue(modSettings.ResampleKineticModelParameters);
        }


        //Test: ModelledFoods
        [TestMethod]
        public void Patch_10_01_0000_ModelledFoodsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "ModelledFoodsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<ModelledFoodsModuleConfig>();

            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.RestrictToModelledFoodSubset);
            Assert.AreEqual("Aa Bb Cc Dd", string.Join(' ', modSettings.ModelledFoodSubset));
            Assert.IsTrue(modSettings.DeriveModelledFoodsFromSampleBasedConcentrations);
            Assert.IsTrue(modSettings.DeriveModelledFoodsFromSingleValueConcentrations);
            Assert.IsTrue(modSettings.UseWorstCaseValues);
            Assert.IsTrue(modSettings.FoodIncludeNonDetects);
            Assert.IsTrue(modSettings.CompoundIncludeNonDetects);
            Assert.IsTrue(modSettings.CompoundIncludeNoMeasurements);
        }


        //Test: MolecularDockingModels
        [TestMethod]
        public void Patch_10_01_0000_MolecularDockingModelsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "MolecularDockingModelsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<MolecularDockingModelsModuleConfig>();

            Assert.IsTrue(modSettings.MultipleEffects);
            Assert.IsTrue(modSettings.IncludeAopNetwork);
        }


        //Test: NonDietaryExposures
        [TestMethod]
        public void Patch_10_01_0000_NonDietaryExposuresModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "NonDietaryExposuresModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<NonDietaryExposuresModuleConfig>();

            Assert.IsTrue(modSettings.MatchSpecificIndividuals);
            Assert.IsTrue(modSettings.ReSampleNonDietaryExposures);
        }


        //Test: OccurrenceFrequencies
        [TestMethod]
        public void Patch_10_01_0000_OccurrenceFrequenciesModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "OccurrenceFrequenciesModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<OccurrenceFrequenciesModuleConfig>();

            Assert.IsTrue(modSettings.SetMissingAgriculturalUseAsUnauthorized);
            Assert.IsTrue(modSettings.UseAgriculturalUsePercentage);
            Assert.AreEqual(SettingsTemplateType.Ec2018DietaryCraChronicTier1, modSettings.OccurrencePatternsTier);
            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.RecomputeOccurrencePatterns);
        }


        //Test: OccurrencePatterns
        [TestMethod]
        public void Patch_10_01_0000_OccurrencePatternsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "OccurrencePatternsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<OccurrencePatternsModuleConfig>();

            Assert.AreEqual(SettingsTemplateType.Ec2018DietaryCraChronicTier2, modSettings.OccurrencePatternsTier);
            Assert.IsTrue(modSettings.SetMissingAgriculturalUseAsUnauthorized);
            Assert.IsTrue(modSettings.UseAgriculturalUsePercentage);
            Assert.IsTrue(modSettings.ScaleUpOccurencePatterns);
            Assert.IsTrue(modSettings.RestrictOccurencePatternScalingToAuthorisedUses);
            Assert.IsTrue(modSettings.Cumulative);
            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.RecomputeOccurrencePatterns);
        }


        //Test: PointsOfDeparture
        [TestMethod]
        public void Patch_10_01_0000_PointsOfDepartureModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "PointsOfDepartureModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<PointsOfDepartureModuleConfig>();

            Assert.IsTrue(modSettings.MultipleEffects);
            Assert.IsTrue(modSettings.IncludeAopNetwork);
            Assert.IsTrue(modSettings.DoUncertaintyAnalysis);
            Assert.IsTrue(modSettings.ReSampleRPFs);
        }


        //Test: Populations
        [TestMethod]
        public void Patch_10_01_0000_PopulationsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "PopulationsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<PopulationsModuleConfig>();

            Assert.IsTrue(modSettings.PopulationSubsetSelection);
            Assert.AreEqual(123.45D, modSettings.NominalPopulationBodyWeight);

            Assert.AreEqual(3, modSettings.IndividualsSubsetDefinitions.Count);
            var idv = modSettings.IndividualsSubsetDefinitions[0];
            Assert.AreEqual("DoubleRange", idv.NameIndividualProperty);
            Assert.AreEqual("61-93", idv.IndividualPropertyQuery);
            Assert.AreEqual(QueryDefinitionType.Range, idv.GetQueryDefinitionType());
            Assert.AreEqual(61, idv.GetRangeMin());
            Assert.AreEqual(93, idv.GetRangeMax());
            idv = modSettings.IndividualsSubsetDefinitions[1];
            Assert.AreEqual("Keywords", idv.NameIndividualProperty);
            Assert.AreEqual("'a b','sf','99'", idv.IndividualPropertyQuery);
            Assert.AreEqual(QueryDefinitionType.ValueList, idv.GetQueryDefinitionType());
            Assert.AreEqual("a b|sf|99", string.Join('|', idv.GetQueryKeywords()));
            idv = modSettings.IndividualsSubsetDefinitions[2];
            Assert.AreEqual("Empty", idv.NameIndividualProperty);
            Assert.AreEqual("-", idv.IndividualPropertyQuery);
            Assert.AreEqual(QueryDefinitionType.Empty, idv.GetQueryDefinitionType());

            Assert.AreEqual("Idsds", modSettings.IndividualDaySubsetDefinition.NameIndividualProperty);
            Assert.IsTrue(modSettings.IndividualDaySubsetDefinition.IncludeMissingValueRecords);
            Assert.AreEqual("1 4 12", string.Join(" ", modSettings.IndividualDaySubsetDefinition.MonthsSubset));
        }


        //Test: ProcessingFactors
        [TestMethod]
        public void Patch_10_01_0000_ProcessingFactorsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "ProcessingFactorsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<ProcessingFactorsModuleConfig>();

            Assert.IsTrue(modSettings.IsProcessing);
            Assert.IsTrue(modSettings.IsDistribution);
            Assert.IsTrue(modSettings.AllowHigherThanOne);
            Assert.IsTrue(modSettings.ReSampleProcessingFactors);
        }


        //Test: QsarMembershipModels
        [TestMethod]
        public void Patch_10_01_0000_QsarMembershipModelsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "QsarMembershipModelsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<QsarMembershipModelsModuleConfig>();

            Assert.IsTrue(modSettings.MultipleEffects);
            Assert.IsTrue(modSettings.IncludeAopNetwork);
        }


        //Test: RelativePotencyFactors
        [TestMethod]
        public void Patch_10_01_0000_RelativePotencyFactorsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "RelativePotencyFactorsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<RelativePotencyFactorsModuleConfig>();

            Assert.IsTrue(modSettings.MultipleEffects);
            Assert.IsTrue(modSettings.IncludeAopNetwork);
            Assert.AreEqual("RF-00055555-VET", modSettings.CodeReferenceSubstance);
            Assert.IsTrue(modSettings.ReSampleRPFs);
            Assert.AreEqual(3.4567D, modSettings.UncertaintyLowerBound);
            Assert.AreEqual(4.5678D, modSettings.UncertaintyUpperBound);
        }


        //Test: Risks
        [TestMethod]
        public void Patch_10_01_0000_RisksModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "RisksModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<RisksModuleConfig>();

            Assert.AreEqual(SettingsTemplateType.Efsa2022DietaryCraChronicTier1, modSettings.RiskCalculationTier);
            Assert.AreEqual(11223344, modSettings.RandomSeed);
            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.CumulativeRisk);
            Assert.AreEqual(HealthEffectType.Benefit, modSettings.HealthEffectType);
            Assert.AreEqual(RiskMetricType.ExposureHazardRatio, modSettings.RiskMetricType);
            Assert.IsTrue(modSettings.IsEAD);
            Assert.AreEqual(3.4567D, modSettings.ThresholdMarginOfExposure);
            Assert.IsTrue(modSettings.IsInverseDistribution);
            Assert.IsTrue(modSettings.Aggregate);
            Assert.AreEqual(TargetLevelType.Internal, modSettings.TargetDoseLevelType);
            Assert.IsTrue(modSettings.CalculateRisksByFood);
            Assert.AreEqual(ExposureCalculationMethod.MonitoringConcentration, modSettings.ExposureCalculationMethod);
            Assert.AreEqual(RiskMetricCalculationType.SumRatios, modSettings.RiskMetricCalculationType);
            Assert.IsTrue(modSettings.AnalyseMcr);
            Assert.AreEqual(ExposureApproachType.UnweightedExposures, modSettings.ExposureApproachType);
            Assert.AreEqual(5.6789D, modSettings.MixtureSelectionRatioCutOff);
            Assert.AreEqual(6.7891D, modSettings.MixtureSelectionTotalExposureCutOff);
            Assert.AreEqual(4.5678D, modSettings.MaximumCumulativeRatioCutOff);
            Assert.AreEqual("1.1 2.22 3.333 4.4444", string.Join(' ', modSettings.MaximumCumulativeRatioPercentiles.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(21.345D, modSettings.MaximumCumulativeRatioMinimumPercentage);
            Assert.IsTrue(modSettings.UseIntraSpeciesConversionFactors);
            Assert.AreEqual("Jj Kk Ll", string.Join(' ', modSettings.CodesHumanMonitoringSamplingMethods));
            Assert.IsTrue(modSettings.HCSubgroupDependent);
            Assert.AreEqual(2.2222D, modSettings.UncertaintyLowerBound);
            Assert.AreEqual(89.433D, modSettings.UncertaintyUpperBound);
            Assert.AreEqual(1.2345D, modSettings.LeftMargin);
            Assert.AreEqual(2.3456D, modSettings.RightMargin);
            Assert.AreEqual(123, modSettings.NumberOfLabels);
            Assert.AreEqual(456, modSettings.NumberOfSubstances);
            Assert.AreEqual(1.2345D, modSettings.ConfidenceInterval);
            Assert.IsTrue(modSettings.SkipPrivacySensitiveOutputs);
            Assert.IsTrue(modSettings.IsDetailedOutput);
            Assert.AreEqual("40 44 45.66 88.88", string.Join(' ', modSettings.SelectedPercentiles.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(91.2345D, modSettings.PercentageForDrilldown);
            Assert.AreEqual(92.3456D, modSettings.PercentageForUpperTail);
            Assert.AreEqual(444.444D, modSettings.Intervals);
            Assert.AreEqual("11.11 22.22 33.33", string.Join(' ', modSettings.ExtraPredictionLevels.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(2.5555D, modSettings.LowerPercentage);
            Assert.AreEqual(93.4567, modSettings.UpperPercentage);
            Assert.IsTrue(modSettings.IsPerPerson);
        }


        //Test: SingleValueConcentrations
        [TestMethod]
        public void Patch_10_01_0000_SingleValueConcentrationsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "SingleValueConcentrationsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<SingleValueConcentrationsModuleConfig>();

            Assert.IsTrue(modSettings.UseDeterministicConversionFactors);
        }


        //Test: SingleValueConsumptions
        [TestMethod]
        public void Patch_10_01_0000_SingleValueConsumptionsModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "SingleValueConsumptionsModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<SingleValueConsumptionsModuleConfig>();

            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.IsTrue(modSettings.ConsumerDaysOnly);
            Assert.IsTrue(modSettings.IsDefaultSamplingWeight);
            Assert.IsTrue(modSettings.UseBodyWeightStandardisedConsumptionDistribution);
            Assert.IsTrue(modSettings.IsProcessing);
            Assert.IsTrue(modSettings.ModelledFoodsConsumerDaysOnly);
        }


        //Test: SingleValueDietaryExposures
        [TestMethod]
        public void Patch_10_01_0000_SingleValueDietaryExposuresModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "SingleValueDietaryExposuresModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<SingleValueDietaryExposuresModuleConfig>();

            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.AreEqual(SettingsTemplateType.Ec2018DietaryCraChronicTier1, modSettings.DietaryIntakeCalculationTier);
            Assert.AreEqual(SingleValueDietaryExposuresCalculationMethod.NEDI2, modSettings.SingleValueDietaryExposureCalculationMethod);
            Assert.IsTrue(modSettings.IsProcessing);
            Assert.IsTrue(modSettings.IsPerPerson);
            Assert.IsTrue(modSettings.UseOccurrenceFrequencies);
            Assert.IsTrue(modSettings.UseUnitVariability);
            Assert.AreEqual(ModelledFoodsCalculationSource.UseWorstCaseValues, modSettings.ModelledFoodsCalculationSource);
            Assert.AreEqual(2.2222D, modSettings.UncertaintyLowerBound);
            Assert.AreEqual(89.433D, modSettings.UncertaintyUpperBound);
            Assert.AreEqual(1.1112D, modSettings.LowerPercentage);
            Assert.AreEqual(78.434D, modSettings.UpperPercentage);
        }

        //Test: SingleValueNonDietaryExposures
        [TestMethod]
        public void Patch_10_01_0000_SingleValueNonDietaryExposuresModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "SingleValueNonDietaryExposuresModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<SingleValueNonDietaryExposuresModuleConfig>();

            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.AreEqual("ADFHIGUHFDG", modSettings.CodeConfiguration);
        }


        //Test: SingleValueRisks
        [TestMethod]
        public void Patch_10_01_0000_SingleValueRisksModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "SingleValueRisksModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<SingleValueRisksModuleConfig>();

            Assert.AreEqual(SettingsTemplateType.Efsa2022DietaryCraChronicTier2, modSettings.SingleValueRisksCalculationTier);
            Assert.AreEqual(SingleValueRiskCalculationMethod.FromIndividualRisks, modSettings.SingleValueRiskCalculationMethod);
            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.Cumulative);
            Assert.AreEqual(HealthEffectType.Benefit, modSettings.HealthEffectType);
            Assert.AreEqual(RiskMetricType.ExposureHazardRatio, modSettings.RiskMetricType);
            Assert.AreEqual(0.12345D, modSettings.Percentage);
            Assert.IsTrue(modSettings.IsInverseDistribution);
            Assert.IsTrue(modSettings.UseAdjustmentFactors);
            Assert.AreEqual(AdjustmentFactorDistributionMethod.Gamma, modSettings.ExposureAdjustmentFactorDistributionMethod);
            Assert.AreEqual(1.2345D, modSettings.ExposureParameterA);
            Assert.AreEqual(2.3456D, modSettings.ExposureParameterB);
            Assert.AreEqual(3.4567D, modSettings.ExposureParameterC);
            Assert.AreEqual(4.5678D, modSettings.ExposureParameterD);
            Assert.AreEqual(AdjustmentFactorDistributionMethod.Beta, modSettings.HazardAdjustmentFactorDistributionMethod);
            Assert.AreEqual(5.6789D, modSettings.HazardParameterA);
            Assert.AreEqual(6.7891D, modSettings.HazardParameterB);
            Assert.AreEqual(7.8912D, modSettings.HazardParameterC);
            Assert.AreEqual(8.9123D, modSettings.HazardParameterD);
            Assert.IsTrue(modSettings.UseBackgroundAdjustmentFactor);
            Assert.IsTrue(modSettings.FocalCommodity);
            Assert.AreEqual(FocalCommodityReplacementMethod.MeasurementRemoval, modSettings.FocalCommodityReplacementMethod);
            Assert.IsTrue(modSettings.IsPerPerson);
            Assert.AreEqual(2.2222D, modSettings.UncertaintyLowerBound);
            Assert.AreEqual(89.433D, modSettings.UncertaintyUpperBound);
        }


        //Test: Substances
        [TestMethod]
        public void Patch_10_01_0000_SubstancesModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "SubstancesModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<SubstancesModuleConfig>();

            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.AreEqual("RF-9999988-VET", modSettings.CodeReferenceSubstance);
        }


        //Test: TargetExposures
        [TestMethod]
        public void Patch_10_01_0000_TargetExposuresModuleConfigTest() {
            var resourceFile = Path.Combine(XmlResourceFolder, "TargetExposuresModuleSettings.xml");
            var xml = createMockSettingsXmlFromFile(resourceFile, new Version(10, 0, 10));
            var settings = ProjectSettingsSerializer.ImportFromXmlString(xml, null, false, out _);
            Assert.IsNotNull(settings);

            var modSettings = settings.GetModuleConfiguration<TargetExposuresModuleConfig>();

            Assert.AreEqual(ExposureType.Chronic, modSettings.ExposureType);
            Assert.AreEqual(11223344, modSettings.RandomSeed);
            Assert.IsTrue(modSettings.MultipleSubstances);
            Assert.IsTrue(modSettings.Cumulative);
            Assert.IsTrue(modSettings.Aggregate);
            Assert.AreEqual(TargetLevelType.Internal, modSettings.TargetDoseLevelType);
            Assert.IsTrue(modSettings.MatchSpecificIndividuals);
            Assert.IsTrue(modSettings.IsCorrelationBetweenIndividuals);
            Assert.IsTrue(modSettings.FirstModelThenAdd);
            Assert.AreEqual("HAP8IUGRHA", modSettings.CodeCompartment);
            Assert.AreEqual("ADBADF", modSettings.CodeKineticModel);
            Assert.AreEqual(InternalModelType.ConversionFactorModel, modSettings.InternalModelType);
            Assert.AreEqual(IntakeModelType.LNN, modSettings.IntakeModelType);
            Assert.AreEqual(ExposureApproachType.UnweightedExposures, modSettings.ExposureApproachType);
            Assert.IsTrue(modSettings.AnalyseMcr);
            Assert.AreEqual(55.555D, modSettings.MaximumCumulativeRatioCutOff);
            Assert.AreEqual("95 99 99.55", string.Join(' ', modSettings.MaximumCumulativeRatioPercentiles.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(44.444D, modSettings.MaximumCumulativeRatioMinimumPercentage);
            Assert.AreEqual(1.2345D, modSettings.MixtureSelectionRatioCutOff);
            Assert.AreEqual(2.3456D, modSettings.MixtureSelectionTotalExposureCutOff);
            Assert.IsTrue(modSettings.ResampleKineticModelParameters);
            Assert.AreEqual(3.4567D, modSettings.UncertaintyLowerBound);
            Assert.AreEqual(4.5678D, modSettings.UncertaintyUpperBound);
            Assert.IsTrue(modSettings.SkipPrivacySensitiveOutputs);
            Assert.IsTrue(modSettings.IsDetailedOutput);
            Assert.IsTrue(modSettings.StoreIndividualDayIntakes);
            Assert.AreEqual("40 44 45.66 88.88", string.Join(' ', modSettings.SelectedPercentiles.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(91.2345D, modSettings.PercentageForDrilldown);
            Assert.AreEqual(92.3456D, modSettings.PercentageForUpperTail);
            Assert.AreEqual(ExposureMethod.Automatic, modSettings.ExposureMethod);
            Assert.AreEqual("1 10.07 50", string.Join(' ', modSettings.ExposureLevels.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(444.444D, modSettings.Intervals);
            Assert.AreEqual("11.11 22.22 33.33", string.Join(' ', modSettings.ExtraPredictionLevels.Select(t => t.ToString(CultureInfo.InvariantCulture))));
            Assert.AreEqual(2.5555D, modSettings.LowerPercentage);
            Assert.AreEqual(93.4567, modSettings.UpperPercentage);
            Assert.IsTrue(modSettings.IsPerPerson);
        }
    }
}