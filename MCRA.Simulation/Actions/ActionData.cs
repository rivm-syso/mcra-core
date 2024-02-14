using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Data.Compiled.Wrappers.ISampleOriginInfo;
using MCRA.Data.Compiled.Wrappers.UnitVariability;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions.ActiveSubstances;
using MCRA.Simulation.Actions.AOPNetworks;
using MCRA.Simulation.Actions.ConcentrationDistributions;
using MCRA.Simulation.Actions.ConcentrationLimits;
using MCRA.Simulation.Actions.ConcentrationModels;
using MCRA.Simulation.Actions.Concentrations;
using MCRA.Simulation.Actions.Consumptions;
using MCRA.Simulation.Actions.ConsumptionsByModelledFood;
using MCRA.Simulation.Actions.DeterministicSubstanceConversionFactors;
using MCRA.Simulation.Actions.DietaryExposures;
using MCRA.Simulation.Actions.DoseResponseData;
using MCRA.Simulation.Actions.DoseResponseModels;
using MCRA.Simulation.Actions.EffectRepresentations;
using MCRA.Simulation.Actions.Effects;
using MCRA.Simulation.Actions.ExposureBiomarkerConversions;
using MCRA.Simulation.Actions.FocalFoodConcentrations;
using MCRA.Simulation.Actions.FoodConversions;
using MCRA.Simulation.Actions.FoodExtrapolations;
using MCRA.Simulation.Actions.FoodRecipes;
using MCRA.Simulation.Actions.Foods;
using MCRA.Simulation.Actions.HazardCharacterisations;
using MCRA.Simulation.Actions.HighExposureFoodSubstanceCombinations;
using MCRA.Simulation.Actions.HumanMonitoringAnalysis;
using MCRA.Simulation.Actions.HumanMonitoringData;
using MCRA.Simulation.Actions.InterSpeciesConversions;
using MCRA.Simulation.Actions.IntraSpeciesFactors;
using MCRA.Simulation.Actions.KineticModels;
using MCRA.Simulation.Actions.MarketShares;
using MCRA.Simulation.Actions.ModelledFoods;
using MCRA.Simulation.Actions.MolecularDockingModels;
using MCRA.Simulation.Actions.NonDietaryExposures;
using MCRA.Simulation.Actions.NonDietaryExposureSources;
using MCRA.Simulation.Actions.OccurrenceFrequencies;
using MCRA.Simulation.Actions.OccurrencePatterns;
using MCRA.Simulation.Actions.PointsOfDeparture;
using MCRA.Simulation.Actions.Populations;
using MCRA.Simulation.Actions.ProcessingFactors;
using MCRA.Simulation.Actions.QsarMembershipModels;
using MCRA.Simulation.Actions.RelativePotencyFactors;
using MCRA.Simulation.Actions.Responses;
using MCRA.Simulation.Actions.Risks;
using MCRA.Simulation.Actions.SingleValueConcentrations;
using MCRA.Simulation.Actions.SingleValueConsumptions;
using MCRA.Simulation.Actions.SingleValueDietaryExposures;
using MCRA.Simulation.Actions.SingleValueNonDietaryExposures;
using MCRA.Simulation.Actions.SingleValueRisks;
using MCRA.Simulation.Actions.SubstanceApprovals;
using MCRA.Simulation.Actions.SubstanceAuthorisations;
using MCRA.Simulation.Actions.SubstanceConversions;
using MCRA.Simulation.Actions.Substances;
using MCRA.Simulation.Actions.TargetExposures;
using MCRA.Simulation.Actions.TestSystems;
using MCRA.Simulation.Actions.TotalDietStudyCompositions;
using MCRA.Simulation.Actions.UnitVariabilityFactors;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.FoodExtrapolationsCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Simulation.Calculators.ModelledFoodsCalculation;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;
using MCRA.Simulation.Calculators.SingleValueInternalExposuresCalculation;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation {
    public class ActionData {

        public HashSet<ActionType> LoadedDataTypes { get; private set; } = new HashSet<ActionType>();

        public Dictionary<ActionType, IModuleOutputData> ModuleOutputData { get; set; } = new();

        public virtual T GetOrCreateModuleOutputData<T>(ActionType actionType) where T : IModuleOutputData, new() {
            if (!ModuleOutputData.TryGetValue(actionType, out var data)) {
                data = new T();
                ModuleOutputData[actionType] = data;
            }
            return (T)data;
        }

        // SingleValueConcentrations
        public ConcentrationUnit SingleValueConcentrationUnit {
            get {
                return GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).SingleValueConcentrationUnit;
            }
            set {
                GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).SingleValueConcentrationUnit = value;
            }
        }

        // Concentrations
        public ConcentrationUnit ConcentrationUnit {
            get {
                return GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ConcentrationUnit;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ConcentrationUnit = value;
            }
        }

        public BodyWeightUnit BodyWeightUnit {
            get {
                return FoodSurvey != null ? FoodSurvey.BodyWeightUnit : BodyWeightUnit.kg;
            }
        }

        public ConsumptionUnit ConsumptionUnit {
            get {
                return FoodSurvey != null ? FoodSurvey.ConsumptionUnit : ConsumptionUnit.g;
            }
        }

        // ActiveSubstances
        public ICollection<ActiveSubstanceModel> AvailableActiveSubstanceModels {
            get {
                return GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType.ActiveSubstances).AvailableActiveSubstanceModels;
            }
            set {
                GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType.ActiveSubstances).AvailableActiveSubstanceModels = value;
            }
        }
        public IDictionary<Compound, double> MembershipProbabilities {
            get {
                return GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType.ActiveSubstances).MembershipProbabilities;
            }
            set {
                GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType.ActiveSubstances).MembershipProbabilities = value;
            }
        }
        public ICollection<Compound> ActiveSubstances {
            get {
                return GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType.ActiveSubstances).ActiveSubstances;
            }
            set {
                GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType.ActiveSubstances).ActiveSubstances = value;
            }
        }

        // AOPNetworks
        public AdverseOutcomePathwayNetwork AdverseOutcomePathwayNetwork {
            get {
                return GetOrCreateModuleOutputData<AOPNetworksOutputData>(ActionType.AOPNetworks).AdverseOutcomePathwayNetwork;
            }
            set {
                GetOrCreateModuleOutputData<AOPNetworksOutputData>(ActionType.AOPNetworks).AdverseOutcomePathwayNetwork = value;
            }
        }

        public ICollection<Effect> RelevantEffects {
            get {
                return GetOrCreateModuleOutputData<AOPNetworksOutputData>(ActionType.AOPNetworks).RelevantEffects;
            }
            set {
                GetOrCreateModuleOutputData<AOPNetworksOutputData>(ActionType.AOPNetworks).RelevantEffects = value;
            }
        }

        // BiologicalMatrixConcentrationComparisons
        // ConcentrationDistributions

        public IDictionary<(Food Food, Compound Substance), ConcentrationDistribution> ConcentrationDistributions {
            get {
                return GetOrCreateModuleOutputData<ConcentrationDistributionsOutputData>(ActionType.ConcentrationDistributions).ConcentrationDistributions;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationDistributionsOutputData>(ActionType.ConcentrationDistributions).ConcentrationDistributions = value;
            }
        }


        // ConcentrationLimits
        public IDictionary<(Food Food, Compound Substance), ConcentrationLimit> MaximumConcentrationLimits {
            get {
                return GetOrCreateModuleOutputData<ConcentrationLimitsOutputData>(ActionType.ConcentrationLimits).MaximumConcentrationLimits;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationLimitsOutputData>(ActionType.ConcentrationLimits).MaximumConcentrationLimits = value;
            }
        }

        // ConcentrationModels

        public IDictionary<(Food Food, Compound Substance), ConcentrationModel> ConcentrationModels {
            get {
                return GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).ConcentrationModels;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).ConcentrationModels = value;
            }
        }

        public IDictionary<Food, ConcentrationModel> CumulativeConcentrationModels {
            get {
                return GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).CumulativeConcentrationModels;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).CumulativeConcentrationModels = value;
            }
        }

        public IDictionary<(Food Food, Compound Substance), CompoundResidueCollection> CompoundResidueCollections {
            get {
                return GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).CompoundResidueCollections;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).CompoundResidueCollections = value;
            }
        }

        public Dictionary<Food, CompoundResidueCollection> CumulativeCompoundResidueCollections {
            get {
                return GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).CumulativeCompoundResidueCollections;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).CumulativeCompoundResidueCollections = value;
            }
        }

        public ICollection<SampleCompoundCollection> MonteCarloSubstanceSampleCollections {
            get {
                return GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).MonteCarloSubstanceSampleCollections;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).MonteCarloSubstanceSampleCollections = value;
            }
        }

        // Concentrations
        public ILookup<Food, FoodSample> FoodSamples {
            get {
                return GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).FoodSamples;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).FoodSamples = value;
            }
        }

        public IDictionary<Food, List<ISampleOrigin>> SampleOriginInfos {
            get {
                return GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).SampleOriginInfos;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).SampleOriginInfos = value;
            }
        }

        public IDictionary<Food, SampleCompoundCollection> MeasuredSubstanceSampleCollections {
            get {
                return GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).MeasuredSubstanceSampleCollections;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).MeasuredSubstanceSampleCollections = value;
            }
        }

        public IDictionary<Food, SampleCompoundCollection> ActiveSubstanceSampleCollections {
            get {
                return GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ActiveSubstanceSampleCollections;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ActiveSubstanceSampleCollections = value;
            }
        }

        public ICollection<Food> MeasuredFoods {
            get {
                return GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).MeasuredFoods;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).MeasuredFoods = value;
            }
        }

        public ICollection<Compound> MeasuredSubstances {
            get {
                return GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).MeasuredSubstances;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).MeasuredSubstances = value;
            }
        }

        public ICollection<Compound> ModelledSubstances {
            get {
                return GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ModelledSubstances;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ModelledSubstances = value;
            }
        }

        public ICollection<FoodSubstanceExtrapolationCandidates> ExtrapolationCandidates {
            get {
                return GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ExtrapolationCandidates;
            }
            set {
                GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ExtrapolationCandidates = value;
            }
        }

        // Consumptions

        public ICollection<Individual> ConsumerIndividuals {
            get {
                return GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).ConsumerIndividuals;
            }
            set {
                GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).ConsumerIndividuals = value;
            }
        }

        public ICollection<IndividualDay> ConsumerIndividualDays {
            get {
                return GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).ConsumerIndividualDays;
            }
            set {
                GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).ConsumerIndividualDays = value;
            }
        }

        public ICollection<FoodConsumption> SelectedFoodConsumptions {
            get {
                return GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).SelectedFoodConsumptions;
            }
            set {
                GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).SelectedFoodConsumptions = value;
            }
        }

        public ICollection<Food> FoodsAsEaten {
            get {
                return GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).FoodsAsEaten;
            }
            set {
                GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).FoodsAsEaten = value;
            }
        }

        public IndividualProperty Cofactor {
            get {
                return GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).Cofactor;
            }
            set {
                GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).Cofactor = value;
            }
        }

        public IndividualProperty Covariable {
            get {
                return GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).Covariable;
            }
            set {
                GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).Covariable = value;
            }
        }

        // ConsumptionsByModeledFood

        public ICollection<Individual> ModelledFoodConsumers {
            get {
                return GetOrCreateModuleOutputData<ConsumptionsByModelledFoodOutputData>(ActionType.ConsumptionsByModelledFood).ModelledFoodConsumers;
            }
            set {
                GetOrCreateModuleOutputData<ConsumptionsByModelledFoodOutputData>(ActionType.ConsumptionsByModelledFood).ModelledFoodConsumers = value;
            }
        }

        public ICollection<IndividualDay> ModelledFoodConsumerDays {
            get {
                return GetOrCreateModuleOutputData<ConsumptionsByModelledFoodOutputData>(ActionType.ConsumptionsByModelledFood).ModelledFoodConsumerDays;
            }
            set {
                GetOrCreateModuleOutputData<ConsumptionsByModelledFoodOutputData>(ActionType.ConsumptionsByModelledFood).ModelledFoodConsumerDays = value;
            }
        }

        public ICollection<ConsumptionsByModelledFood> ConsumptionsByModelledFood {
            get {
                return GetOrCreateModuleOutputData<ConsumptionsByModelledFoodOutputData>(ActionType.ConsumptionsByModelledFood).ConsumptionsByModelledFood;
            }
            set {
                GetOrCreateModuleOutputData<ConsumptionsByModelledFoodOutputData>(ActionType.ConsumptionsByModelledFood).ConsumptionsByModelledFood = value;
            }
        }

        // DeterministicSubstanceConversionFactors

        public ICollection<DeterministicSubstanceConversionFactor> DeterministicSubstanceConversionFactors {
            get {
                return GetOrCreateModuleOutputData<DeterministicSubstanceConversionFactorsOutputData>(ActionType.DeterministicSubstanceConversionFactors).DeterministicSubstanceConversionFactors;
            }
            set {
                GetOrCreateModuleOutputData<DeterministicSubstanceConversionFactorsOutputData>(ActionType.DeterministicSubstanceConversionFactors).DeterministicSubstanceConversionFactors = value;
            }
        }

        // DietaryExposures
        public FoodSurvey FoodSurvey {
            get {
                return GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).FoodSurvey;
            }
            set {
                GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).FoodSurvey = value;
            }
        }

        public TargetUnit DietaryExposureUnit {
            get {
                return GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryExposureUnit;
            }
            set {
                GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryExposureUnit = value;
            }
        }

        public IntakeModelType DesiredIntakeModelType {
            get {
                return GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DesiredIntakeModelType;
            }
            set {
                GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DesiredIntakeModelType = value;
            }
        }

        public ICollection<ModelBasedIntakeResult> DietaryModelBasedIntakeResults {
            get {
                return GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryModelBasedIntakeResults;
            }
            set {
                GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryModelBasedIntakeResults = value;
            }
        }

        public ICollection<DietaryIndividualDayIntake> DietaryIndividualDayIntakes {
            get {
                return GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryIndividualDayIntakes;
            }
            set {
                GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryIndividualDayIntakes = value;
            }
        }

        public List<DietaryIndividualIntake> DietaryObservedIndividualMeans {
            get {
                return GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryObservedIndividualMeans;
            }
            set {
                GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryObservedIndividualMeans = value;
            }
        }

        public List<DietaryIndividualIntake> DietaryModelAssistedIntakes {
            get {
                return GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryModelAssistedIntakes;
            }
            set {
                GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryModelAssistedIntakes = value;
            }
        }

        public List<ModelAssistedIntake> DrillDownDietaryIndividualIntakes {
            get {
                return GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DrillDownDietaryIndividualIntakes;
            }
            set {
                GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DrillDownDietaryIndividualIntakes = value;
            }
        }

        public IIntakeModel DietaryExposuresIntakeModel {
            get {
                return GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryExposuresIntakeModel;
            }
            set {
                GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryExposuresIntakeModel = value;
            }
        }

        public IDictionary<(Food Food, Compound Substance), double> TdsReductionFactors {
            get {
                return GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).TdsReductionFactors;
            }
            set {
                GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).TdsReductionFactors = value;
            }
        }

        // ExposureBiomarkerConversions
        public ICollection<ExposureBiomarkerConversion> ExposureBiomarkerConversions {
            get {
                return GetOrCreateModuleOutputData<ExposureBiomarkerConversionsOutputData>(ActionType.ExposureBiomarkerConversions).ExposureBiomarkerConversions;
            }
            set {
                GetOrCreateModuleOutputData<ExposureBiomarkerConversionsOutputData>(ActionType.ExposureBiomarkerConversions).ExposureBiomarkerConversions = value;
            }
        }

        public ICollection<ExposureBiomarkerConversionModelBase> ExposureBiomarkerConversionModels {
            get {
                return GetOrCreateModuleOutputData<ExposureBiomarkerConversionsOutputData>(ActionType.ExposureBiomarkerConversions).ExposureBiomarkerConversionModels;
            }
            set {
                GetOrCreateModuleOutputData<ExposureBiomarkerConversionsOutputData>(ActionType.ExposureBiomarkerConversions).ExposureBiomarkerConversionModels = value;
            }
        }

        public ICollection<Food> TdsReductionScenarioAnalysisFoods {
            get {
                return GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).TdsReductionScenarioAnalysisFoods;
            }
            set {
                GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).TdsReductionScenarioAnalysisFoods = value;
            }
        }

        // DoseResponseData

        public ICollection<DoseResponseExperiment> AvailableDoseResponseExperiments {
            get {
                return GetOrCreateModuleOutputData<DoseResponseDataOutputData>(ActionType.DoseResponseData).AvailableDoseResponseExperiments;
            }
            set {
                GetOrCreateModuleOutputData<DoseResponseDataOutputData>(ActionType.DoseResponseData).AvailableDoseResponseExperiments = value;
            }
        }

        public ICollection<DoseResponseExperiment> SelectedResponseExperiments {
            get {
                return GetOrCreateModuleOutputData<DoseResponseDataOutputData>(ActionType.DoseResponseData).SelectedResponseExperiments;
            }
            set {
                GetOrCreateModuleOutputData<DoseResponseDataOutputData>(ActionType.DoseResponseData).SelectedResponseExperiments = value;
            }
        }

        // DoseResponseModels
        public ICollection<DoseResponseModel> DoseResponseModels {
            get {
                return GetOrCreateModuleOutputData<DoseResponseModelsOutputData>(ActionType.DoseResponseModels).DoseResponseModels;
            }
            set {
                GetOrCreateModuleOutputData<DoseResponseModelsOutputData>(ActionType.DoseResponseModels).DoseResponseModels = value;
            }
        }

        // EffectRepresentations

        public ILookup<Effect, EffectRepresentation> AllEffectRepresentations {
            get {
                return GetOrCreateModuleOutputData<EffectRepresentationsOutputData>(ActionType.EffectRepresentations).AllEffectRepresentations;
            }
            set {
                GetOrCreateModuleOutputData<EffectRepresentationsOutputData>(ActionType.EffectRepresentations).AllEffectRepresentations = value;
            }
        }

        public ICollection<EffectRepresentation> FocalEffectRepresentations {
            get {
                return GetOrCreateModuleOutputData<EffectRepresentationsOutputData>(ActionType.EffectRepresentations).FocalEffectRepresentations;
            }
            set {
                GetOrCreateModuleOutputData<EffectRepresentationsOutputData>(ActionType.EffectRepresentations).FocalEffectRepresentations = value;
            }
        }

        // Effects

        public ICollection<Effect> AllEffects {
            get {
                return GetOrCreateModuleOutputData<EffectsOutputData>(ActionType.Effects).AllEffects;
            }
            set {
                GetOrCreateModuleOutputData<EffectsOutputData>(ActionType.Effects).AllEffects = value;
            }
        }

        public Effect SelectedEffect {
            get {
                return GetOrCreateModuleOutputData<EffectsOutputData>(ActionType.Effects).SelectedEffect;
            }
            set {
                GetOrCreateModuleOutputData<EffectsOutputData>(ActionType.Effects).SelectedEffect = value;
            }
        }

        // FocalFoodConcentrations

        public ICollection<(Food Food, Compound Substance)> FocalCommodityCombinations {
            get {
                return GetOrCreateModuleOutputData<FocalFoodConcentrationsOutputData>(ActionType.FocalFoodConcentrations).FocalCommodityCombinations;
            }
            set {
                GetOrCreateModuleOutputData<FocalFoodConcentrationsOutputData>(ActionType.FocalFoodConcentrations).FocalCommodityCombinations = value;
            }
        }

        public ICollection<FoodSample> FocalCommoditySamples {
            get {
                return GetOrCreateModuleOutputData<FocalFoodConcentrationsOutputData>(ActionType.FocalFoodConcentrations).FocalCommoditySamples;
            }
            set {
                GetOrCreateModuleOutputData<FocalFoodConcentrationsOutputData>(ActionType.FocalFoodConcentrations).FocalCommoditySamples = value;
            }
        }

        public ICollection<SampleCompoundCollection> FocalCommoditySubstanceSampleCollections {
            get {
                return GetOrCreateModuleOutputData<FocalFoodConcentrationsOutputData>(ActionType.FocalFoodConcentrations).FocalCommoditySubstanceSampleCollections;
            }
            set {
                GetOrCreateModuleOutputData<FocalFoodConcentrationsOutputData>(ActionType.FocalFoodConcentrations).FocalCommoditySubstanceSampleCollections = value;
            }
        }

        // FoodConversion

        public ICollection<FoodConversionResult> FoodConversionResults {
            get {
                return GetOrCreateModuleOutputData<FoodConversionsOutputData>(ActionType.FoodConversions).FoodConversionResults;
            }
            set {
                GetOrCreateModuleOutputData<FoodConversionsOutputData>(ActionType.FoodConversions).FoodConversionResults = value;
            }
        }

        // FoodExtrapolations

        public IDictionary<Food, ICollection<Food>> FoodExtrapolations {
            get {
                return GetOrCreateModuleOutputData<FoodExtrapolationsOutputData>(ActionType.FoodExtrapolations).FoodExtrapolations;
            }
            set {
                GetOrCreateModuleOutputData<FoodExtrapolationsOutputData>(ActionType.FoodExtrapolations).FoodExtrapolations = value;
            }
        }

        // FoodRecipes

        public ICollection<FoodTranslation> FoodRecipes {
            get {
                return GetOrCreateModuleOutputData<FoodRecipesOutputData>(ActionType.FoodRecipes).FoodRecipes;
            }
            set {
                GetOrCreateModuleOutputData<FoodRecipesOutputData>(ActionType.FoodRecipes).FoodRecipes = value;
            }
        }

        // Foods
        public ICollection<Food> AllFoods {
            get {
                return GetOrCreateModuleOutputData<FoodsOutputData>(ActionType.Foods).AllFoods;
            }
            set {
                GetOrCreateModuleOutputData<FoodsOutputData>(ActionType.Foods).AllFoods = value;
            }
        }

        public IDictionary<string, Food> AllFoodsByCode {
            get {
                return GetOrCreateModuleOutputData<FoodsOutputData>(ActionType.Foods).AllFoodsByCode;
            }
            set {
                GetOrCreateModuleOutputData<FoodsOutputData>(ActionType.Foods).AllFoodsByCode = value;
            }
        }

        public ICollection<ProcessingType> ProcessingTypes {
            get {
                return GetOrCreateModuleOutputData<FoodsOutputData>(ActionType.Foods).ProcessingTypes;
            }
            set {
                GetOrCreateModuleOutputData<FoodsOutputData>(ActionType.Foods).ProcessingTypes = value;
            }
        }

        public TargetUnit HazardCharacterisationsUnit {
            get {
                return HazardCharacterisationModelsCollections?.FirstOrDefault()?.TargetUnit;
            }
        }

        public ICollection<HazardCharacterisationModelCompoundsCollection> HazardCharacterisationModelsCollections {
            get {
                return GetOrCreateModuleOutputData<HazardCharacterisationsOutputData>(ActionType.HazardCharacterisations).HazardCharacterisationModelsCollections;
            }
            set {
                GetOrCreateModuleOutputData<HazardCharacterisationsOutputData>(ActionType.HazardCharacterisations).HazardCharacterisationModelsCollections = value;
            }
        }

        // HighExposureFoodSusbtanceConbinations

        public ScreeningResult ScreeningResult {
            get {
                return GetOrCreateModuleOutputData<HighExposureFoodSubstanceCombinationsOutputData>(ActionType.HighExposureFoodSubstanceCombinations).ScreeningResult;
            }
            set {
                GetOrCreateModuleOutputData<HighExposureFoodSubstanceCombinationsOutputData>(ActionType.HighExposureFoodSubstanceCombinations).ScreeningResult = value;
            }
        }

        // HumanMonitoringData

        public ICollection<HumanMonitoringSurvey> HbmSurveys {
            get {
                return GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmSurveys;
            }
            set {
                GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmSurveys = value;
            }
        }

        public ICollection<Individual> HbmIndividuals {
            get {
                return GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmIndividuals;
            }
            set {
                GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmIndividuals = value;
            }
        }

        public ICollection<HumanMonitoringSample> HbmAllSamples {
            get {
                return GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmAllSamples;
            }
            set {
                GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmAllSamples = value;
            }
        }

        public ICollection<HumanMonitoringSamplingMethod> HbmSamplingMethods {
            get {
                return GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmSamplingMethods;
            }
            set {
                GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmSamplingMethods = value;
            }
        }

        public ICollection<HumanMonitoringSampleSubstanceCollection> HbmSampleSubstanceCollections {
            get {
                return GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmSampleSubstanceCollections;
            }
            set {
                GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmSampleSubstanceCollections = value;
            }
        }

        public ICollection<HbmIndividualDayCollection> HbmIndividualDayCollections {
            get {
                return GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmIndividualDayCollections;
            }
            set {
                GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmIndividualDayCollections = value;
            }
        }

        public ICollection<HbmIndividualCollection> HbmIndividualCollections {
            get {
                return GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmIndividualCollections;
            }
            set {
                GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmIndividualCollections = value;
            }
        }

        public HbmCumulativeIndividualCollection HbmCumulativeIndividualCollection {
            get {
                return GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmCumulativeIndividualCollection;
            }
            set {
                GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmCumulativeIndividualCollection = value;
            }
        }

        public HbmCumulativeIndividualDayCollection HbmCumulativeIndividualDayCollection {
            get {
                return GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmCumulativeIndividualDayCollection;
            }
            set {
                GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmCumulativeIndividualDayCollection = value;
            }
        }


        // InterSpeciesConversions

        public ICollection<InterSpeciesFactor> InterSpeciesFactors {
            get {
                return GetOrCreateModuleOutputData<InterSpeciesConversionsOutputData>(ActionType.InterSpeciesConversions).InterSpeciesFactors;
            }
            set {
                GetOrCreateModuleOutputData<InterSpeciesConversionsOutputData>(ActionType.InterSpeciesConversions).InterSpeciesFactors = value;
            }
        }

        public IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> InterSpeciesFactorModels {
            get {
                return GetOrCreateModuleOutputData<InterSpeciesConversionsOutputData>(ActionType.InterSpeciesConversions).InterSpeciesFactorModels;
            }
            set {
                GetOrCreateModuleOutputData<InterSpeciesConversionsOutputData>(ActionType.InterSpeciesConversions).InterSpeciesFactorModels = value;
            }
        }

        // IntraSpeciesFactors

        public ICollection<IntraSpeciesFactor> IntraSpeciesFactors {
            get {
                return GetOrCreateModuleOutputData<IntraSpeciesFactorsOutputData>(ActionType.IntraSpeciesFactors).IntraSpeciesFactors;
            }
            set {
                GetOrCreateModuleOutputData<IntraSpeciesFactorsOutputData>(ActionType.IntraSpeciesFactors).IntraSpeciesFactors = value;
            }
        }

        public IDictionary<(Effect, Compound), IntraSpeciesFactorModel> IntraSpeciesFactorModels {
            get {
                return GetOrCreateModuleOutputData<IntraSpeciesFactorsOutputData>(ActionType.IntraSpeciesFactors).IntraSpeciesFactorModels;
            }
            set {
                GetOrCreateModuleOutputData<IntraSpeciesFactorsOutputData>(ActionType.IntraSpeciesFactors).IntraSpeciesFactorModels = value;
            }
        }

        // KineticModels

        public ICollection<KineticModelInstance> KineticModelInstances {
            get {
                return GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).KineticModelInstances;
            }
            set {
                GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).KineticModelInstances = value;
            }
        }

        public ICollection<KineticAbsorptionFactor> KineticAbsorptionFactors {
            get {
                return GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).KineticAbsorptionFactors;
            }
            set {
                GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).KineticAbsorptionFactors = value;
            }
        }

        public ICollection<KineticConversionFactor> KineticConversionFactors {
            get {
                return GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).KineticConversionFactors;
            }
            set {
                GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).KineticConversionFactors = value;
            }
        }

        public ICollection<KineticConversionFactorModelBase> KineticConversionFactorModels {
            get {
                return GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).KineticConversionFactorModels;
            }
            set {
                GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).KineticConversionFactorModels = value;
            }
        }

        public IDictionary<(ExposurePathType, Compound), double> AbsorptionFactors {
            get {
                return GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).AbsorptionFactors;
            }
            set {
                GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).AbsorptionFactors = value;
            }
        }

        // MarketShares
        public ICollection<MarketShare> MarketShares {
            get {
                return GetOrCreateModuleOutputData<MarketSharesOutputData>(ActionType.MarketShares).MarketShares;
            }
            set {
                GetOrCreateModuleOutputData<MarketSharesOutputData>(ActionType.MarketShares).MarketShares = value;
            }
        }

        // ModelledFoods

        public ILookup<Food, ModelledFoodInfo> ModelledFoodInfos {
            get {
                return GetOrCreateModuleOutputData<ModelledFoodsOutputData>(ActionType.ModelledFoods).ModelledFoodInfos;
            }
            set {
                GetOrCreateModuleOutputData<ModelledFoodsOutputData>(ActionType.ModelledFoods).ModelledFoodInfos = value;
            }
        }

        public ICollection<Food> ModelledFoods {
            get {
                return GetOrCreateModuleOutputData<ModelledFoodsOutputData>(ActionType.ModelledFoods).ModelledFoods;
            }
            set {
                GetOrCreateModuleOutputData<ModelledFoodsOutputData>(ActionType.ModelledFoods).ModelledFoods = value;
            }
        }

        // MolecularDockingModels

        public ICollection<MolecularDockingModel> MolecularDockingModels {
            get {
                return GetOrCreateModuleOutputData<MolecularDockingModelsOutputData>(ActionType.MolecularDockingModels).MolecularDockingModels;
            }
            set {
                GetOrCreateModuleOutputData<MolecularDockingModelsOutputData>(ActionType.MolecularDockingModels).MolecularDockingModels = value;
            }
        }

        // NonDietaryExposures

        public ICollection<NonDietaryExposureSet> NonDietaryExposureSets {
            get {
                return GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposureSets;
            }
            set {
                GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposureSets = value;
            }
        }

        public IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> NonDietaryExposures {
            get {
                return GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposures;
            }
            set {
                GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposures = value;
            }
        }

        public ICollection<ExposurePathType> NonDietaryExposureRoutes {
            get {
                return GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposureRoutes;
            }
            set {
                GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposureRoutes = value;
            }
        }

        public ExternalExposureUnit NonDietaryExposureUnit {
            get {
                return GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposureUnit;
            }
        }

        // NonDietaryExposureSources

        public ICollection<NonDietaryExposureSource> NonDietaryExposureSources {
            get {
                return GetOrCreateModuleOutputData<NonDietaryExposureSourcesOutputData>(ActionType.NonDietaryExposureSources).NonDietaryExposureSources;
            }
            set {
                GetOrCreateModuleOutputData<NonDietaryExposureSourcesOutputData>(ActionType.NonDietaryExposureSources).NonDietaryExposureSources = value;
            }
        }

        // OccurrenceFrequencies

        public IDictionary<(Food Food, Compound Substance), OccurrenceFraction> OccurrenceFractions {
            get {
                return GetOrCreateModuleOutputData<OccurrenceFrequenciesOutputData>(ActionType.OccurrenceFrequencies).OccurrenceFractions;
            }
            set {
                GetOrCreateModuleOutputData<OccurrenceFrequenciesOutputData>(ActionType.OccurrenceFrequencies).OccurrenceFractions = value;
            }
        }

        // OccurrencePatterns

        public Dictionary<Food, List<MarginalOccurrencePattern>> MarginalOccurrencePatterns {
            get {
                return GetOrCreateModuleOutputData<OccurrencePatternsOutputData>(ActionType.OccurrencePatterns).MarginalOccurrencePatterns;
            }
            set {
                GetOrCreateModuleOutputData<OccurrencePatternsOutputData>(ActionType.OccurrencePatterns).MarginalOccurrencePatterns = value;
            }
        }

        public ICollection<OccurrencePattern> RawAgriculturalUses {
            get {
                return GetOrCreateModuleOutputData<OccurrencePatternsOutputData>(ActionType.OccurrencePatterns).RawAgriculturalUses;
            }
            set {
                GetOrCreateModuleOutputData<OccurrencePatternsOutputData>(ActionType.OccurrencePatterns).RawAgriculturalUses = value;
            }
        }

        // PointsOfDeparture

        public ICollection<Data.Compiled.Objects.PointOfDeparture> PointsOfDeparture {
            get {
                return GetOrCreateModuleOutputData<PointsOfDepartureOutputData>(ActionType.PointsOfDeparture).PointsOfDeparture;
            }
            set {
                GetOrCreateModuleOutputData<PointsOfDepartureOutputData>(ActionType.PointsOfDeparture).PointsOfDeparture = value;
            }
        }

        // Populations

        public Population SelectedPopulation {
            get {
                return GetOrCreateModuleOutputData<PopulationsOutputData>(ActionType.Populations).SelectedPopulation;
            }
            set {
                GetOrCreateModuleOutputData<PopulationsOutputData>(ActionType.Populations).SelectedPopulation = value;
            }
        }
        // ProcessingFactors

        public ICollection<ProcessingFactor> ProcessingFactors {
            get {
                return GetOrCreateModuleOutputData<ProcessingFactorsOutputData>(ActionType.ProcessingFactors).ProcessingFactors;
            }
            set {
                GetOrCreateModuleOutputData<ProcessingFactorsOutputData>(ActionType.ProcessingFactors).ProcessingFactors = value;
            }
        }

        public ProcessingFactorModelCollection ProcessingFactorModels {
            get {
                return GetOrCreateModuleOutputData<ProcessingFactorsOutputData>(ActionType.ProcessingFactors).ProcessingFactorModels;
            }
            set {
                GetOrCreateModuleOutputData<ProcessingFactorsOutputData>(ActionType.ProcessingFactors).ProcessingFactorModels = value;
            }
        }

        public IDictionary<(Food, Compound, ProcessingType), ProcessingFactor> ProcessingFactorsDictionary {
            get {
                return GetOrCreateModuleOutputData<ProcessingFactorsOutputData>(ActionType.ProcessingFactors).ProcessingFactorsDictionary;
            }
            set {
                GetOrCreateModuleOutputData<ProcessingFactorsOutputData>(ActionType.ProcessingFactors).ProcessingFactorsDictionary = value;
            }
        }

        // QsarMembershipModels

        public ICollection<QsarMembershipModel> QsarMembershipModels {
            get {
                return GetOrCreateModuleOutputData<QsarMembershipModelsOutputData>(ActionType.QsarMembershipModels).QsarMembershipModels;
            }
            set {
                GetOrCreateModuleOutputData<QsarMembershipModelsOutputData>(ActionType.QsarMembershipModels).QsarMembershipModels = value;
            }
        }

        // RelativePotencyFactors

        public IDictionary<Compound, RelativePotencyFactor> RawRelativePotencyFactors {
            get {
                return GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).RawRelativePotencyFactors;
            }
            set {
                GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).RawRelativePotencyFactors = value;
            }
        }

        public IDictionary<Compound, double> CorrectedRelativePotencyFactors {
            get {
                return GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).CorrectedRelativePotencyFactors;
            }
            set {
                GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).CorrectedRelativePotencyFactors = value;
            }
        }

        // Responses

        public IDictionary<string, Response> Responses {
            get {
                return GetOrCreateModuleOutputData<ResponsesOutputData>(ActionType.Responses).Responses;
            }
            set {
                GetOrCreateModuleOutputData<ResponsesOutputData>(ActionType.Responses).Responses = value;
            }
        }

        // Risks

        public ICollection<IndividualEffect> CumulativeIndividualEffects {
            get {
                return GetOrCreateModuleOutputData<RisksOutputData>(ActionType.Risks).CumulativeIndividualEffects;
            }
            set {
                GetOrCreateModuleOutputData<RisksOutputData>(ActionType.Risks).CumulativeIndividualEffects = value;
            }
        }

        // SingleValueConcentrations

        public ICollection<ConcentrationSingleValue> SingleValueConcentrations {
            get {
                return GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).SingleValueConcentrations;
            }
            set {
                GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).SingleValueConcentrations = value;
            }
        }

        public IDictionary<(Food Food, Compound Substance), SingleValueConcentrationModel> MeasuredSubstanceSingleValueConcentrations {
            get {
                return GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).MeasuredSubstanceSingleValueConcentrations;
            }
            set {
                GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).MeasuredSubstanceSingleValueConcentrations = value;
            }
        }

        public IDictionary<(Food Food, Compound Substance), SingleValueConcentrationModel> ActiveSubstanceSingleValueConcentrations {
            get {
                return GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).ActiveSubstanceSingleValueConcentrations;
            }
            set {
                GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).ActiveSubstanceSingleValueConcentrations = value;
            }
        }

        // SingleValueConsumptions

        public ICollection<SingleValueConsumptionModel> SingleValueConsumptionModels {
            get {
                return GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).SingleValueConsumptionModels;
            }
            set {
                GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).SingleValueConsumptionModels = value;
            }
        }

        public ConsumptionIntakeUnit SingleValueConsumptionIntakeUnit {
            get {
                return GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).SingleValueConsumptionIntakeUnit;
            }
            set {
                GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).SingleValueConsumptionIntakeUnit = value;
            }
        }

        public BodyWeightUnit SingleValueConsumptionBodyWeightUnit {
            get {
                return GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).SingleValueConsumptionBodyWeightUnit;
            }
            set {
                GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).SingleValueConsumptionBodyWeightUnit = value;
            }
        }

        public ICollection<PopulationConsumptionSingleValue> FoodConsumptionSingleValues {
            get {
                return GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).FoodConsumptionSingleValues;
            }
            set {
                GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).FoodConsumptionSingleValues = value;
            }
        }

        // SingleValueDietaryExposures

        public TargetUnit SingleValueDietaryExposureUnit {
            get {
                return GetOrCreateModuleOutputData<SingleValueDietaryExposuresOutputData>(ActionType.SingleValueDietaryExposures).SingleValueDietaryExposureUnit;
            }
            set {
                GetOrCreateModuleOutputData<SingleValueDietaryExposuresOutputData>(ActionType.SingleValueDietaryExposures).SingleValueDietaryExposureUnit = value;
            }
        }

        public ICollection<ISingleValueDietaryExposure> SingleValueDietaryExposureResults {
            get {
                return GetOrCreateModuleOutputData<SingleValueDietaryExposuresOutputData>(ActionType.SingleValueDietaryExposures).SingleValueDietaryExposureResults;
            }
            set {
                GetOrCreateModuleOutputData<SingleValueDietaryExposuresOutputData>(ActionType.SingleValueDietaryExposures).SingleValueDietaryExposureResults = value;
            }
        }

        // SingleValueRisks

        public ICollection<SingleValueRiskCalculationResult> SingleValueRiskCalculationResults {
            get {
                return GetOrCreateModuleOutputData<SingleValueRisksOutputData>(ActionType.SingleValueRisks).SingleValueRiskCalculationResults;
            }
            set {
                GetOrCreateModuleOutputData<SingleValueRisksOutputData>(ActionType.SingleValueRisks).SingleValueRiskCalculationResults = value;
            }
        }

        // SingleValueNonDietaryExposures

        public ICollection<ISingleValueNonDietaryExposure> SingleValueInternalExposureResults {
            get {
                return GetOrCreateModuleOutputData<SingleValueNonDietaryExposuresOutputData>(ActionType.SingleValueNonDietaryExposures).SingleValueInternalExposureResults;
            }
            set {
                GetOrCreateModuleOutputData<SingleValueNonDietaryExposuresOutputData>(ActionType.SingleValueNonDietaryExposures).SingleValueInternalExposureResults = value;
            }
        }

        // SubstanceApprovals

        public IDictionary<Compound, SubstanceApproval> SubstanceApprovals {
            get {
                return GetOrCreateModuleOutputData<SubstanceApprovalsOutputData>(ActionType.SubstanceApprovals).SubstanceApprovals;
            }
            set {
                GetOrCreateModuleOutputData<SubstanceApprovalsOutputData>(ActionType.SubstanceApprovals).SubstanceApprovals = value;
            }
        }

        // SubstanceAuthorisations

        public IDictionary<(Food Food, Compound Substance), SubstanceAuthorisation> SubstanceAuthorisations {
            get {
                return GetOrCreateModuleOutputData<SubstanceAuthorisationsOutputData>(ActionType.SubstanceAuthorisations).SubstanceAuthorisations;
            }
            set {
                GetOrCreateModuleOutputData<SubstanceAuthorisationsOutputData>(ActionType.SubstanceAuthorisations).SubstanceAuthorisations = value;
            }
        }

        // SubstanceConversions

        public ICollection<SubstanceConversion> SubstanceConversions {
            get {
                return GetOrCreateModuleOutputData<SubstanceConversionsOutputData>(ActionType.SubstanceConversions).SubstanceConversions;
            }
            set {
                GetOrCreateModuleOutputData<SubstanceConversionsOutputData>(ActionType.SubstanceConversions).SubstanceConversions = value;
            }
        }


        public Compound ReferenceSubstance {
            get {
                return GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).ReferenceSubstance;
            }
            set {
                GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).ReferenceSubstance = value;
            }
        }

        public ICollection<Compound> AllCompounds {
            get {
                return GetOrCreateModuleOutputData<SubstancesOutputData>(ActionType.Substances).AllCompounds;
            }
            set {
                GetOrCreateModuleOutputData<SubstancesOutputData>(ActionType.Substances).AllCompounds = value;
            }
        }

        public Compound CumulativeCompound {
            get {
                return GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).CumulativeCompound;
            }
            set {
                GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).CumulativeCompound = value;
            }
        }

        // TargetExposures
        public ExposureUnitTriple ExternalExposureUnit {
            get {
                return GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).ExternalExposureUnit;
            }
            set {
                GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).ExternalExposureUnit = value;
            }
        }

        public TargetUnit TargetExposureUnit {
            get {
                return GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).TargetExposureUnit;
            }
            set {
                GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).TargetExposureUnit = value;
            }
        }

        public ICollection<ExposurePathType> ExposureRoutes {
            get {
                return GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).ExposureRoutes;
            }
            set {
                GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).ExposureRoutes = value;
            }
        }

        public ICollection<AggregateIndividualDayExposure> AggregateIndividualDayExposures {
            get {
                return GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).AggregateIndividualDayExposures;
            }
            set {
                GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).AggregateIndividualDayExposures = value;
            }
        }

        public ICollection<AggregateIndividualExposure> AggregateIndividualExposures {
            get {
                return GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).AggregateIndividualExposures;
            }
            set {
                GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).AggregateIndividualExposures = value;
            }
        }

        // TestSystems
        public ICollection<TestSystem> TestSystems {
            get {
                return GetOrCreateModuleOutputData<TestSystemsOutputData>(ActionType.TestSystems).TestSystems;
            }
            set {
                GetOrCreateModuleOutputData<TestSystemsOutputData>(ActionType.TestSystems).TestSystems = value;
            }
        }

        // TotalDietStudyCompositions
        public ILookup<Food, TDSFoodSampleComposition> TdsFoodCompositions {
            get {
                return GetOrCreateModuleOutputData<TotalDietStudyCompositionsOutputData>(ActionType.TotalDietStudyCompositions).TdsFoodCompositions;
            }
            set {
                GetOrCreateModuleOutputData<TotalDietStudyCompositionsOutputData>(ActionType.TotalDietStudyCompositions).TdsFoodCompositions = value;
            }
        }

        // UnitVariabilityFactors
        public Dictionary<Food, FoodUnitVariabilityInfo> UnitVariabilityDictionary {
            get {
                return GetOrCreateModuleOutputData<UnitVariabilityFactorsOutputData>(ActionType.UnitVariabilityFactors).UnitVariabilityDictionary;
            }
            set {
                GetOrCreateModuleOutputData<UnitVariabilityFactorsOutputData>(ActionType.UnitVariabilityFactors).UnitVariabilityDictionary = value;
            }
        }

        public ICollection<IestiSpecialCase> IestiSpecialCases {
            get {
                return GetOrCreateModuleOutputData<UnitVariabilityFactorsOutputData>(ActionType.UnitVariabilityFactors).IestiSpecialCases;
            }
            set {
                GetOrCreateModuleOutputData<UnitVariabilityFactorsOutputData>(ActionType.UnitVariabilityFactors).IestiSpecialCases = value;
            }
        }

        /// <summary>
        /// Creates a copy of the action data, to be used in bootstrap/uncertainty runs.
        /// </summary>
        /// <returns></returns>
        public ActionData Copy() {
            var newDataSource = new ActionData() {
                ModuleOutputData = ModuleOutputData.ToDictionary(r => r.Key, r => r.Value.Copy())
            };

            return newDataSource;
        }
    }
}
