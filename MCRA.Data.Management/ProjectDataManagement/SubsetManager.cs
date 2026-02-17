using System.Data;
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
        private DirectoryInfo _tempDataFolder;
        /// <summary>
        /// Constructor
        /// </summary>
        public SubsetManager(ICompiledDataManager dataManager, ProjectDto project, DirectoryInfo tempDataFolder = null) {
            _dataManager = dataManager;
            Project = project;
            if (tempDataFolder != null && tempDataFolder.Exists) {
                //create a folder for subsetmanager extra data files as a subfolder of the
                //tempDataFolder
                var subFolderName= $"sbsmgr{Guid.NewGuid().ToString()[..8]}";
                _tempDataFolder = tempDataFolder.CreateSubdirectory(subFolderName);
            }
        }

        /// <summary>
        /// TODO: implement IDisposable
        /// </summary>
        ~SubsetManager() {
            _dataManager = null;
            try {
                _tempDataFolder?.Delete(true);
            } catch { }
        }

        /// <summary>
        /// This manager's project instance
        /// </summary>
        public ProjectDto Project { get; }

        /// <summary>
        /// Gets all foods of the compiled data source.
        /// </summary>
        public ICollection<Food> AllFoods => _dataManager.GetAllFoods().Values.ToHashSet();

        /// <summary>
        /// Gets all foods of the compiled data source in a dictionary by food code
        /// </summary>
        public IDictionary<string, Food> AllFoodsByCode => _dataManager.GetAllFoods();

        /// <summary>
        /// Gets all foods of the compiled data source.
        /// </summary>
        public ICollection<NonDietaryExposureSource> AllNonDietaryExposureSources =>
            _dataManager.GetAllNonDietaryExposureSources().Values.ToHashSet();

        /// <summary>
        /// Get all compounds.
        /// </summary>
        public ICollection<Compound> AllCompounds =>
            _allCompounds ??= _dataManager.GetAllCompounds().Values.ToHashSet();

        /// <summary>
        /// Gets all substances of the compiled data source in a dictionary by substance code
        /// </summary>
        public IDictionary<string, Compound> AllCompoundsByCode => _dataManager.GetAllCompounds();

        /// <summary>
        /// The selected reference substance.
        /// </summary>
        public Compound ReferenceSubstance {
            get {
                var codeReferenceSubstance = Project.SubstancesSettings.CodeReferenceSubstance;
                if (!string.IsNullOrEmpty(codeReferenceSubstance) &&
                    AllCompoundsByCode.TryGetValue(codeReferenceSubstance, out var substance)
                ) {
                    return substance;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets all effects.
        /// </summary>
        public IDictionary<string, Effect> AllEffects => _dataManager.GetAllEffects();

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
        public IDictionary<string, FoodSurvey> AllFoodSurveys => _dataManager.GetAllFoodSurveys();

        /// <summary>
        /// The selected food survey.
        /// </summary>
        public FoodSurvey SelectedFoodSurvey =>
            AllFoodSurveys?.Count == 1 ? AllFoodSurveys.First().Value : null;

        /// <summary>
        /// Gets all individuals of the compiled data source.
        /// </summary>
        public IDictionary<string, Individual> AllIndividuals => _dataManager.GetAllIndividuals();

        /// <summary>
        /// Returns the all the individual properties present in the data source.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IndividualProperty> AllIndividualProperties =>
            _allIndividualProperties ??= _dataManager.GetAllIndividualProperties();

        /// <summary>
        /// Returns the all the individual properties present in the data source.
        ///
        /// DIT moet vervane
        /// </summary>
        /// <returns></returns>
        public List<IndividualProperty> CovariableIndividualProperties =>
            [.. AllIndividualProperties.Values.Where(ip => ip.PropertyType.GetPropertyType() == PropertyType.Covariable)];

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
        public ICollection<FoodConsumption> AllFoodConsumptions => _dataManager.GetAllFoodConsumptions();

        public ICollection<PopulationConsumptionSingleValue> AllPopulationConsumptionSingleValues =>
            _dataManager.GetAllPopulationConsumptionSingleValues();

        /// <summary>
        /// All foods as eaten of all consumptions in the selected food consumptions.
        /// </summary>
        public ICollection<Food> AllConsumedFoods =>
            _allFoodsAsEaten ??= AllFoodConsumptions?.Select(fc => fc.Food).Distinct().ToList();

        /// <summary>
        /// Gets all sample properties of the compiled data source.
        /// </summary>
        public IDictionary<string, SampleProperty> AllAdditionalSampleProperties => _dataManager.GetAllAdditionalSampleProperties();

        /// <summary>
        /// Gets all sample years of the compiled data source.
        /// </summary>
        public ICollection<int> AllSampleYears => _dataManager.GetAllSampleYears();

        /// <summary>
        /// Gets all sample locations of the compiled data source.
        /// </summary>
        public ICollection<string> AllSampleLocations => _dataManager.GetAllSampleLocations();

        /// <summary>
        /// Gets all sample regions of the compiled data source.
        /// </summary>
        public ICollection<string> AllSampleRegions => _dataManager.GetAllSampleRegions();

        /// <summary>
        /// Gets all sample production methods of the compiled data source.
        /// </summary>
        public ICollection<string> AllSampleProductionMethods => _dataManager.GetAllSampleProductionMethods();

        /// <summary>
        /// Returns all focal commodity samples.
        /// </summary>
        public ICollection<FoodSample> AllFocalCommoditySamples => _dataManager.GetAllFocalFoodSamples().Values;

        /// <summary>
        /// Gets all focal sample properties of the compiled data source.
        /// </summary>
        public IDictionary<string, SampleProperty> AllFocalSampleProperties => _dataManager.GetAllFocalSampleProperties();

        /// <summary>
        /// Returns the selected focal commodity samples.
        /// </summary>
        public ICollection<FoodSample> SelectedFocalCommoditySamples =>
            AllFocalCommoditySamples?
                .Where(r => SelectedFocalCommodityFoods?.Contains(r.Food) ?? false)
                .ToList();

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
                    _selectedFoodSamples = [.. selectedFoodSamples];
                }
                return _selectedFoodSamples;
            }
        }


        /// <summary>
        /// Gets all analytical methods of the compiled data source.
        /// </summary>
        public IDictionary<string, AnalyticalMethod> AllAnalyticalMethods => _dataManager.GetAllAnalyticalMethods();

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
        public ICollection<ConcentrationLimit> AllMaximumConcentrationLimits =>
            _allMaximumConcentrationLimits ??= [.. _dataManager.GetAllMaximumConcentrationLimits()];

        /// <summary>
        /// Returns all residue definitions.
        /// </summary>
        public IList<SubstanceConversion> AllSubstanceConversions => _dataManager.GetAllSubstanceConversions();

        /// <summary>
        /// Gets all processing types.
        /// </summary>
        public ICollection<ProcessingType> AllProcessingTypes => _dataManager.GetAllProcessingTypes().Values.ToHashSet();

        /// <summary>
        /// Returns all non-compound-specific processing factors.
        /// </summary>
        public ICollection<ProcessingFactor> AllProcessingFactors => _dataManager.GetAllProcessingFactors();

        /// <summary>
        /// Gets all agricultural uses.
        /// </summary>
        public ICollection<OccurrencePattern> AllOccurrencePatterns => _dataManager.GetAllOccurrencePatterns();

        /// <summary>
        /// Gets all occurrence frequencies.
        /// </summary>
        public ICollection<OccurrenceFrequency> AllOccurrenceFrequencies => _dataManager.GetAllOccurrenceFrequencies();

        /// <summary>
        /// Gets all authorised uses.
        /// </summary>
        public ICollection<SubstanceAuthorisation> AllSubstanceAuthorisations => _dataManager.GetAllSubstanceAuthorisations();

        /// <summary>
        /// Gets all substance approvals.
        /// </summary>
        public ICollection<SubstanceApproval> AllSubstanceApprovals => _dataManager.GetAllSubstanceApprovals();

        /// <summary>
        /// Returns all relative potency factors of the compiled datasource.
        /// </summary>
        public IDictionary<string, List<RelativePotencyFactor>> AllRelativePotencyFactors => _dataManager.GetAllRelativePotencyFactors();

        /// <summary>
        /// Returns all hazard doses of the compiled datasource.
        /// </summary>
        public ICollection<Compiled.Objects.PointOfDeparture> AllPointsOfDeparture => _dataManager.GetAllPointsOfDeparture();

        /// <summary>
        /// Returns all hazard characterisations.
        /// </summary>
        public ICollection<HazardCharacterisation> AllHazardCharacterisations => _dataManager.GetAllHazardCharacterisations();

        public ICollection<ActiveSubstanceModel> AllActiveSubstances => [.. _dataManager.GetAllActiveSubstanceModels().Values];

        public ICollection<MolecularDockingModel> AllMolecularDockingModels => [.. _dataManager.GetAllMolecularDockingModels().Values];

        public ICollection<QsarMembershipModel> AllQsarMembershipModels => [.. _dataManager.GetAllQsarMembershipModels().Values];

        /// <summary>
        /// Gets all human monitoring surveys.
        /// </summary>
        public ICollection<HumanMonitoringSurvey> AllHumanMonitoringSurveys => [.. _dataManager.GetAllHumanMonitoringSurveys().Values];

        /// <summary>
        /// Gets all human monitoring individuals.
        /// </summary>
        public ICollection<Individual> AllHumanMonitoringIndividuals => [.. _dataManager.GetAllHumanMonitoringIndividuals().Values];

        /// <summary>
        /// Returns the all the individual properties present in the data source.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IndividualProperty> AllHumanMonitoringIndividualProperties => _dataManager.GetAllHumanMonitoringIndividualProperties();

        /// <summary>
        /// Returns all human monitoring samples.
        /// </summary>
        public ICollection<HumanMonitoringSample> AllHumanMonitoringSamples => [.. _dataManager.GetAllHumanMonitoringSamples().Values];

        /// <summary>
        /// Returns all human monitoring analytical methods.
        /// </summary>
        public IDictionary<string, AnalyticalMethod> AllHumanMonitoringAnalyticalMethods => _dataManager.GetAllHumanMonitoringAnalyticalMethods();

        /// <summary>
        /// Gets all human monitoring endpoints of all surveys.
        /// </summary>
        /// <returns></returns>
        public ICollection<HumanMonitoringSamplingMethod> GetAllHumanMonitoringSamplingMethods() => _dataManager.GetAllHumanMonitoringSamplingMethods();

        /// <summary>
        /// Gets all populations.
        /// </summary>
        public ICollection<Population> AllPopulations => [.. _dataManager.GetAllPopulations().Values];

        /// <summary>
        /// Gets all inter-species factors.
        /// </summary>
        public ICollection<InterSpeciesFactor> AllInterSpeciesFactors => _dataManager.GetAllInterSpeciesFactors();

        /// <summary>
        /// Gets all intra-species factors.
        /// </summary>
        public ICollection<IntraSpeciesFactor> AllIntraSpeciesFactors => _dataManager.GetAllIntraSpeciesFactors();

        public IList<ConcentrationDistribution> AllConcentrationDistributions => _dataManager.GetAllConcentrationDistributions();

        public List<Food> AvailableScenarioAnalysisFoods =>
            [.. AllConcentrationDistributions
                .Where(s => s.Limit != null && s.Percentile != null && s.Limit / s.Percentile < 1)
                .Select(cd => cd.Food)
                .Distinct()];

        /// <summary>
        /// Returns all focal commodity foods.
        /// </summary>
        public HashSet<Food> AllFocalCommodityFoods => [.. _dataManager.GetAllFocalCommodityFoods().Values];

        /// <summary>
        /// Returns all analytical methods of the focal commodity samples.
        /// </summary>
        public IDictionary<string, AnalyticalMethod> AllFocalCommodityAnalyticalMethods => _dataManager.GetAllFocalFoodAnalyticalMethods();

        /// <summary>
        ///
        /// </summary>
        public List<NonDietaryExposureSet> NonDietaryExposureSets => [.. _dataManager.GetAllNonDietaryExposureSets()];

        /// <summary>
        /// Returns all available dietary exposure models.
        /// </summary>
        public IDictionary<string, DietaryExposureModel> AllDietaryExposureModels => _dataManager.GetAllDietaryExposureModels();

        /// <summary>
        /// Returns all available target exposure models.
        /// </summary>
        public IDictionary<string, TargetExposureModel> AllTargetExposureModels => _dataManager.GetAllTargetExposureModels();

        /// <summary>
        /// Returns all available risk models.
        /// </summary>
        public IDictionary<string, RiskModel> AllRiskModels => _dataManager.GetAllRiskModels();
        /// <summary>
        /// Returns all unit variability factors.
        /// </summary>
        /// <returns></returns>
        public ICollection<UnitVariabilityFactor> GetAllUnitVariabilityFactors() => _dataManager.GetAllUnitVariabilityFactors();

        /// <summary>
        /// Returns all post harvest applications.
        /// </summary>
        /// <returns></returns>
        public ICollection<IestiSpecialCase> GetAllIestiSpecialCases() => _dataManager.GetAllIestiSpecialCases();

        /// <summary>
        /// Checks if there are unit variability factors specified in the form of coefficients.
        /// </summary>
        public bool UnitVariabilityDataHasCoefficients => _dataManager.GetAllUnitVariabilityFactors().Any(uv => uv.Coefficient.HasValue);

        /// <summary>
        /// Checks if there are unit variability factors specified in the form of factors.
        /// </summary>
        public bool UnitVariabilityDataHasFactors => _dataManager.GetAllUnitVariabilityFactors().Any(uv => uv.Factor.HasValue);

        /// <summary>
        /// Returns all responses.
        /// </summary>
        public IDictionary<string, Response> AllResponses => _dataManager.GetAllResponses();

        /// <summary>
        /// Returns the selected responses.
        /// </summary>
        public IDictionary<string, Response> SelectedResponses {
            get {
                var codes = Project.GetFilterCodes(ScopingType.Responses);

                if (codes?.Count > 0) {
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
        public ILookup<Effect, EffectRepresentation> AllEffectRepresentations => _dataManager.GetAllEffectRepresentations().ToLookup(r => r.Effect);

        /// <summary>
        /// Gets all dose response experiments from the compiled data source.
        /// </summary>
        public List<DoseResponseExperiment> AllDoseResponseExperiments => [.. _dataManager.GetAllDoseResponseExperiments().Values];

        /// <summary>
        /// Returns all test systems.
        /// </summary>
        public List<TestSystem> AllTestSystems => [.. _dataManager.GetAllTestSystems().Values];

        /// <summary>
        /// Returns all dose response models.
        /// </summary>
        public List<DoseResponseModel> AllDoseResponseModels => [.. _dataManager.GetAllDoseResponseModels()];

        /// <summary>
        /// Returns all available adverse outcome pathway networks.
        /// </summary>
        public IDictionary<string, AdverseOutcomePathwayNetwork> AllAdverseOutcomePathwayNetworks => _dataManager.GetAdverseOutcomePathwayNetworks();

        /// <summary>
        /// Gets all PBK models of the compiled data source.
        /// </summary>
        public List<KineticModelInstance> AllPbkModels => _dataManager.GetAllPbkModels(_tempDataFolder?.FullName)?.ToList();

        /// <summary>
        /// Gets all absorption factors
        /// </summary>
        public ICollection<SimpleAbsorptionFactor> AllAbsorptionFactors => _dataManager.GetAllAbsorptionFactors()?.ToList();

        /// <summary>
        /// Gets all kinetic model conversion factors
        /// </summary>
        public ICollection<KineticConversionFactor> AllKineticConversionFactors => _dataManager.GetAllKineticConversionFactors()?.ToList();

        public List<FoodTranslation> AllFoodTranslations => [.. _dataManager.GetAllFoodTranslations()];

        public List<TDSFoodSampleComposition> AllTDSFoodSampleCompositions => [.. _dataManager.GetAllTDSFoodSampleCompositions()];

        public IDictionary<Food, ICollection<Food>> AllFoodExtrapolations => _dataManager.GetAllFoodExtrapolations();

        public List<MarketShare> AllMarketShares => [.. _dataManager.GetAllMarketShares()];

        public ICollection<DeterministicSubstanceConversionFactor> AllDeterministicSubstanceConversionFactors => _dataManager.GetAllDeterministicSubstanceConversionFactors();

        public ICollection<ConcentrationSingleValue> AllConcentrationSingleValues => _dataManager.GetAllConcentrationSingleValues();

        public ICollection<ExposureBiomarkerConversion> AllExposureBiomarkerConversions => _dataManager.GetAllExposureBiomarkerConversions();

        public IDictionary<string, ExposureScenario> AllSingleValueNonDietaryExposureScenarios => _dataManager.GetAllSingleValuNonDietaryExposureScenarios();

        public IDictionary<string, ExposureDeterminantCombination> AllSingleValueNonDietaryExposureDeterminantCombinations => _dataManager.GetAllSingleValueNonDietaryExposureDeterminantCombinations();

        public IList<ExposureEstimate> AllSingleValueNonDietaryExposures => _dataManager.GetAllSingleValueNonDietaryExposures();
        public IList<AirConcentrationDistribution> AllIndoorAirConcentrationDistributions => _dataManager.GetAllIndoorAirConcentrationDistributions();
        public IList<AirConcentrationDistribution> AllOutdoorAirConcentrationDistributions => _dataManager.GetAllOutdoorAirConcentrationDistributions();

        public IList<AirConcentration> AllIndoorAirConcentrations => _dataManager.GetAllIndoorAirConcentrations();

        public IList<AirConcentration> AllOutdoorAirConcentrations => _dataManager.GetAllOutdoorAirConcentrations();

        public IList<AirIndoorFraction> AllAirIndoorFractions => _dataManager.GetAllAirIndoorFractions();

        public IList<AirVentilatoryFlowRate> AllAirVentilatoryFlowRates => _dataManager.GetAllAirVentilatoryFlowRates();
        public IList<AirBodyExposureFraction> AllAirBodyExposureFractions => _dataManager.GetAllAirBodyExposureFractions();

        public IList<SubstanceConcentration> AllDustConcentrations => _dataManager.GetAllDustConcentrations();

        public IList<DustConcentrationDistribution> AllDustConcentrationDistributions => _dataManager.GetAllDustConcentrationDistributions();

        public IList<DustIngestion> AllDustIngestions => _dataManager.GetAllDustIngestions();

        public IList<DustBodyExposureFraction> AllDustBodyExposureFractions => _dataManager.GetAllDustBodyExposureFractions();

        public IList<DustAdherenceAmount> AllDustAdherenceAmounts => _dataManager.GetAllDustAdherenceAmounts();

        public IList<DustAvailabilityFraction> AllDustAvailabilityFractions => _dataManager.GetAllDustAvailabilityFractions();

        public IList<ExposureResponseFunction> AllExposureResponseFunctions => _dataManager.GetAllExposureResponseFunctions();

        public IDictionary<string, PbkModelDefinition> AllPbkModelDefinitions => _dataManager.GetAllPbkModelDefinitions(_tempDataFolder?.FullName);

        public IList<SubstanceConcentration> AllSoilConcentrations => _dataManager.GetAllSoilConcentrations();

        public IList<SoilConcentrationDistribution> AllSoilConcentrationDistributions => _dataManager.GetAllSoilConcentrationDistributions();

        public IList<SoilIngestion> AllSoilIngestions => _dataManager.GetAllSoilIngestions();

        public IList<BurdenOfDisease> AllBurdensOfDisease => _dataManager.GetAllBurdensOfDisease();

        public IList<BodIndicatorConversion> AllBodIndicatorConversions => _dataManager.GetAllBodIndicatorConversions();

        /// <summary>
        /// Gets all consumer products of the compiled data source.
        /// </summary>
        public ICollection<ConsumerProduct> AllConsumerProducts => _dataManager.GetAllConsumerProducts().Values.ToHashSet();
        public ICollection<IndividualConsumerProductUseFrequency> AllIndividualConsumerProductUseFrequencies => _dataManager.GetAllIndividualConsumerProductUseFrequencies();
        public IDictionary<string, ConsumerProductSurvey> AllConsumerProductSurveys => _dataManager.GetAllConsumerProductSurveys();
        public ICollection<Individual> AllConsumerProductIndividuals => [.. _dataManager.GetAllConsumerProductIndividuals().Values];
        public IDictionary<string, IndividualProperty> AllConsumerProductIndividualProperties => _dataManager.GetAllConsumerProductIndividualProperties();
        public IList<ConsumerProductConcentration> AllConsumerProductConcentrations => _dataManager.GetAllConsumerProductConcentrations();
        public IList<ConsumerProductExposureFraction> AllConsumerProductExposureFractions => _dataManager.GetAllConsumerProductExposureFractions();
        public IList<ConsumerProductApplicationAmount> AllConsumerProductApplicationAmounts => _dataManager.GetAllConsumerProductApplicationAmounts();
        public IList<ConsumerProductConcentrationDistribution> AllConsumerProductConcentrationDistributions => _dataManager.GetAllConsumerProductConcentrationDistributions();
        public ICollection<HbmSingleValueExposureSet> GetAllHbmSingleValueExposureSets => _dataManager.GetAllHbmSingleValueExposures();

        public ICollection<IndividualSet> AllIndividualSets => [.. _dataManager.GetAllIndividualSets().Values];
        public ICollection<Individual> AllIndividualSetIndividuals => [.. _dataManager.GetAllIndividualSetIndividuals().Values];
        public IDictionary<string, IndividualProperty> AllIndividualSetIndividualProperties => _dataManager.GetAllIndividualSetIndividualProperties();

        public IDictionary<string, OccupationalTask> AllOccupationalTasks => _dataManager.GetAllOccupationalTasks();
        public IDictionary<string, OccupationalScenario> AllOccupationalScenarios => _dataManager.GetAllOccupationalScenarios();
        public ICollection<OccupationalTaskExposure> AllOccupationalTaskExposures => _dataManager.GetAllOccupationalTaskExposures();

    }
}
