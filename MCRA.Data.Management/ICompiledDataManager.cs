using MCRA.Data.Compiled.Objects;

namespace MCRA.Data.Management {
    public interface ICompiledDataManager {
        bool WriteToZippedCsvFile(string filename);

        ICollection<OccurrencePattern> GetAllOccurrencePatterns();
        IDictionary<string, AnalyticalMethod> GetAllAnalyticalMethods();
        IDictionary<string, AnalyticalMethod> GetAllFocalFoodAnalyticalMethods();
        ICollection<SubstanceAuthorisation> GetAllSubstanceAuthorisations();
        ICollection<SubstanceApproval> GetAllSubstanceApprovals();
        IDictionary<string, Compound> GetAllCompounds();
        IList<ConcentrationDistribution> GetAllConcentrationDistributions();
        IList<ConcentrationSingleValue> GetAllConcentrationSingleValues();
        IDictionary<string, Effect> GetAllEffects();
        IDictionary<string, List<RelativePotencyFactor>> GetAllRelativePotencyFactors();
        ICollection<PointOfDeparture> GetAllPointsOfDeparture();
        ICollection<HazardCharacterisation> GetAllHazardCharacterisations();
        IDictionary<string, Food> GetAllFocalCommodityFoods();
        IList<FoodConsumption> GetAllFoodConsumptions();
        IList<PopulationConsumptionSingleValue> GetAllPopulationConsumptionSingleValues();
        IDictionary<string, Food> GetAllFoods();
        IDictionary<string, FoodSurvey> GetAllFoodSurveys();
        IDictionary<string, Individual> GetAllIndividuals();
        IDictionary<string, IndividualProperty> GetAllIndividualProperties();
        IList<ConcentrationLimit> GetAllMaximumConcentrationLimits();
        IList<SubstanceConversion> GetAllSubstanceConversions();
        IList<DeterministicSubstanceConversionFactor> GetAllDeterministicSubstanceConversionFactors();
        IList<ProcessingFactor> GetAllProcessingFactors();
        IDictionary<string, ProcessingType> GetAllProcessingTypes();
        IDictionary<Food, ICollection<Food>> GetAllFoodExtrapolations();
        IDictionary<string, FoodSample> GetAllFoodSamples();
        ICollection<int> GetAllSampleYears();
        ICollection<string> GetAllSampleProductionMethods();
        ICollection<string> GetAllSampleLocations();
        ICollection<string> GetAllSampleRegions();
        IDictionary<string, SampleProperty> GetAllAdditionalSampleProperties();
        IDictionary<string, SampleProperty> GetAllFocalSampleProperties();

        IDictionary<string, FoodSample> GetAllFocalFoodSamples();
        IList<TDSFoodSampleComposition> GetAllTDSFoodSampleCompositions();
        IList<UnitVariabilityFactor> GetAllUnitVariabilityFactors();
        IList<IestiSpecialCase> GetAllIestiSpecialCases();
        IDictionary<string, DietaryExposureModel> GetAllDietaryExposureModels();
        IDictionary<string, TargetExposureModel> GetAllTargetExposureModels();
        IDictionary<string, RiskModel> GetAllRiskModels();
        IList<NonDietaryExposureSet> GetAllNonDietaryExposureSets();
        IDictionary<string, NonDietaryExposureSource> GetAllNonDietaryExposureSources();
        ICollection<OccurrenceFrequency> GetAllOccurrenceFrequencies();
        IDictionary<string, Response> GetAllResponses();
        IDictionary<string, TestSystem> GetAllTestSystems();
        IDictionary<string, DoseResponseExperiment> GetAllDoseResponseExperiments();
        IList<DoseResponseModel> GetAllDoseResponseModels();
        IList<KineticModelInstance> GetAllPbkModels(string dataFolderName = null);
        IList<SimpleAbsorptionFactor> GetAllAbsorptionFactors();
        IList<KineticConversionFactor> GetAllKineticConversionFactors();
        IList<EffectRepresentation> GetAllEffectRepresentations();
        IDictionary<string, AdverseOutcomePathwayNetwork> GetAdverseOutcomePathwayNetworks();
        IDictionary<string, ActiveSubstanceModel> GetAllActiveSubstanceModels();
        IDictionary<string, MolecularDockingModel> GetAllMolecularDockingModels();
        IDictionary<string, QsarMembershipModel> GetAllQsarMembershipModels();
        ICollection<InterSpeciesFactor> GetAllInterSpeciesFactors();
        ICollection<IntraSpeciesFactor> GetAllIntraSpeciesFactors();

        IDictionary<string, HumanMonitoringSurvey> GetAllHumanMonitoringSurveys();
        IDictionary<string, Individual> GetAllHumanMonitoringIndividuals();
        IDictionary<string, IndividualProperty> GetAllHumanMonitoringIndividualProperties();
        IDictionary<string, HumanMonitoringSample> GetAllHumanMonitoringSamples();
        ICollection<HumanMonitoringSamplingMethod> GetAllHumanMonitoringSamplingMethods();
        IDictionary<string, AnalyticalMethod> GetAllHumanMonitoringAnalyticalMethods();

        IDictionary<string, Population> GetAllPopulations();
        IList<FoodTranslation> GetAllFoodTranslations();
        IList<MarketShare> GetAllMarketShares();
        ICollection<ExposureBiomarkerConversion> GetAllExposureBiomarkerConversions();

        IDictionary<string, ExposureScenario> GetAllSingleValuNonDietaryExposureScenarios();
        IDictionary<string, ExposureDeterminantCombination> GetAllSingleValueNonDietaryExposureDeterminantCombinations();
        IList<ExposureEstimate> GetAllSingleValueNonDietaryExposures();
        IList<DustConcentrationDistribution> GetAllDustConcentrationDistributions();
        IList<DustIngestion> GetAllDustIngestions();
        IList<DustBodyExposureFraction> GetAllDustBodyExposureFractions();
        IList<DustAdherenceAmount> GetAllDustAdherenceAmounts();
        IList<DustAvailabilityFraction> GetAllDustAvailabilityFractions();
        IList<ExposureEffectFunction> GetAllExposureEffectFunctions();
        IDictionary<string, PbkModelDefinition> GetAllPbkModelDefinitions(string dataFilePath = null);
        IList<SoilConcentrationDistribution> GetAllSoilConcentrationDistributions();
        IList<SoilIngestion> GetAllSoilIngestions();
        IList<BaselineBodIndicator> GetAllBaselineBodIndicators();
    }
}
