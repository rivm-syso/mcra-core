using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.RawDataProviders;
using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General.Action.Settings;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledTestsBase {

        private CsvRawDataProvider _rawDataProvider;
        private CompiledDataManager _compiledDataManager;
        private SubsetManager _subsetManager;
        private ProjectDto _project;

        protected CsvRawDataProvider RawDataProvider => _rawDataProvider;
        protected CompiledDataManager CompiledDataManager => _compiledDataManager;
        protected SubsetManager SubsetManager => _subsetManager ??= new(CompiledDataManager, Project);
        protected ProjectDto Project => _project ??= new();

        [TestInitialize]
        public virtual void TestInitialize() {
            _rawDataProvider = new CsvRawDataProvider(@"Resources/Csv/");
            _compiledDataManager = new CompiledDataManager(_rawDataProvider);
            _subsetManager = null;
        }

        protected void ResetSubsetManager() {
            _subsetManager = new(CompiledDataManager, Project);
        }

        #region Data manager functions
        protected IDictionary<string, FoodSurvey> GetAllFoodSurveys(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllFoodSurveys(),
                ManagerType.SubsetManager => SubsetManager.AllFoodSurveys,
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, Individual> GetAllIndividuals(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllIndividuals(),
                ManagerType.SubsetManager => SubsetManager.AllIndividuals,
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, Food> GetAllFoods(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllFoods(),
                ManagerType.SubsetManager => SubsetManager.AllFoodsByCode,
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, Compound> GetAllCompounds(ManagerType managerType, bool useDictionary = false) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllCompounds(),
                ManagerType.SubsetManager => useDictionary
                    ? SubsetManager.AllCompounds.ToDictionary(c => c.Code, StringComparer.OrdinalIgnoreCase)
                    : SubsetManager.AllCompoundsByCode,
                _ => throw new NotImplementedException()
            };

        protected IList<FoodConsumption> GetAllFoodConsumptions(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllFoodConsumptions(),
                ManagerType.SubsetManager => [.. SubsetManager.AllFoodConsumptions],
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, ActiveSubstanceModel> GetAllActiveSubstanceModels(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllActiveSubstanceModels(),
                ManagerType.SubsetManager => SubsetManager.AllActiveSubstances.ToDictionary(m => m.Code, StringComparer.OrdinalIgnoreCase),
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, AdverseOutcomePathwayNetwork> GetAllAdverseOutcomePathwayNetworks(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAdverseOutcomePathwayNetworks(),
                ManagerType.SubsetManager => SubsetManager.AllAdverseOutcomePathwayNetworks,
                _ => throw new NotImplementedException()
            };

        protected ICollection<ConcentrationDistribution> GetAllConcentrationDistributions(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllConcentrationDistributions(),
                ManagerType.SubsetManager => SubsetManager.AllConcentrationDistributions,
                _ => throw new NotImplementedException()
            };

        protected ICollection<ConcentrationLimit> GetAllMaximumConcentrationLimits(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllMaximumConcentrationLimits(),
                ManagerType.SubsetManager => SubsetManager.AllMaximumConcentrationLimits,
                _ => throw new NotImplementedException()
            };


        protected IDictionary<string, AnalyticalMethod> GetAllAnalyticalMethods(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllAnalyticalMethods(),
                ManagerType.SubsetManager => SubsetManager.AllAnalyticalMethods,
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, FoodSample> GetAllFoodSamples(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllFoodSamples(),
                ManagerType.SubsetManager => SubsetManager.SelectedFoodSamples.ToDictionary(s => s.Code, StringComparer.OrdinalIgnoreCase),
                _ => throw new NotImplementedException()
            };

        protected ICollection<DeterministicSubstanceConversionFactor> GetAllDeterministicSubstanceConversionFactors(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllDeterministicSubstanceConversionFactors(),
                ManagerType.SubsetManager => SubsetManager.AllDeterministicSubstanceConversionFactors,
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, DietaryExposureModel> GetAllDietaryExposureModels(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllDietaryExposureModels(),
                ManagerType.SubsetManager => SubsetManager.AllDietaryExposureModels,
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, DoseResponseExperiment> GetAllDoseResponseExperiments(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllDoseResponseExperiments(),
                ManagerType.SubsetManager => SubsetManager.AllDoseResponseExperiments.ToDictionary(e => e.Code, StringComparer.OrdinalIgnoreCase),
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, Response> GetAllResponses(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllResponses(),
                ManagerType.SubsetManager => SubsetManager.AllResponses,
                _ => throw new NotImplementedException()
            };

        protected IList<DoseResponseModel> GetAllDoseResponseModels(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllDoseResponseModels(),
                ManagerType.SubsetManager => SubsetManager.AllDoseResponseModels,
                _ => throw new NotImplementedException()
            };

        protected IList<EffectRepresentation> GetAllEffectRepresentations(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllEffectRepresentations(),
                ManagerType.SubsetManager => [.. SubsetManager.AllEffectRepresentations.SelectMany(r => r)],
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, Effect> GetAllEffects(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllEffects(),
                ManagerType.SubsetManager => SubsetManager.AllEffects,
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, Food> GetAllFocalCommodityFoods(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllFocalCommodityFoods(),
                ManagerType.SubsetManager => SubsetManager.AllFocalCommodityFoods.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase),
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, AnalyticalMethod> GetAllFocalFoodAnalyticalMethods(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllFocalFoodAnalyticalMethods(),
                ManagerType.SubsetManager => SubsetManager.AllFocalCommodityAnalyticalMethods,
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, FoodSample> GetAllFocalFoodSamples(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllFocalFoodSamples(),
                ManagerType.SubsetManager => SubsetManager.AllFocalCommoditySamples.ToDictionary(s => s.Code, StringComparer.OrdinalIgnoreCase),
                _ => throw new NotImplementedException()
            };

        protected IDictionary<Food, ICollection<Food>> GetAllFoodExtrapolations(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllFoodExtrapolations(),
                ManagerType.SubsetManager => SubsetManager.AllFoodExtrapolations,
                _ => throw new NotImplementedException()
            };

        protected IList<FoodTranslation> GetAllFoodTranslations(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllFoodTranslations(),
                ManagerType.SubsetManager => SubsetManager.AllFoodTranslations,
                _ => throw new NotImplementedException()
            };

        protected ICollection<HazardCharacterisation> GetAllHazardCharacterisations(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllHazardCharacterisations(),
                ManagerType.SubsetManager => SubsetManager.AllHazardCharacterisations,
                _ => throw new NotImplementedException()
            };


        protected IDictionary<string, AnalyticalMethod> GetAllHumanMonitoringAnalyticalMethods(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllHumanMonitoringAnalyticalMethods(),
                ManagerType.SubsetManager => SubsetManager.AllHumanMonitoringAnalyticalMethods,
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, HumanMonitoringSample> GetAllHumanMonitoringSamples(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllHumanMonitoringSamples(),
                ManagerType.SubsetManager => SubsetManager.AllHumanMonitoringSamples.ToDictionary(s => s.Code),
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, Individual> GetAllHumanMonitoringIndividuals(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllHumanMonitoringIndividuals(),
                ManagerType.SubsetManager => SubsetManager.AllHumanMonitoringIndividuals.ToDictionary(s => s.Code),
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, HumanMonitoringSurvey> GetAllHumanMonitoringSurveys(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllHumanMonitoringSurveys(),
                ManagerType.SubsetManager => SubsetManager.AllHumanMonitoringSurveys.ToDictionary(s => s.Code),
                _ => throw new NotImplementedException()
            };

        protected ICollection<InterSpeciesFactor> GetAllInterSpeciesFactors(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllInterSpeciesFactors(),
                ManagerType.SubsetManager => SubsetManager.AllInterSpeciesFactors,
                _ => throw new NotImplementedException()
            };

        protected ICollection<IntraSpeciesFactor> GetAllIntraSpeciesFactors(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllIntraSpeciesFactors(),
                ManagerType.SubsetManager => SubsetManager.AllIntraSpeciesFactors,
                _ => throw new NotImplementedException()
            };

        protected ICollection<NonDietaryExposureSet> GetAllNonDietaryExposureSets(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllNonDietaryExposureSets(),
                ManagerType.SubsetManager => SubsetManager.NonDietaryExposureSets,
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, MolecularDockingModel> GetAllMolecularDockingModels(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllMolecularDockingModels(),
                ManagerType.SubsetManager => SubsetManager.AllMolecularDockingModels.ToDictionary(m => m.Code, StringComparer.OrdinalIgnoreCase),
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, NonDietaryExposureSource> GetAllNonDietaryExposureSources(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllNonDietaryExposureSources(),
                ManagerType.SubsetManager => SubsetManager.AllNonDietaryExposureSources.ToDictionary(m => m.Code, StringComparer.OrdinalIgnoreCase),
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, OccurrencePattern> GetAllOccurrencePatterns(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllOccurrencePatterns().ToDictionary(a => a.Code),
                ManagerType.SubsetManager => SubsetManager.AllOccurrencePatterns.ToDictionary(m => m.Code, StringComparer.OrdinalIgnoreCase),
                _ => throw new NotImplementedException()
            };

        protected ICollection<KineticModelInstance> GetAllPbkModels(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllPbkModels(),
                ManagerType.SubsetManager => SubsetManager.AllPbkModels,
                _ => throw new NotImplementedException()
            };

        protected ICollection<PointOfDeparture> GetAllPointsOfDeparture(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllPointsOfDeparture(),
                ManagerType.SubsetManager => SubsetManager.AllPointsOfDeparture,
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, Population> GetAllPopulations(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllPopulations(),
                ManagerType.SubsetManager => SubsetManager.AllPopulations.ToDictionary(m => m.Code, StringComparer.OrdinalIgnoreCase),
                _ => throw new NotImplementedException()
            };

        protected ICollection<ProcessingFactor> GetAllProcessingFactors(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllProcessingFactors(),
                ManagerType.SubsetManager => [.. SubsetManager.AllProcessingFactors],
                _ => throw new NotImplementedException()
            };


        protected IDictionary<string, QsarMembershipModel> GetAllQsarMembershipModels(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllQsarMembershipModels(),
                ManagerType.SubsetManager => SubsetManager.AllQsarMembershipModels.ToDictionary(m => m.Code, StringComparer.OrdinalIgnoreCase),
                _ => throw new NotImplementedException()
            };

        protected IDictionary<string, List<RelativePotencyFactor>> GetAllRelativePotencyFactors(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllRelativePotencyFactors(),
                ManagerType.SubsetManager => SubsetManager.AllRelativePotencyFactors,
                _ => throw new NotImplementedException()
            };


        protected IDictionary<string, RiskModel> GetAllRiskModels(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllRiskModels(),
                ManagerType.SubsetManager => SubsetManager.AllRiskModels,
                _ => throw new NotImplementedException()
            };

        protected ICollection<SubstanceApproval> GetAllSubstanceApprovals(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllSubstanceApprovals(),
                ManagerType.SubsetManager => SubsetManager.AllSubstanceApprovals,
                _ => throw new NotImplementedException()
            };

        protected ICollection<SubstanceAuthorisation> GetAllSubstanceAuthorisations(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllSubstanceAuthorisations(),
                ManagerType.SubsetManager => SubsetManager.AllSubstanceAuthorisations,
                _ => throw new NotImplementedException()
            };

        protected ICollection<SubstanceConversion> GetAllSubstanceConversions(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllSubstanceConversions(),
                ManagerType.SubsetManager => [.. SubsetManager.AllSubstanceConversions],
                _ => throw new NotImplementedException()
            };


        protected IDictionary<string, TargetExposureModel> GetAllTargetExposureModels(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllTargetExposureModels(),
                ManagerType.SubsetManager => SubsetManager.AllTargetExposureModels,
                _ => throw new NotImplementedException()
            };

        protected IList<TDSFoodSampleComposition> GetAllTDSFoodSampleCompositions(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllTDSFoodSampleCompositions(),
                ManagerType.SubsetManager => SubsetManager.AllTDSFoodSampleCompositions,
                _ => throw new NotImplementedException()
            };

        protected ICollection<UnitVariabilityFactor> GetAllUnitVariabilityFactors(ManagerType managerType) =>
            managerType switch {
                ManagerType.CompiledDataManager => CompiledDataManager.GetAllUnitVariabilityFactors(),
                ManagerType.SubsetManager => SubsetManager.GetAllUnitVariabilityFactors(),
                _ => throw new NotImplementedException()
            };

        #endregion
    }
}
