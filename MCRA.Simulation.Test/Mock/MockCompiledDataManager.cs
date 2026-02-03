using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers;

namespace MCRA.Simulation.Test.Mock {

    public class MockCompiledDataManager : ICompiledDataManager {

        private CompiledData _data;

        public MockCompiledDataManager(CompiledData data) {
            _data = data;
        }

        public IDictionary<string, AdverseOutcomePathwayNetwork> GetAdverseOutcomePathwayNetworks() {
            return _data.AllAdverseOutcomePathwayNetworks;
        }

        public ICollection<OccurrencePattern> GetAllOccurrencePatterns() {
            return _data.AllOccurrencePatterns;
        }

        public IDictionary<string, AnalyticalMethod> GetAllAnalyticalMethods() {
            return _data.AllAnalyticalMethods;
        }

        public IDictionary<string, ActiveSubstanceModel> GetAllActiveSubstanceModels() {
            return _data.AllActiveSubstanceModels;
        }

        public ICollection<SubstanceAuthorisation> GetAllSubstanceAuthorisations() {
            return _data.AllSubstanceAuthorisations;
        }

        public ICollection<SubstanceApproval> GetAllSubstanceApprovals() {
            return _data.AllSubstanceApprovals;
        }

        public IDictionary<string, Compound> GetAllCompounds() {
            return _data.AllSubstances;
        }

        public IList<ConcentrationDistribution> GetAllConcentrationDistributions() {
            return _data.AllConcentrationDistributions;
        }

        public IDictionary<string, DoseResponseExperiment> GetAllDoseResponseExperiments() {
            return _data.AllDoseResponseExperiments;
        }

        public IList<DoseResponseModel> GetAllDoseResponseModels() {
            return _data.AllDoseResponseModels.Select(c => c.Value).ToList();
        }

        public IList<EffectRepresentation> GetAllEffectRepresentations() {
            return _data.AllEffectRepresentations;
        }

        public IDictionary<string, Effect> GetAllEffects() {
            return _data.AllEffects;
        }

        public IDictionary<string, Food> GetAllFocalCommodityFoods() {
            throw new NotImplementedException();
        }

        public IList<FoodConsumption> GetAllFoodConsumptions() {
            return _data.AllFoodConsumptions;
        }

        public IDictionary<Food, ICollection<Food>> GetAllFoodExtrapolations() {
            return _data.AllFoodExtrapolations;
        }

        public IDictionary<string, Food> GetAllFoods() {
            return _data.AllFoods;
        }

        public IDictionary<string, FoodSurvey> GetAllFoodSurveys() {
            return _data.AllFoodSurveys;
        }

        public IList<FoodTranslation> GetAllFoodTranslations() {
            return _data.AllFoodTranslations;
        }

        public ICollection<PointOfDeparture> GetAllPointsOfDeparture() {
            return _data.AllPointsOfDeparture;
        }

        public ICollection<HazardCharacterisation> GetAllHazardCharacterisations() {
            return _data.AllHazardCharacterisations;
        }

        public IDictionary<string, Individual> GetAllHumanMonitoringIndividuals() {
            return _data.AllHumanMonitoringIndividuals;
        }

        public IDictionary<string, HumanMonitoringSample> GetAllHumanMonitoringSamples() {
            return _data.AllHumanMonitoringSamples;
        }

        public ICollection<HumanMonitoringSamplingMethod> GetAllHumanMonitoringSamplingMethods() {
            return _data.HumanMonitoringSamplingMethods;
        }

        public IDictionary<string, HumanMonitoringSurvey> GetAllHumanMonitoringSurveys() {
            return _data.AllHumanMonitoringSurveys;
        }

        public IDictionary<string, AnalyticalMethod> GetAllHumanMonitoringAnalyticalMethods() {
            return _data.AllHumanMonitoringAnalyticalMethods;
        }

        public IDictionary<string, Individual> GetAllIndividuals() {
            return _data.AllIndividuals;
        }

        public IDictionary<string, IndividualProperty> GetAllIndividualProperties() {
            return _data.AllDietaryIndividualProperties;
        }

        public ICollection<InterSpeciesFactor> GetAllInterSpeciesFactors() {
            return _data.AllInterSpeciesFactors;
        }

        public ICollection<IntraSpeciesFactor> GetAllIntraSpeciesFactors() {
            return _data.AllIntraSpeciesFactors;
        }

        public IList<SimpleAbsorptionFactor> GetAllAbsorptionFactors() {
            return _data.AllAbsorptionFactors;
        }

        public IList<KineticConversionFactor> GetAllKineticConversionFactors() {
            return _data.AllKineticConversionFactors;
        }

        public IList<KineticModelInstance> GetAllPbkModels(string dataFolderName = null) {
            return _data.AllKineticModelInstances;
        }

        public IList<MarketShare> GetAllMarketShares() {
            return _data.AllMarketShares;
        }

        public IList<ConcentrationLimit> GetAllMaximumConcentrationLimits() {
            return _data.AllMaximumConcentrationLimits;
        }

        public IDictionary<string, MolecularDockingModel> GetAllMolecularDockingModels() {
            return _data.AllMolecularDockingModels;
        }

        public IDictionary<string, Population> GetAllPopulations() {
            return _data.AllPopulations;
        }

        public IList<ProcessingFactor> GetAllProcessingFactors() {
            return _data.AllProcessingFactors;
        }

        public IDictionary<string, ProcessingType> GetAllProcessingTypes() {
            return _data.AllProcessingTypes;
        }

        public IDictionary<string, QsarMembershipModel> GetAllQsarMembershipModels() {
            return _data.AllQsarMembershipModels;
        }

        public IDictionary<string, List<RelativePotencyFactor>> GetAllRelativePotencyFactors() {
            return _data.AllRelativePotencyFactors;
        }

        public IList<SubstanceConversion> GetAllSubstanceConversions() {
            return _data.AllSubstanceConversions;
        }

        public IList<DeterministicSubstanceConversionFactor> GetAllDeterministicSubstanceConversionFactors() {
            return _data.AllDeterministicSubstanceConversionFactors;
        }

        public IDictionary<string, Response> GetAllResponses() {
            return _data.AllResponses;
        }

        public IList<TDSFoodSampleComposition> GetAllTDSFoodSampleCompositions() {
            return _data.AllTDSFoodSampleCompositions;
        }

        public IDictionary<string, TestSystem> GetAllTestSystems() {
            return _data.AllTestSystems;
        }

        public IList<UnitVariabilityFactor> GetAllUnitVariabilityFactors() {
            return _data.AllUnitVariabilityFactors;
        }

        public IList<IestiSpecialCase> GetAllIestiSpecialCases() {
            return _data.AllIestiSpecialCases;
        }

        public IList<NonDietaryExposureSet> GetAllNonDietaryExposureSets() {
            return _data.NonDietaryExposureSets;
        }

        public IDictionary<string, DietaryExposureModel> GetAllDietaryExposureModels() {
            return _data.AllDietaryExposureModels;
        }

        public IDictionary<string, TargetExposureModel> GetAllTargetExposureModels() {
            return _data.AllTargetExposureModels;
        }

        public IDictionary<string, RiskModel> GetAllRiskModels() {
            return _data.AllRiskModels;
        }

        public IDictionary<string, AnalyticalMethod> GetAllFocalFoodAnalyticalMethods() {
            return _data.AllFocalFoodAnalyticalMethods;
        }

        public IDictionary<string, FoodSample> GetAllFocalFoodSamples() {
            return _data.AllFocalFoodSamples;
        }

        public IList<PopulationConsumptionSingleValue> GetAllPopulationConsumptionSingleValues() {
            return _data.AllPopulationConsumptionSingleValues;
        }

        public IList<ConcentrationSingleValue> GetAllConcentrationSingleValues() {
            return _data.AllConcentrationSingleValues;
        }

        public ICollection<OccurrenceFrequency> GetAllOccurrenceFrequencies() {
            return _data.AllOccurrenceFrequencies;
        }

        public IDictionary<string, List<string>> GetAllSampleProperties() {
            throw new NotImplementedException();
        }

        public ICollection<int> GetAllSampleYears() {
            throw new NotImplementedException();
        }

        public ICollection<string> GetAllSampleProductionMethods() {
            throw new NotImplementedException();
        }

        public ICollection<string> GetAllSampleLocations() {
            throw new NotImplementedException();
        }

        public ICollection<string> GetAllSampleRegions() {
            throw new NotImplementedException();
        }

        public IDictionary<string, SampleProperty> GetAllAdditionalSampleProperties() {
            throw new NotImplementedException();
        }

        public IDictionary<string, SampleProperty> GetAllFocalSampleProperties() {
            throw new NotImplementedException();
        }

        public IDictionary<string, FoodSample> GetAllFoodSamples() {
            return _data.AllFoodSamples;
        }

        public IDictionary<string, IndividualProperty> GetAllHumanMonitoringIndividualProperties() {
            return _data.AllHumanMonitoringIndividualProperties;
        }

        public IDictionary<string, NonDietaryExposureSource> GetAllNonDietaryExposureSources() {
            return _data.AllNonDietaryExposureSources;
        }

        public ICollection<ExposureBiomarkerConversion> GetAllExposureBiomarkerConversions() {
            return _data.AllExposureBiomarkerConversions;
        }

        public IDictionary<string, ExposureScenario> GetAllSingleValuNonDietaryExposureScenarios() {
            return _data.AllSingleValueNonDietaryExposureScenarios;
        }

        public IDictionary<string, ExposureDeterminantCombination> GetAllSingleValueNonDietaryExposureDeterminantCombinations() {
            return _data.AllSingleValueNonDietaryExposureDeterminantCombinations;
        }

        public IList<ExposureEstimate> GetAllSingleValueNonDietaryExposures() {
            return _data.AllSingleValueNonDietaryExposureEstimates;
        }

        public IList<SubstanceConcentration> GetAllDustConcentrations() {
            return _data.AllDustConcentrations;
        }

        public IList<DustConcentrationDistribution> GetAllDustConcentrationDistributions() {
            return _data.AllDustConcentrationDistributions;
        }

        public IList<DustIngestion> GetAllDustIngestions() {
            return _data.AllDustIngestions;
        }

        public IList<DustBodyExposureFraction> GetAllDustBodyExposureFractions() {
            return _data.AllDustBodyExposureFractions;
        }

        public IList<DustAdherenceAmount> GetAllDustAdherenceAmounts() {
            return _data.AllDustAdherenceAmounts;
        }
        public IList<DustAvailabilityFraction> GetAllDustAvailabilityFractions() {
            return _data.AllDustAvailabilityFractions;
        }

        public IList<ExposureResponseFunction> GetAllExposureResponseFunctions() {
            return _data.AllExposureResponseFunctions;
        }

        IDictionary<string, PbkModelDefinition> ICompiledDataManager.GetAllPbkModelDefinitions(string dataFilePath) {
            return _data.AllPbkModelDefinitions;
        }
        public IList<SubstanceConcentration> GetAllSoilConcentrations() {
            return _data.AllSoilConcentrations;
        }

        public IList<SoilIngestion> GetAllSoilIngestions() {
            return _data.AllSoilIngestions;
        }
        public IList<BurdenOfDisease> GetAllBurdensOfDisease() {
            return _data.AllBurdensOfDisease;
        }
        public IList<BodIndicatorConversion> GetAllBodIndicatorConversions() {
            return _data.AllBodIndicatorConversions;
        }
        public IList<AirConcentration> GetAllIndoorAirConcentrations() {
            return _data.AllIndoorAirConcentrations;
        }
        public IList<AirConcentration> GetAllOutdoorAirConcentrations() {
            return _data.AllOutdoorAirConcentrations;
        }
        public IList<AirIndoorFraction> GetAllAirIndoorFractions() {
            return _data.AllAirIndoorFractions;
        }
        public IList<AirVentilatoryFlowRate> GetAllAirVentilatoryFlowRates() {
            return _data.AllAirVentilatoryFlowRates;
        }

        public IDictionary<string, ConsumerProduct> GetAllConsumerProducts() {
            return _data.AllConsumerProducts;
        }
        public IDictionary<string, IndividualProperty> GetAllConsumerProductIndividualProperties() {
            return _data.AllConsumerProductIndividualProperties;
        }
        public IList<IndividualConsumerProductUseFrequency> GetAllIndividualConsumerProductUseFrequencies() {
            return _data.AllIndividualConsumerProductUseFrequencies;
        }
        public IDictionary<string, ConsumerProductSurvey> GetAllConsumerProductSurveys() {
            return _data.AllConsumerProductSurveys;
        }
        public IDictionary<string, Individual> GetAllConsumerProductIndividuals() {
            return _data.AllConsumerProductIndividuals;
        }
        public IList<ConsumerProductConcentration> GetAllConsumerProductConcentrations() {
            return _data.AllConsumerProductConcentrations;
        }
        public IList<ConsumerProductExposureFraction> GetAllConsumerProductExposureFractions() {
            return _data.AllConsumerProductExposureFractions;
        }
        public IList<ConsumerProductApplicationAmount> GetAllConsumerProductApplicationAmounts() {
            return _data.AllConsumerProductApplicationAmounts;
        }
        public IList<ConsumerProductConcentrationDistribution> GetAllConsumerProductConcentrationDistributions() {
            return _data.AllConsumerProductConcentrationDistributions;
        }
        public ICollection<HbmSingleValueExposureSet> GetAllHbmSingleValueExposures() {
            return _data.AllHbmSingleValueExposureSets;
        }
        public IDictionary<string, IndividualSet> GetAllIndividualSets() {
            return _data.AllIndividualSets;
        }
        public IDictionary<string, Individual> GetAllIndividualSetIndividuals() {
            return _data.AllIndividualSetIndividuals;
        }
        public IDictionary<string, IndividualProperty> GetAllIndividualSetIndividualProperties() {
            return _data.AllIndividualSetIndividualProperties;
        }
        public IDictionary<string, OccupationalTask> GetAllOccupationalTasks() {
            return _data.AllOccupationalTasks;
        }
        public IDictionary<string, OccupationalScenario> GetAllOccupationalScenarios() {
            return _data.AllOccupationalScenarios;
        }
        public ICollection<OccupationalTaskExposure> GetAllOccupationalTaskExposures() {
            return _data.AllOccupationalTaskExposures;
        }
    }
}
