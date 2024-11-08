﻿using System.Data;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Management {
    public class SubsetManager {

        private ICompiledDataManager _dataManager;
        private List<Food> _allFoodsAsEaten;
        private ICollection<Compound> _allCompounds;
        private IDictionary<string, IndividualProperty> _allIndividualProperties;
        private List<FoodSample> _selectedFoodSamples;
        private HashSet<ConcentrationLimit> _allMaximumConcentrationLimits;

        /// <summary>
        /// Constructor
        /// </summary>
        public SubsetManager(ICompiledDataManager dataManager, ProjectDto project) {
            _dataManager = dataManager;
            Project = project;
        }

        /// <summary>
        /// This manager's project instance
        /// </summary>
        public ProjectDto Project { get; }

        /// <summary>
        /// Write the data of this instance to a zipped CSV file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>true if succeeded</returns>
        public bool WriteToZippedCsvFile(string filename) {
            return _dataManager.WriteToZippedCsvFile(filename);
        }

        /// <summary>
        /// Gets all foods of the compiled data source.
        /// </summary>
        public ICollection<Food> AllFoods {
            get {
                return _dataManager.GetAllFoods().Values.ToHashSet();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, Food> AllFoodsByCode {
            get {
                return _dataManager.GetAllFoods();
            }
        }

        /// <summary>
        /// Gets all foods of the compiled data source.
        /// </summary>
        public ICollection<NonDietaryExposureSource> AllNonDietaryExposureSources {
            get {
                return _dataManager.GetAllNonDietaryExposureSources().Values.ToHashSet();
            }
        }

        /// <summary>
        /// Get all compounds.
        /// </summary>
        public ICollection<Compound> AllCompounds {
            get {
                if (_allCompounds == null) {
                    _allCompounds = _dataManager.GetAllCompounds().Values.ToHashSet();
                }
                return _allCompounds;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, Compound> AllCompoundsByCode {
            get {
                return _dataManager.GetAllCompounds();
            }
        }

        /// <summary>
        /// The selected reference substance.
        /// </summary>
        public Compound ReferenceSubstance {
            get {
                var codeReferenceSubstance = Project.SubstancesSettings.CodeReferenceSubstance;
                return AllCompounds?.FirstOrDefault(c => c.Code.Equals(codeReferenceSubstance, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Gets all effects.
        /// </summary>
        public IDictionary<string, Effect> AllEffects {
            get {
                return _dataManager.GetAllEffects();
            }
        }

        /// <summary>
        /// The selected effect.
        /// </summary>
        public Effect SelectedEffect {
            get {
                var config = Project.EffectsSettings;
                if (config.MultipleEffects) {
                    return null;
                } else if (config.IncludeAopNetwork) {
                    AllEffects.TryGetValue(config.CodeFocalEffect, out var effect);
                    return effect;
                } else if (AllEffects.Values.Count == 1) {
                    return AllEffects.Values.SingleOrDefault();
                }
                return null;
            }
        }

        /// <summary>
        /// Gets all food surveys of the compiled data source.
        /// </summary>
        public IDictionary<string, FoodSurvey> AllFoodSurveys {
            get {
                return _dataManager.GetAllFoodSurveys();
            }
        }

        /// <summary>
        /// The selected food survey.
        /// </summary>
        public FoodSurvey SelectedFoodSurvey {
            get {
                if (AllFoodSurveys != null && AllFoodSurveys.Count == 1) {
                    return AllFoodSurveys.First().Value;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets all individuals of the compiled data source.
        /// </summary>
        public IDictionary<string, Individual> AllIndividuals {
            get {
                return _dataManager.GetAllIndividuals();
            }
        }

        /// <summary>
        /// Returns the all the individual properties present in the data source.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IndividualProperty> AllIndividualProperties {
            get {
                if (_allIndividualProperties == null) {
                    _allIndividualProperties = _dataManager.GetAllIndividualProperties();
                }
                return _allIndividualProperties;
            }
        }

        /// <summary>
        /// Returns the all the individual properties present in the data source.
        /// 
        /// DIT moet vervane
        /// </summary>
        /// <returns></returns>
        public List<IndividualProperty> CovariableIndividualProperties {
            get {
                return AllIndividualProperties.Values
                    .Where(ip => ip.PropertyType.GetPropertyType() == PropertyType.Covariable).ToList();
            }
        }

        /// <summary>
        /// Returns the selected covariable property if this selection is made, otherwise null.
        /// </summary>
        public IndividualProperty CovariableIndividualProperty {
            get {
                var config = Project.DietaryExposuresSettings;
                if (!string.IsNullOrEmpty(config.NameCovariable)
                    && AllIndividualProperties.TryGetValue(config.NameCovariable, out var property)
                ) {
                    return property;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the selected cofactor if this selection is made in the covariate selection settings, otherwise null.
        /// </summary>
        public IndividualProperty CofactorIndividualProperty {
            get {
                var config = Project.DietaryExposuresSettings;
                if (!string.IsNullOrEmpty(config.NameCofactor)
                    && AllIndividualProperties.TryGetValue(config.NameCofactor, out var property)
                ) {
                    return property;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets all food consumptions of the compiled data source.
        /// </summary>
        public ICollection<FoodConsumption> AllFoodConsumptions {
            get {
                return _dataManager.GetAllFoodConsumptions();
            }
        }

        public ICollection<PopulationConsumptionSingleValue> AllPopulationConsumptionSingleValues {
            get {
                return _dataManager.GetAllPopulationConsumptionSingleValues();
            }
        }

        /// <summary>
        /// All foods as eaten of all consumptions in the selected food consumptions.
        /// </summary>
        public ICollection<Food> AllConsumedFoods {
            get {
                if (_allFoodsAsEaten == null) {
                    _allFoodsAsEaten = AllFoodConsumptions?
                        .Select(fc => fc.Food).Distinct().ToList();
                }
                return _allFoodsAsEaten;
            }
        }

        /// <summary>
        /// Gets all sample properties of the compiled data source.
        /// </summary>
        public IDictionary<string, SampleProperty> AllAdditionalSampleProperties {
            get {
                return _dataManager.GetAllAdditionalSampleProperties();
            }
        }

        /// <summary>
        /// Gets all sample years of the compiled data source.
        /// </summary>
        public ICollection<int> AllSampleYears {
            get {
                return _dataManager.GetAllSampleYears();
            }
        }

        /// <summary>
        /// Gets all sample locations of the compiled data source.
        /// </summary>
        public ICollection<string> AllSampleLocations {
            get {
                return _dataManager.GetAllSampleLocations();
            }
        }

        /// <summary>
        /// Gets all sample regions of the compiled data source.
        /// </summary>
        public ICollection<string> AllSampleRegions {
            get {
                return _dataManager.GetAllSampleRegions();
            }
        }

        /// <summary>
        /// Gets all sample production methods of the compiled data source.
        /// </summary>
        public ICollection<string> AllSampleProductionMethods {
            get {
                return _dataManager.GetAllSampleProductionMethods();
            }
        }

        /// <summary>
        /// Returns all focal commodity samples.
        /// </summary>
        public ICollection<FoodSample> AllFocalCommoditySamples {
            get {
                return _dataManager.GetAllFocalFoodSamples().Values;
            }
        }

        /// <summary>
        /// Returns the selected focal commodity samples.
        /// </summary>
        public ICollection<FoodSample> SelectedFocalCommoditySamples {
            get {
                return AllFocalCommoditySamples?
                    .Where(r => SelectedFocalCommodityFoods?.Contains(r.Food) ?? false)
                    .ToList();
            }
        }

        /// <summary>
        /// Returns all food samples that are selected using:
        /// - Selected foods (as measured)
        /// </summary>
        public List<FoodSample> SelectedFoodSamples {
            get {
                if (_selectedFoodSamples == null) {
                    var allFoodSamples = _dataManager.GetAllFoodSamples().Values;
                    IEnumerable<FoodSample> selectedFoodSamples = allFoodSamples;
                    // Filter by selected food-as-measured
                    var config = Project.ModelledFoodsSettings;
                    if (config.RestrictToModelledFoodSubset && config.ModelledFoodSubset.Any()) {
                        var allFoods = _dataManager.GetAllFoods();
                        var foodsAsMeasuredSubset = config.ModelledFoodSubset.Select(f => allFoods[f]).ToHashSet();
                        selectedFoodSamples = selectedFoodSamples.Where(s => foodsAsMeasuredSubset.Contains(s.Food));
                    }
                    _selectedFoodSamples = selectedFoodSamples.ToList();
                }
                return _selectedFoodSamples;
            }
        }


        /// <summary>
        /// Gets all analytical methods of the compiled data source.
        /// </summary>
        public IDictionary<string, AnalyticalMethod> AllAnalyticalMethods {
            get {
                return _dataManager.GetAllAnalyticalMethods();
            }
        }

        private HashSet<Food> _allModelledFoods;

        /// <summary>
        /// All foods that are marked as modelled foods by the list of food conversions.
        /// </summary>
        public HashSet<Food> AllModelledFoods {
            get {
                if (_allModelledFoods == null) {
                    var result = new HashSet<Food>();
                    var config = Project.ModelledFoodsSettings;
                    if (config.DeriveModelledFoodsFromSampleBasedConcentrations) {
                        result.UnionWith(_dataManager.GetAllFoodSamples().Values.Select(s => s.Food).Distinct());
                    }
                    if (config.DeriveModelledFoodsFromSingleValueConcentrations) {
                        result.UnionWith(_dataManager.GetAllConcentrationSingleValues().Select(s => s.Food).Distinct());
                    }
                    if (config.UseWorstCaseValues) {
                        result.UnionWith(_dataManager.GetAllMaximumConcentrationLimits().Select(s => s.Food).Distinct());
                    }
                    _allModelledFoods = result;
                }
                return _allModelledFoods;
            }
        }

        /// <summary>
        /// Returns the project's selected focal commodity foods.
        /// </summary>
        public ICollection<Food> SelectedFocalCommodityFoods {
            get {
                var selectedFoods = Project.ConcentrationsSettings.FocalFoods
                    .Where(r => !string.IsNullOrEmpty(r.CodeFood) && AllFoodsByCode.ContainsKey(r.CodeFood))
                    .Select(r => AllFoodsByCode[r.CodeFood])
                    .ToHashSet();
                return selectedFoods;
            }
        }

        /// <summary>
        /// Returns the project's selected focal commodity substances.
        /// </summary>
        public ICollection<Compound> SelectedFocalCommoditySubstances {
            get {
                var selectedSubstances = Project.ConcentrationsSettings.FocalFoods
                    .Where(r => !string.IsNullOrEmpty(r.CodeSubstance) && AllCompoundsByCode.ContainsKey(r.CodeSubstance))
                    .Select(cf => AllCompoundsByCode[cf.CodeSubstance])
                    .ToHashSet();
                return selectedSubstances;
            }
        }

        /// <summary>
        /// Returns all maximum residue limits.
        /// </summary>
        public ICollection<ConcentrationLimit> AllMaximumConcentrationLimits {
            get {
                if (_allMaximumConcentrationLimits == null) {
                    _allMaximumConcentrationLimits = _dataManager.GetAllMaximumConcentrationLimits().ToHashSet();
                }
                return _allMaximumConcentrationLimits;
            }
        }

        /// <summary>
        /// Returns all residue definitions.
        /// </summary>
        public IList<SubstanceConversion> AllSubstanceConversions {
            get {
                return _dataManager.GetAllSubstanceConversions();
            }
        }

        /// <summary>
        /// Gets all processing types.
        /// </summary>
        public ICollection<ProcessingType> AllProcessingTypes {
            get {
                return _dataManager.GetAllProcessingTypes().Values.ToHashSet();
            }
        }

        /// <summary>
        /// Returns all non-compound-specific processing factors.
        /// </summary>
        public ICollection<ProcessingFactor> AllProcessingFactors {
            get {
                return _dataManager.GetAllProcessingFactors();
            }
        }

        /// <summary>
        /// Gets all agricultural uses.
        /// </summary>
        public ICollection<OccurrencePattern> AllOccurrencePatterns {
            get {
                return _dataManager.GetAllOccurrencePatterns();
            }
        }

        /// <summary>
        /// Gets all occurrence frequencies.
        /// </summary>
        public ICollection<OccurrenceFrequency> AllOccurrenceFrequencies {
            get {
                return _dataManager.GetAllOccurrenceFrequencies();
            }
        }

        /// <summary>
        /// Gets all authorised uses.
        /// </summary>
        public ICollection<SubstanceAuthorisation> AllSubstanceAuthorisations {
            get {
                return _dataManager.GetAllSubstanceAuthorisations();
            }
        }

        /// <summary>
        /// Gets all substance approvals.
        /// </summary>
        public ICollection<SubstanceApproval> AllSubstanceApprovals {
            get {
                return _dataManager.GetAllSubstanceApprovals();
            }
        }

        /// <summary>
        /// Returns all relative potency factors of the compiled datasource.
        /// </summary>
        public IDictionary<string, List<RelativePotencyFactor>> AllRelativePotencyFactors {
            get {
                return _dataManager.GetAllRelativePotencyFactors();
            }
        }

        /// <summary>
        /// Returns all hazard doses of the compiled datasource.
        /// </summary>
        public ICollection<Compiled.Objects.PointOfDeparture> AllPointsOfDeparture {
            get {
                return _dataManager.GetAllPointsOfDeparture();
            }
        }

        /// <summary>
        /// Returns all hazard characterisations.
        /// </summary>
        public ICollection<HazardCharacterisation> AllHazardCharacterisations {
            get {
                return _dataManager.GetAllHazardCharacterisations();
            }
        }

        public ICollection<ActiveSubstanceModel> AllActiveSubstances {
            get {
                return _dataManager.GetAllActiveSubstanceModels().Values.ToList();
            }
        }

        public ICollection<MolecularDockingModel> AllMolecularDockingModels {
            get {
                return _dataManager.GetAllMolecularDockingModels().Values.ToList();
            }
        }

        public ICollection<QsarMembershipModel> AllQsarMembershipModels {
            get {
                return _dataManager.GetAllQsarMembershipModels().Values.ToList();
            }
        }

        /// <summary>
        /// Gets all human monitoring surveys.
        /// </summary>
        public ICollection<HumanMonitoringSurvey> AllHumanMonitoringSurveys {
            get {
                return _dataManager.GetAllHumanMonitoringSurveys().Values.ToList();
            }
        }

        /// <summary>
        /// Gets all human monitoring individuals.
        /// </summary>
        public ICollection<Individual> AllHumanMonitoringIndividuals {
            get {
                return _dataManager.GetAllHumanMonitoringIndividuals().Values.ToList();
            }
        }

        /// <summary>
        /// Returns the all the individual properties present in the data source.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IndividualProperty> AllHumanMonitoringIndividualProperties {
            get {
                return _dataManager.GetAllHumanMonitoringIndividualProperties();
            }
        }

        /// <summary>
        /// Returns all human monitoring samples.
        /// </summary>
        public ICollection<HumanMonitoringSample> AllHumanMonitoringSamples {
            get {
                return _dataManager.GetAllHumanMonitoringSamples().Values.ToList();
            }
        }

        /// <summary>
        /// Returns all human monitoring analytical methods.
        /// </summary>
        public IDictionary<string, AnalyticalMethod> AllHumanMonitoringAnalyticalMethods {
            get {
                return _dataManager.GetAllHumanMonitoringAnalyticalMethods();
            }
        }

        /// <summary>
        /// Gets all human monitoring endpoints of all surveys.
        /// </summary>
        /// <returns></returns>
        public ICollection<HumanMonitoringSamplingMethod> GetAllHumanMonitoringSamplingMethods() {
            return _dataManager.GetAllHumanMonitoringSamplingMethods();
        }

        /// <summary>
        /// Gets all populations.
        /// </summary>
        public ICollection<Population> AllPopulations {
            get {
                return _dataManager.GetAllPopulations().Values.ToList();
            }
        }

        /// <summary>
        /// Gets all inter-species factors.
        /// </summary>
        public ICollection<InterSpeciesFactor> AllInterSpeciesFactors {
            get {
                return _dataManager.GetAllInterSpeciesFactors();
            }
        }

        /// <summary>
        /// Gets all intra-species factors.
        /// </summary>
        public ICollection<IntraSpeciesFactor> AllIntraSpeciesFactors {
            get {
                return _dataManager.GetAllIntraSpeciesFactors();
            }
        }

        public IList<ConcentrationDistribution> AllConcentrationDistributions {
            get {
                return _dataManager.GetAllConcentrationDistributions();
            }
        }

        public List<Food> AvailableScenarioAnalysisFoods {
            get {
                return AllConcentrationDistributions
                    .Where(s => s.Limit != null && s.Percentile != null && s.Limit / s.Percentile < 1)
                    .Select(cd => cd.Food).Distinct().ToList();
            }
        }

        /// <summary>
        /// Returns all focal commodity foods.
        /// </summary>
        public HashSet<Food> AllFocalCommodityFoods {
            get {
                return _dataManager.GetAllFocalCommodityFoods().Values.ToHashSet();
            }
        }

        /// <summary>
        /// Returns all analytical methods of the focal commodity samples.
        /// </summary>
        public IDictionary<string, AnalyticalMethod> AllFocalCommodityAnalyticalMethods {
            get {
                return _dataManager.GetAllFocalFoodAnalyticalMethods();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<NonDietaryExposureSet> NonDietaryExposureSets {
            get {
                return _dataManager.GetAllNonDietaryExposureSets().ToList();
            }
        }

        /// <summary>
        /// Returns all available dietary exposure models.
        /// </summary>
        public IDictionary<string, DietaryExposureModel> AllDietaryExposureModels {
            get {
                return _dataManager.GetAllDietaryExposureModels();
            }
        }

        /// <summary>
        /// Returns all available target exposure models.
        /// </summary>
        public IDictionary<string, TargetExposureModel> AllTargetExposureModels {
            get {
                return _dataManager.GetAllTargetExposureModels();
            }
        }

        /// <summary>
        /// Returns all available risk models.
        /// </summary>
        public IDictionary<string, RiskModel> AllRiskModels {
            get {
                return _dataManager.GetAllRiskModels();
            }
        }
        /// <summary>
        /// Returns all unit variability factors.
        /// </summary>
        /// <returns></returns>
        public ICollection<UnitVariabilityFactor> GetAllUnitVariabilityFactors() {
            return _dataManager.GetAllUnitVariabilityFactors();
        }

        /// <summary>
        /// Returns all post harvest applications.
        /// </summary>
        /// <returns></returns>
        public ICollection<IestiSpecialCase> GetAllIestiSpecialCases() {
            return _dataManager.GetAllIestiSpecialCases();
        }

        /// <summary>
        /// Checks if there are unit variability factors specified in the form of coefficients.
        /// </summary>
        public bool UnitVariabilityDataHasCoefficients {
            get {
                return _dataManager.GetAllUnitVariabilityFactors().Any(uv => uv.Coefficient.HasValue);
            }
        }

        /// <summary>
        /// Checks if there are unit variability factors specified in the form of factors.
        /// </summary>
        public bool UnitVariabilityDataHasFactors {
            get {
                return _dataManager.GetAllUnitVariabilityFactors().Any(uv => uv.Factor.HasValue);
            }
        }

        /// <summary>
        /// Returns all responses.
        /// </summary>
        public IDictionary<string, Response> AllResponses {
            get {
                return _dataManager.GetAllResponses();
            }
        }

        /// <summary>
        /// Returns the selected responses.
        /// </summary>
        public IDictionary<string, Response> SelectedResponses {
            get {
                var codes = Project.GetFilterCodes(ScopingType.Responses);

                if (codes?.Any() ?? false) {
                    return AllResponses
                        .Where(r => codes.Contains(r.Key))
                        .ToDictionary(r => r.Key, r => r.Value);
                }

                return new Dictionary<string, Response>();
            }
        }

        /// <summary>
        /// Returns all effect representations.
        /// </summary>
        public ILookup<Effect, EffectRepresentation> AllEffectRepresentations {
            get {
                return _dataManager.GetAllEffectRepresentations().ToLookup(r => r.Effect);
            }
        }

        /// <summary>
        /// Gets all dose response experiments from the compiled data source.
        /// </summary>
        public List<DoseResponseExperiment> AllDoseResponseExperiments {
            get {
                return _dataManager.GetAllDoseResponseExperiments().Values.ToList();
            }
        }

        /// <summary>
        /// Returns all test systems.
        /// </summary>
        public List<TestSystem> AllTestSystems {
            get {
                return _dataManager.GetAllTestSystems().Values.ToList();
            }
        }

        /// <summary>
        /// Returns all dose response models.
        /// </summary>
        public List<DoseResponseModel> AllDoseResponseModels {
            get {
                return _dataManager.GetAllDoseResponseModels().ToList();
            }
        }

        /// <summary>
        /// Returns all available adverse outcome pathway networks.
        /// </summary>
        public IDictionary<string, AdverseOutcomePathwayNetwork> AllAdverseOutcomePathwayNetworks {
            get {
                return _dataManager.GetAdverseOutcomePathwayNetworks();
            }
        }

        /// <summary>
        /// Gets all PBK models of the compiled data source.
        /// </summary>
        public List<KineticModelInstance> AllPbkModels {
            get {
                return _dataManager.GetAllPbkModels()?.ToList();
            }
        }

        /// <summary>
        /// Gets all absorption factors
        /// </summary>
        public ICollection<SimpleAbsorptionFactor> AllAbsorptionFactors {
            get {
                return _dataManager.GetAllAbsorptionFactors()?.ToList();
            }
        }

        /// <summary>
        /// Gets all kinetic model conversion factors
        /// </summary>
        public ICollection<KineticConversionFactor> AllKineticConversionFactors {
            get {
                return _dataManager.GetAllKineticConversionFactors()?.ToList();
            }
        }

        public List<FoodTranslation> AllFoodTranslations {
            get {
                return _dataManager.GetAllFoodTranslations().ToList();
            }
        }

        public List<TDSFoodSampleComposition> AllTDSFoodSampleCompositions {
            get {
                return _dataManager.GetAllTDSFoodSampleCompositions().ToList();
            }
        }

        public IDictionary<Food, ICollection<Food>> AllFoodExtrapolations {
            get {
                return _dataManager.GetAllFoodExtrapolations();
            }
        }

        public List<MarketShare> AllMarketShares {
            get {
                return _dataManager.GetAllMarketShares().ToList();
            }
        }


        public ICollection<DeterministicSubstanceConversionFactor> AllDeterministicSubstanceConversionFactors {
            get {
                return _dataManager.GetAllDeterministicSubstanceConversionFactors();
            }
        }

        public ICollection<ConcentrationSingleValue> AllConcentrationSingleValues {
            get {
                return _dataManager.GetAllConcentrationSingleValues();
            }
        }

        public ICollection<ExposureBiomarkerConversion> AllExposureBiomarkerConversions {
            get {
                return _dataManager.GetAllExposureBiomarkerConversions();
            }
        }

        public IDictionary<string, ExposureScenario> AllSingleValueNonDietaryExposureScenarios {
            get {
                return _dataManager.GetAllSingleValuNonDietaryExposureScenarios();
            }
        }

        public IDictionary<string, ExposureDeterminantCombination> AllSingleValueNonDietaryExposureDeterminantCombinations {
            get {
                return _dataManager.GetAllSingleValueNonDietaryExposureDeterminantCombinations();
            }
        }

        public IList<ExposureEstimate> AllSingleValueNonDietaryExposures {
            get {
                return _dataManager.GetAllSingleValueNonDietaryExposures();
            }
        }

        public IList<DustConcentrationDistribution> AllDustConcentrationDistributions {
            get {
                return _dataManager.GetAllDustConcentrationDistributions();
            }
        }

        public IList<DustIngestion> AllDustIngestions {
            get {
                return _dataManager.GetAllDustIngestions();
            }
        }

        public IList<DustBodyExposureFraction> AllDustBodyExposureFractions {
            get {
                return _dataManager.GetAllDustBodyExposureFractions();
            }
        }

        public IList<DustAdherenceAmount> AllDustAdherenceAmounts {
            get {
                return _dataManager.GetAllDustAdherenceAmounts();
            }
        }

        public IList<DustAvailabilityFraction> AllDustAvailabilityFractions {
            get {
                return _dataManager.GetAllDustAvailabilityFractions();
            }
        }

        public IList<ExposureEffectFunction> AllExposureEffectFunctions {
            get {
                return _dataManager.GetAllExposureEffectFunctions();
            }
        }
    }
}
