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
using MCRA.Simulation.Actions.DustConcentrationDistributions;
using MCRA.Simulation.Actions.DustExposureDeterminants;
using MCRA.Simulation.Actions.DustExposures;
using MCRA.Simulation.Actions.EffectRepresentations;
using MCRA.Simulation.Actions.Effects;
using MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease;
using MCRA.Simulation.Actions.ExposureBiomarkerConversions;
using MCRA.Simulation.Actions.ExposureEffectFunctions;
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
using MCRA.Simulation.Actions.KineticConversionFactors;
using MCRA.Simulation.Actions.KineticModels;
using MCRA.Simulation.Actions.MarketShares;
using MCRA.Simulation.Actions.ModelledFoods;
using MCRA.Simulation.Actions.MolecularDockingModels;
using MCRA.Simulation.Actions.NonDietaryExposures;
using MCRA.Simulation.Actions.NonDietaryExposureSources;
using MCRA.Simulation.Actions.OccurrenceFrequencies;
using MCRA.Simulation.Actions.OccurrencePatterns;
using MCRA.Simulation.Actions.PbkModelDefinitions;
using MCRA.Simulation.Actions.PbkModels;
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
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.Calculators.FoodExtrapolationsCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion.ExposureBiomarkerConversionModels;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.ModelledFoodsCalculation;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;
using MCRA.Simulation.Calculators.SingleValueNonDietaryExposuresCalculation;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation {
    public class ActionData {

        public HashSet<ActionType> LoadedDataTypes { get; private set; } = [];

        public Dictionary<ActionType, IModuleOutputData> ModuleOutputData { get; set; } = [];

        public virtual T GetOrCreateModuleOutputData<T>(ActionType actionType) where T : IModuleOutputData, new() {
            if (!ModuleOutputData.TryGetValue(actionType, out var data)) {
                data = new T();
                ModuleOutputData[actionType] = data;
            }
            return (T)data;
        }

        // SingleValueConcentrations
        public ConcentrationUnit SingleValueConcentrationUnit {
            get => GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).SingleValueConcentrationUnit;
            set => GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).SingleValueConcentrationUnit = value;
        }

        // Concentrations
        public ConcentrationUnit ConcentrationUnit {
            get => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ConcentrationUnit;
            set => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ConcentrationUnit = value;
        }

        public BodyWeightUnit BodyWeightUnit => FoodSurvey?.BodyWeightUnit ?? BodyWeightUnit.kg;

        public ConsumptionUnit ConsumptionUnit => FoodSurvey?.ConsumptionUnit ?? ConsumptionUnit.g;

        // ActiveSubstances
        public ICollection<ActiveSubstanceModel> AvailableActiveSubstanceModels {
            get => GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType.ActiveSubstances).AvailableActiveSubstanceModels;
            set => GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType.ActiveSubstances).AvailableActiveSubstanceModels = value;
        }
        public IDictionary<Compound, double> MembershipProbabilities {
            get => GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType.ActiveSubstances).MembershipProbabilities;
            set => GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType.ActiveSubstances).MembershipProbabilities = value;
        }
        public ICollection<Compound> ActiveSubstances {
            get => GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType.ActiveSubstances).ActiveSubstances;
            set => GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType.ActiveSubstances).ActiveSubstances = value;
        }

        // AOPNetworks
        public AdverseOutcomePathwayNetwork AdverseOutcomePathwayNetwork {
            get => GetOrCreateModuleOutputData<AOPNetworksOutputData>(ActionType.AOPNetworks).AdverseOutcomePathwayNetwork;
            set => GetOrCreateModuleOutputData<AOPNetworksOutputData>(ActionType.AOPNetworks).AdverseOutcomePathwayNetwork = value;
        }

        public ICollection<Effect> RelevantEffects {
            get => GetOrCreateModuleOutputData<AOPNetworksOutputData>(ActionType.AOPNetworks).RelevantEffects;
            set => GetOrCreateModuleOutputData<AOPNetworksOutputData>(ActionType.AOPNetworks).RelevantEffects = value;
        }

        // BiologicalMatrixConcentrationComparisons
        // ConcentrationDistributions
        public IDictionary<(Food Food, Compound Substance), ConcentrationDistribution> ConcentrationDistributions {
            get => GetOrCreateModuleOutputData<ConcentrationDistributionsOutputData>(ActionType.ConcentrationDistributions).ConcentrationDistributions;
            set => GetOrCreateModuleOutputData<ConcentrationDistributionsOutputData>(ActionType.ConcentrationDistributions).ConcentrationDistributions = value;
        }

        // ConcentrationLimits
        public IDictionary<(Food Food, Compound Substance), ConcentrationLimit> MaximumConcentrationLimits {
            get => GetOrCreateModuleOutputData<ConcentrationLimitsOutputData>(ActionType.ConcentrationLimits).MaximumConcentrationLimits;
            set => GetOrCreateModuleOutputData<ConcentrationLimitsOutputData>(ActionType.ConcentrationLimits).MaximumConcentrationLimits = value;
        }

        // ConcentrationModels
        public IDictionary<(Food Food, Compound Substance), ConcentrationModel> ConcentrationModels {
            get => GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).ConcentrationModels;
            set => GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).ConcentrationModels = value;
        }

        public IDictionary<Food, ConcentrationModel> CumulativeConcentrationModels {
            get => GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).CumulativeConcentrationModels;
            set => GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).CumulativeConcentrationModels = value;
        }

        public IDictionary<(Food Food, Compound Substance), CompoundResidueCollection> CompoundResidueCollections {
            get => GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).CompoundResidueCollections;
            set => GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).CompoundResidueCollections = value;
        }

        public Dictionary<Food, CompoundResidueCollection> CumulativeCompoundResidueCollections {
            get => GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).CumulativeCompoundResidueCollections;
            set => GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).CumulativeCompoundResidueCollections = value;
        }

        public ICollection<SampleCompoundCollection> MonteCarloSubstanceSampleCollections {
            get => GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).MonteCarloSubstanceSampleCollections;
            set => GetOrCreateModuleOutputData<ConcentrationModelsOutputData>(ActionType.ConcentrationModels).MonteCarloSubstanceSampleCollections = value;
        }

        // Concentrations
        public ILookup<Food, FoodSample> FoodSamples {
            get => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).FoodSamples;
            set => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).FoodSamples = value;
        }

        public IDictionary<Food, List<ISampleOrigin>> SampleOriginInfos {
            get => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).SampleOriginInfos;
            set => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).SampleOriginInfos = value;
        }

        public IDictionary<Food, SampleCompoundCollection> MeasuredSubstanceSampleCollections {
            get => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).MeasuredSubstanceSampleCollections;
            set => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).MeasuredSubstanceSampleCollections = value;
        }

        public IDictionary<Food, SampleCompoundCollection> ActiveSubstanceSampleCollections {
            get => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ActiveSubstanceSampleCollections;
            set => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ActiveSubstanceSampleCollections = value;
        }

        public ICollection<Food> MeasuredFoods {
            get => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).MeasuredFoods;
            set => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).MeasuredFoods = value;
        }

        public ICollection<Compound> MeasuredSubstances {
            get => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).MeasuredSubstances;
            set => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).MeasuredSubstances = value;
        }

        public ICollection<Compound> ModelledSubstances {
            get => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ModelledSubstances;
            set => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ModelledSubstances = value;
        }

        public ICollection<FoodSubstanceExtrapolationCandidates> ExtrapolationCandidates {
            get => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ExtrapolationCandidates;
            set => GetOrCreateModuleOutputData<ConcentrationsOutputData>(ActionType.Concentrations).ExtrapolationCandidates = value;
        }

        // Consumptions
        public ICollection<Individual> ConsumerIndividuals {
            get => GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).ConsumerIndividuals;
            set => GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).ConsumerIndividuals = value;
        }

        public ICollection<IndividualDay> ConsumerIndividualDays {
            get => GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).ConsumerIndividualDays;
            set => GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).ConsumerIndividualDays = value;
        }

        public ICollection<FoodConsumption> SelectedFoodConsumptions {
            get => GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).SelectedFoodConsumptions;
            set => GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).SelectedFoodConsumptions = value;
        }

        public ICollection<Food> FoodsAsEaten {
            get => GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).FoodsAsEaten;
            set => GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).FoodsAsEaten = value;
        }

        public IndividualProperty Cofactor {
            get => GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).Cofactor;
            set => GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).Cofactor = value;
        }

        public IndividualProperty Covariable {
            get => GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).Covariable;
            set => GetOrCreateModuleOutputData<ConsumptionsOutputData>(ActionType.Consumptions).Covariable = value;
        }

        // ConsumptionsByModeledFood
        public ICollection<Individual> ModelledFoodConsumers {
            get => GetOrCreateModuleOutputData<ConsumptionsByModelledFoodOutputData>(ActionType.ConsumptionsByModelledFood).ModelledFoodConsumers;
            set => GetOrCreateModuleOutputData<ConsumptionsByModelledFoodOutputData>(ActionType.ConsumptionsByModelledFood).ModelledFoodConsumers = value;
        }

        public ICollection<IndividualDay> ModelledFoodConsumerDays {
            get => GetOrCreateModuleOutputData<ConsumptionsByModelledFoodOutputData>(ActionType.ConsumptionsByModelledFood).ModelledFoodConsumerDays;
            set => GetOrCreateModuleOutputData<ConsumptionsByModelledFoodOutputData>(ActionType.ConsumptionsByModelledFood).ModelledFoodConsumerDays = value;
        }

        public ICollection<ConsumptionsByModelledFood> ConsumptionsByModelledFood {
            get => GetOrCreateModuleOutputData<ConsumptionsByModelledFoodOutputData>(ActionType.ConsumptionsByModelledFood).ConsumptionsByModelledFood;
            set => GetOrCreateModuleOutputData<ConsumptionsByModelledFoodOutputData>(ActionType.ConsumptionsByModelledFood).ConsumptionsByModelledFood = value;
        }

        // DeterministicSubstanceConversionFactors
        public ICollection<DeterministicSubstanceConversionFactor> DeterministicSubstanceConversionFactors {
            get => GetOrCreateModuleOutputData<DeterministicSubstanceConversionFactorsOutputData>(ActionType.DeterministicSubstanceConversionFactors).DeterministicSubstanceConversionFactors;
            set => GetOrCreateModuleOutputData<DeterministicSubstanceConversionFactorsOutputData>(ActionType.DeterministicSubstanceConversionFactors).DeterministicSubstanceConversionFactors = value;
        }

        // DietaryExposures
        public FoodSurvey FoodSurvey {
            get => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).FoodSurvey;
            set => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).FoodSurvey = value;
        }

        public TargetUnit DietaryExposureUnit {
            get => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryExposureUnit;
            set => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryExposureUnit = value;
        }

        public IntakeModelType DesiredIntakeModelType {
            get => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DesiredIntakeModelType;
            set => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DesiredIntakeModelType = value;
        }

        public ICollection<ModelBasedIntakeResult> DietaryModelBasedIntakeResults {
            get => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryModelBasedIntakeResults;
            set => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryModelBasedIntakeResults = value;
        }

        public ICollection<DietaryIndividualDayIntake> DietaryIndividualDayIntakes {
            get => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryIndividualDayIntakes;
            set => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryIndividualDayIntakes = value;
        }

        public List<DietaryIndividualIntake> DietaryObservedIndividualMeans {
            get => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryObservedIndividualMeans;
            set => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryObservedIndividualMeans = value;
        }

        public List<DietaryIndividualIntake> DietaryModelAssistedIntakes {
            get => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryModelAssistedIntakes;
            set => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryModelAssistedIntakes = value;
        }

        public List<ModelAssistedIntake> DrillDownDietaryIndividualIntakes {
            get => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DrillDownDietaryIndividualIntakes;
            set => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DrillDownDietaryIndividualIntakes = value;
        }

        public IIntakeModel DietaryExposuresIntakeModel {
            get => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryExposuresIntakeModel;
            set => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).DietaryExposuresIntakeModel = value;
        }

        public IDictionary<(Food Food, Compound Substance), double> TdsReductionFactors {
            get => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).TdsReductionFactors;
            set => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).TdsReductionFactors = value;
        }

        // DustConcentrationDistributions
        public IList<DustConcentrationDistribution> DustConcentrationDistributions {
            get => GetOrCreateModuleOutputData<DustConcentrationDistributionsOutputData>(ActionType.DustConcentrationDistributions).DustConcentrationDistributions;
            set => GetOrCreateModuleOutputData<DustConcentrationDistributionsOutputData>(ActionType.DustConcentrationDistributions).DustConcentrationDistributions = value;
        }

        public ConcentrationUnit DustConcentrationUnit {
            get => GetOrCreateModuleOutputData<DustConcentrationDistributionsOutputData>(ActionType.DustConcentrationDistributions).DustConcentrationUnit;
            set => GetOrCreateModuleOutputData<DustConcentrationDistributionsOutputData>(ActionType.DustConcentrationDistributions).DustConcentrationUnit = value;
        }

        // DustExposures
        public ICollection<DustIndividualDayExposure> IndividualDustExposures {
            get => GetOrCreateModuleOutputData<DustExposuresOutputData>(ActionType.DustExposures).IndividualDustExposures;
            set => GetOrCreateModuleOutputData<DustExposuresOutputData>(ActionType.DustExposures).IndividualDustExposures = value;
        }

        public ExposureUnitTriple DustExposureUnit {
            get {
                return GetOrCreateModuleOutputData<DustExposuresOutputData>(ActionType.DustExposures).DustExposureUnit;
            }
            set {
                GetOrCreateModuleOutputData<DustExposuresOutputData>(ActionType.DustExposures).DustExposureUnit = value;
            }
        }

        // DustExposureDeterminants
        public IList<DustIngestion> DustIngestions {
            get => GetOrCreateModuleOutputData<DustExposureDeterminantsOutputData>(ActionType.DustExposureDeterminants).DustIngestions;
            set => GetOrCreateModuleOutputData<DustExposureDeterminantsOutputData>(ActionType.DustExposureDeterminants).DustIngestions = value;
        }
        public ExternalExposureUnit DustIngestionUnit {
            get => GetOrCreateModuleOutputData<DustExposureDeterminantsOutputData>(ActionType.DustExposureDeterminants).DustIngestionUnit;
            set => GetOrCreateModuleOutputData<DustExposureDeterminantsOutputData>(ActionType.DustExposureDeterminants).DustIngestionUnit = value;
        }

        public IList<DustBodyExposureFraction> DustBodyExposureFractions {
            get => GetOrCreateModuleOutputData<DustExposureDeterminantsOutputData>(ActionType.DustExposureDeterminants).DustBodyExposureFractions;
            set => GetOrCreateModuleOutputData<DustExposureDeterminantsOutputData>(ActionType.DustExposureDeterminants).DustBodyExposureFractions = value;
        }

        public IList<DustAdherenceAmount> DustAdherenceAmounts {
            get => GetOrCreateModuleOutputData<DustExposureDeterminantsOutputData>(ActionType.DustExposureDeterminants).DustAdherenceAmounts;
            set => GetOrCreateModuleOutputData<DustExposureDeterminantsOutputData>(ActionType.DustExposureDeterminants).DustAdherenceAmounts = value;
        }

        public IList<DustAvailabilityFraction> DustAvailabilityFractions {
            get => GetOrCreateModuleOutputData<DustExposureDeterminantsOutputData>(ActionType.DustExposureDeterminants).DustAvailabilityFractions;
            set => GetOrCreateModuleOutputData<DustExposureDeterminantsOutputData>(ActionType.DustExposureDeterminants).DustAvailabilityFractions = value;
        }

        // EnvironmentalBurdenOfDisease
        public List<EnvironmentalBurdenOfDiseaseResultRecord> AttributableEbds {
            get => GetOrCreateModuleOutputData<EnvironmentalBurdenOfDiseaseOutputData>(ActionType.EnvironmentalBurdenOfDisease).AttributableEbds;
            set => GetOrCreateModuleOutputData<EnvironmentalBurdenOfDiseaseOutputData>(ActionType.EnvironmentalBurdenOfDisease).AttributableEbds = value;
        }

        public List<ExposureEffectResultRecord> ExposureEffects {
            get => GetOrCreateModuleOutputData<EnvironmentalBurdenOfDiseaseOutputData>(ActionType.EnvironmentalBurdenOfDisease).ExposureEffects;
            set => GetOrCreateModuleOutputData<EnvironmentalBurdenOfDiseaseOutputData>(ActionType.EnvironmentalBurdenOfDisease).ExposureEffects = value;
        }

        // ExposureEffectFunctions
        public List<ExposureEffectFunction> ExposureEffectFunctions {
            get => (List<ExposureEffectFunction>)GetOrCreateModuleOutputData<ExposureEffectFunctionsOutputData>(ActionType.ExposureEffectFunctions).ExposureEffectFunctions;
            set => GetOrCreateModuleOutputData<ExposureEffectFunctionsOutputData>(ActionType.ExposureEffectFunctions).ExposureEffectFunctions = value;
        }

        // ExposureBiomarkerConversions
        public ICollection<ExposureBiomarkerConversion> ExposureBiomarkerConversions {
            get => GetOrCreateModuleOutputData<ExposureBiomarkerConversionsOutputData>(ActionType.ExposureBiomarkerConversions).ExposureBiomarkerConversions;
            set => GetOrCreateModuleOutputData<ExposureBiomarkerConversionsOutputData>(ActionType.ExposureBiomarkerConversions).ExposureBiomarkerConversions = value;
        }

        public ICollection<IExposureBiomarkerConversionModel> ExposureBiomarkerConversionModels {
            get => GetOrCreateModuleOutputData<ExposureBiomarkerConversionsOutputData>(ActionType.ExposureBiomarkerConversions).ExposureBiomarkerConversionModels;
            set => GetOrCreateModuleOutputData<ExposureBiomarkerConversionsOutputData>(ActionType.ExposureBiomarkerConversions).ExposureBiomarkerConversionModels = value;
        }

        public ICollection<Food> TdsReductionScenarioAnalysisFoods {
            get => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).TdsReductionScenarioAnalysisFoods;
            set => GetOrCreateModuleOutputData<DietaryExposuresOutputData>(ActionType.DietaryExposures).TdsReductionScenarioAnalysisFoods = value;
        }

        // DoseResponseData

        public ICollection<DoseResponseExperiment> AvailableDoseResponseExperiments {
            get => GetOrCreateModuleOutputData<DoseResponseDataOutputData>(ActionType.DoseResponseData).AvailableDoseResponseExperiments;
            set => GetOrCreateModuleOutputData<DoseResponseDataOutputData>(ActionType.DoseResponseData).AvailableDoseResponseExperiments = value;
        }

        public ICollection<DoseResponseExperiment> SelectedResponseExperiments {
            get => GetOrCreateModuleOutputData<DoseResponseDataOutputData>(ActionType.DoseResponseData).SelectedResponseExperiments;
            set => GetOrCreateModuleOutputData<DoseResponseDataOutputData>(ActionType.DoseResponseData).SelectedResponseExperiments = value;
        }

        // DoseResponseModels
        public ICollection<DoseResponseModel> DoseResponseModels {
            get => GetOrCreateModuleOutputData<DoseResponseModelsOutputData>(ActionType.DoseResponseModels).DoseResponseModels;
            set => GetOrCreateModuleOutputData<DoseResponseModelsOutputData>(ActionType.DoseResponseModels).DoseResponseModels = value;
        }

        // EffectRepresentations

        public ILookup<Effect, EffectRepresentation> AllEffectRepresentations {
            get => GetOrCreateModuleOutputData<EffectRepresentationsOutputData>(ActionType.EffectRepresentations).AllEffectRepresentations;
            set => GetOrCreateModuleOutputData<EffectRepresentationsOutputData>(ActionType.EffectRepresentations).AllEffectRepresentations = value;
        }

        public ICollection<EffectRepresentation> FocalEffectRepresentations {
            get => GetOrCreateModuleOutputData<EffectRepresentationsOutputData>(ActionType.EffectRepresentations).FocalEffectRepresentations;
            set => GetOrCreateModuleOutputData<EffectRepresentationsOutputData>(ActionType.EffectRepresentations).FocalEffectRepresentations = value;
        }

        // Effects

        public ICollection<Effect> AllEffects {
            get => GetOrCreateModuleOutputData<EffectsOutputData>(ActionType.Effects).AllEffects;
            set => GetOrCreateModuleOutputData<EffectsOutputData>(ActionType.Effects).AllEffects = value;
        }

        public Effect SelectedEffect {
            get => GetOrCreateModuleOutputData<EffectsOutputData>(ActionType.Effects).SelectedEffect;
            set => GetOrCreateModuleOutputData<EffectsOutputData>(ActionType.Effects).SelectedEffect = value;
        }

        // FocalFoodConcentrations

        public ICollection<(Food Food, Compound Substance)> FocalCommodityCombinations {
            get => GetOrCreateModuleOutputData<FocalFoodConcentrationsOutputData>(ActionType.FocalFoodConcentrations).FocalCommodityCombinations;
            set => GetOrCreateModuleOutputData<FocalFoodConcentrationsOutputData>(ActionType.FocalFoodConcentrations).FocalCommodityCombinations = value;
        }

        public ICollection<FoodSample> FocalCommoditySamples {
            get => GetOrCreateModuleOutputData<FocalFoodConcentrationsOutputData>(ActionType.FocalFoodConcentrations).FocalCommoditySamples;
            set => GetOrCreateModuleOutputData<FocalFoodConcentrationsOutputData>(ActionType.FocalFoodConcentrations).FocalCommoditySamples = value;
        }

        public ICollection<SampleCompoundCollection> FocalCommoditySubstanceSampleCollections {
            get => GetOrCreateModuleOutputData<FocalFoodConcentrationsOutputData>(ActionType.FocalFoodConcentrations).FocalCommoditySubstanceSampleCollections;
            set => GetOrCreateModuleOutputData<FocalFoodConcentrationsOutputData>(ActionType.FocalFoodConcentrations).FocalCommoditySubstanceSampleCollections = value;
        }

        // FoodConversion

        public ICollection<FoodConversionResult> FoodConversionResults {
            get => GetOrCreateModuleOutputData<FoodConversionsOutputData>(ActionType.FoodConversions).FoodConversionResults;
            set => GetOrCreateModuleOutputData<FoodConversionsOutputData>(ActionType.FoodConversions).FoodConversionResults = value;
        }

        // FoodExtrapolations

        public IDictionary<Food, ICollection<Food>> FoodExtrapolations {
            get => GetOrCreateModuleOutputData<FoodExtrapolationsOutputData>(ActionType.FoodExtrapolations).FoodExtrapolations;
            set => GetOrCreateModuleOutputData<FoodExtrapolationsOutputData>(ActionType.FoodExtrapolations).FoodExtrapolations = value;
        }

        // FoodRecipes

        public ICollection<FoodTranslation> FoodRecipes {
            get => GetOrCreateModuleOutputData<FoodRecipesOutputData>(ActionType.FoodRecipes).FoodRecipes;
            set => GetOrCreateModuleOutputData<FoodRecipesOutputData>(ActionType.FoodRecipes).FoodRecipes = value;
        }

        // Foods
        public ICollection<Food> AllFoods {
            get => GetOrCreateModuleOutputData<FoodsOutputData>(ActionType.Foods).AllFoods;
            set => GetOrCreateModuleOutputData<FoodsOutputData>(ActionType.Foods).AllFoods = value;
        }

        public IDictionary<string, Food> AllFoodsByCode {
            get => GetOrCreateModuleOutputData<FoodsOutputData>(ActionType.Foods).AllFoodsByCode;
            set => GetOrCreateModuleOutputData<FoodsOutputData>(ActionType.Foods).AllFoodsByCode = value;
        }

        public ICollection<ProcessingType> ProcessingTypes {
            get => GetOrCreateModuleOutputData<FoodsOutputData>(ActionType.Foods).ProcessingTypes;
            set => GetOrCreateModuleOutputData<FoodsOutputData>(ActionType.Foods).ProcessingTypes = value;
        }

        public TargetUnit HazardCharacterisationsUnit => HazardCharacterisationModelsCollections?.FirstOrDefault()?.TargetUnit;

        public ICollection<HazardCharacterisationModelCompoundsCollection> HazardCharacterisationModelsCollections {
            get => GetOrCreateModuleOutputData<HazardCharacterisationsOutputData>(ActionType.HazardCharacterisations).HazardCharacterisationModelsCollections;
            set => GetOrCreateModuleOutputData<HazardCharacterisationsOutputData>(ActionType.HazardCharacterisations).HazardCharacterisationModelsCollections = value;
        }

        // HighExposureFoodSusbtanceConbinations
        public ScreeningResult ScreeningResult {
            get => GetOrCreateModuleOutputData<HighExposureFoodSubstanceCombinationsOutputData>(ActionType.HighExposureFoodSubstanceCombinations).ScreeningResult;
            set => GetOrCreateModuleOutputData<HighExposureFoodSubstanceCombinationsOutputData>(ActionType.HighExposureFoodSubstanceCombinations).ScreeningResult = value;
        }

        // HumanMonitoringData
        public ICollection<HumanMonitoringSurvey> HbmSurveys {
            get => GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmSurveys;
            set => GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmSurveys = value;
        }

        public ICollection<Individual> HbmIndividuals {
            get => GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmIndividuals;
            set => GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmIndividuals = value;
        }

        public ICollection<HumanMonitoringSample> HbmAllSamples {
            get => GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmAllSamples;
            set => GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmAllSamples = value;
        }

        public ICollection<HumanMonitoringSamplingMethod> HbmSamplingMethods {
            get => GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmSamplingMethods;
            set => GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmSamplingMethods = value;
        }

        public ICollection<HumanMonitoringSampleSubstanceCollection> HbmSampleSubstanceCollections {
            get => GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmSampleSubstanceCollections;
            set => GetOrCreateModuleOutputData<HumanMonitoringDataOutputData>(ActionType.HumanMonitoringData).HbmSampleSubstanceCollections = value;
        }

        public ICollection<HbmIndividualDayCollection> HbmIndividualDayCollections {
            get => GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmIndividualDayCollections;
            set => GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmIndividualDayCollections = value;
        }

        public ICollection<HbmIndividualCollection> HbmIndividualCollections {
            get => GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmIndividualCollections;
            set => GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmIndividualCollections = value;
        }

        public HbmCumulativeIndividualCollection HbmCumulativeIndividualCollection {
            get => GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmCumulativeIndividualCollection;
            set => GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmCumulativeIndividualCollection = value;
        }

        public HbmCumulativeIndividualDayCollection HbmCumulativeIndividualDayCollection {
            get => GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmCumulativeIndividualDayCollection;
            set => GetOrCreateModuleOutputData<HumanMonitoringAnalysisOutputData>(ActionType.HumanMonitoringAnalysis).HbmCumulativeIndividualDayCollection = value;
        }

        // InterSpeciesConversions
        public ICollection<InterSpeciesFactor> InterSpeciesFactors {
            get => GetOrCreateModuleOutputData<InterSpeciesConversionsOutputData>(ActionType.InterSpeciesConversions).InterSpeciesFactors;
            set => GetOrCreateModuleOutputData<InterSpeciesConversionsOutputData>(ActionType.InterSpeciesConversions).InterSpeciesFactors = value;
        }

        public IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> InterSpeciesFactorModels {
            get => GetOrCreateModuleOutputData<InterSpeciesConversionsOutputData>(ActionType.InterSpeciesConversions).InterSpeciesFactorModels;
            set => GetOrCreateModuleOutputData<InterSpeciesConversionsOutputData>(ActionType.InterSpeciesConversions).InterSpeciesFactorModels = value;
        }

        // IntraSpeciesFactors
        public ICollection<IntraSpeciesFactor> IntraSpeciesFactors {
            get => GetOrCreateModuleOutputData<IntraSpeciesFactorsOutputData>(ActionType.IntraSpeciesFactors).IntraSpeciesFactors;
            set => GetOrCreateModuleOutputData<IntraSpeciesFactorsOutputData>(ActionType.IntraSpeciesFactors).IntraSpeciesFactors = value;
        }

        public IDictionary<(Effect, Compound), IntraSpeciesFactorModel> IntraSpeciesFactorModels {
            get => GetOrCreateModuleOutputData<IntraSpeciesFactorsOutputData>(ActionType.IntraSpeciesFactors).IntraSpeciesFactorModels;
            set => GetOrCreateModuleOutputData<IntraSpeciesFactorsOutputData>(ActionType.IntraSpeciesFactors).IntraSpeciesFactorModels = value;
        }

        // PbkModels
        public ICollection<KineticModelInstance> KineticModelInstances {
            get => GetOrCreateModuleOutputData<PbkModelsOutputData>(ActionType.PbkModels).KineticModelInstances;
            set => GetOrCreateModuleOutputData<PbkModelsOutputData>(ActionType.PbkModels).KineticModelInstances = value;
        }

        public ICollection<SimpleAbsorptionFactor> AbsorptionFactors {
            get => GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).SimpleAbsorptionFactors;
            set => GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).SimpleAbsorptionFactors = value;
        }

        public ICollection<IKineticConversionFactorModel> KineticConversionFactorModels {
            get => GetOrCreateModuleOutputData<KineticConversionFactorsOutputData>(ActionType.KineticConversionFactors).KineticConversionFactorModels;
            set => GetOrCreateModuleOutputData<KineticConversionFactorsOutputData>(ActionType.KineticConversionFactors).KineticConversionFactorModels = value;
        }

        public ICollection<IKineticConversionFactorModel> SimpleAbsorptionFactorModels {
            get => GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).AbsorptionFactorModels;
            set => GetOrCreateModuleOutputData<KineticModelsOutputData>(ActionType.KineticModels).AbsorptionFactorModels = value;
        }

        public ICollection<PbkModelDefinition> AllPbkModelDefinitions {
            get => GetOrCreateModuleOutputData<PbkModelDefinitionsOutputData>(ActionType.PbkModelDefinitions).PbkModelDefinitions;
            set => GetOrCreateModuleOutputData<PbkModelDefinitionsOutputData>(ActionType.PbkModelDefinitions).PbkModelDefinitions = value;
        }

        // MarketShares
        public ICollection<MarketShare> MarketShares {
            get => GetOrCreateModuleOutputData<MarketSharesOutputData>(ActionType.MarketShares).MarketShares;
            set => GetOrCreateModuleOutputData<MarketSharesOutputData>(ActionType.MarketShares).MarketShares = value;
        }

        // ModelledFoods
        public ILookup<Food, ModelledFoodInfo> ModelledFoodInfos {
            get => GetOrCreateModuleOutputData<ModelledFoodsOutputData>(ActionType.ModelledFoods).ModelledFoodInfos;
            set => GetOrCreateModuleOutputData<ModelledFoodsOutputData>(ActionType.ModelledFoods).ModelledFoodInfos = value;
        }

        public ICollection<Food> ModelledFoods {
            get => GetOrCreateModuleOutputData<ModelledFoodsOutputData>(ActionType.ModelledFoods).ModelledFoods;
            set => GetOrCreateModuleOutputData<ModelledFoodsOutputData>(ActionType.ModelledFoods).ModelledFoods = value;
        }

        // MolecularDockingModels
        public ICollection<MolecularDockingModel> MolecularDockingModels {
            get => GetOrCreateModuleOutputData<MolecularDockingModelsOutputData>(ActionType.MolecularDockingModels).MolecularDockingModels;
            set => GetOrCreateModuleOutputData<MolecularDockingModelsOutputData>(ActionType.MolecularDockingModels).MolecularDockingModels = value;
        }

        // NonDietaryExposures
        public ICollection<NonDietaryExposureSet> NonDietaryExposureSets {
            get => GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposureSets;
            set => GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposureSets = value;
        }

        public IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> NonDietaryExposures {
            get => GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposures;
            set => GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposures = value;
        }

        public ICollection<ExposurePathType> NonDietaryExposureRoutes {
            get => GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposureRoutes;
            set => GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposureRoutes = value;
        }

        public ExternalExposureUnit NonDietaryExposureUnit => GetOrCreateModuleOutputData<NonDietaryExposuresOutputData>(ActionType.NonDietaryExposures).NonDietaryExposureUnit;

        // NonDietaryExposureSources
        public ICollection<NonDietaryExposureSource> NonDietaryExposureSources {
            get => GetOrCreateModuleOutputData<NonDietaryExposureSourcesOutputData>(ActionType.NonDietaryExposureSources).NonDietaryExposureSources;
            set => GetOrCreateModuleOutputData<NonDietaryExposureSourcesOutputData>(ActionType.NonDietaryExposureSources).NonDietaryExposureSources = value;
        }

        // OccurrenceFrequencies
        public IDictionary<(Food Food, Compound Substance), OccurrenceFraction> OccurrenceFractions {
            get => GetOrCreateModuleOutputData<OccurrenceFrequenciesOutputData>(ActionType.OccurrenceFrequencies).OccurrenceFractions;
            set => GetOrCreateModuleOutputData<OccurrenceFrequenciesOutputData>(ActionType.OccurrenceFrequencies).OccurrenceFractions = value;
        }

        // OccurrencePatterns
        public Dictionary<Food, List<MarginalOccurrencePattern>> MarginalOccurrencePatterns {
            get => GetOrCreateModuleOutputData<OccurrencePatternsOutputData>(ActionType.OccurrencePatterns).MarginalOccurrencePatterns;
            set => GetOrCreateModuleOutputData<OccurrencePatternsOutputData>(ActionType.OccurrencePatterns).MarginalOccurrencePatterns = value;
        }

        public ICollection<OccurrencePattern> RawAgriculturalUses {
            get => GetOrCreateModuleOutputData<OccurrencePatternsOutputData>(ActionType.OccurrencePatterns).RawAgriculturalUses;
            set => GetOrCreateModuleOutputData<OccurrencePatternsOutputData>(ActionType.OccurrencePatterns).RawAgriculturalUses = value;
        }

        // PointsOfDeparture
        public ICollection<Data.Compiled.Objects.PointOfDeparture> PointsOfDeparture {
            get => GetOrCreateModuleOutputData<PointsOfDepartureOutputData>(ActionType.PointsOfDeparture).PointsOfDeparture;
            set => GetOrCreateModuleOutputData<PointsOfDepartureOutputData>(ActionType.PointsOfDeparture).PointsOfDeparture = value;
        }

        // Populations
        public Population SelectedPopulation {
            get => GetOrCreateModuleOutputData<PopulationsOutputData>(ActionType.Populations).SelectedPopulation;
            set => GetOrCreateModuleOutputData<PopulationsOutputData>(ActionType.Populations).SelectedPopulation = value;
        }
        // ProcessingFactors
        public ICollection<ProcessingFactor> ProcessingFactors {
            get => GetOrCreateModuleOutputData<ProcessingFactorsOutputData>(ActionType.ProcessingFactors).ProcessingFactors;
            set => GetOrCreateModuleOutputData<ProcessingFactorsOutputData>(ActionType.ProcessingFactors).ProcessingFactors = value;
        }

        public ProcessingFactorModelCollection ProcessingFactorModels {
            get => GetOrCreateModuleOutputData<ProcessingFactorsOutputData>(ActionType.ProcessingFactors).ProcessingFactorModels;
            set => GetOrCreateModuleOutputData<ProcessingFactorsOutputData>(ActionType.ProcessingFactors).ProcessingFactorModels = value;
        }

        public IDictionary<(Food, Compound, ProcessingType), ProcessingFactor> ProcessingFactorsDictionary {
            get => GetOrCreateModuleOutputData<ProcessingFactorsOutputData>(ActionType.ProcessingFactors).ProcessingFactorsDictionary;
            set => GetOrCreateModuleOutputData<ProcessingFactorsOutputData>(ActionType.ProcessingFactors).ProcessingFactorsDictionary = value;
        }

        // QsarMembershipModels
        public ICollection<QsarMembershipModel> QsarMembershipModels {
            get => GetOrCreateModuleOutputData<QsarMembershipModelsOutputData>(ActionType.QsarMembershipModels).QsarMembershipModels;
            set => GetOrCreateModuleOutputData<QsarMembershipModelsOutputData>(ActionType.QsarMembershipModels).QsarMembershipModels = value;
        }

        // RelativePotencyFactors
        public IDictionary<Compound, RelativePotencyFactor> RawRelativePotencyFactors {
            get => GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).RawRelativePotencyFactors;
            set => GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).RawRelativePotencyFactors = value;
        }

        public IDictionary<Compound, double> CorrectedRelativePotencyFactors {
            get => GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).CorrectedRelativePotencyFactors;
            set => GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).CorrectedRelativePotencyFactors = value;
        }

        // Responses
        public IDictionary<string, Response> Responses {
            get => GetOrCreateModuleOutputData<ResponsesOutputData>(ActionType.Responses).Responses;
            set => GetOrCreateModuleOutputData<ResponsesOutputData>(ActionType.Responses).Responses = value;
        }

        // Risks
        public ICollection<IndividualEffect> CumulativeIndividualEffects {
            get => GetOrCreateModuleOutputData<RisksOutputData>(ActionType.Risks).CumulativeIndividualEffects;
            set => GetOrCreateModuleOutputData<RisksOutputData>(ActionType.Risks).CumulativeIndividualEffects = value;
        }

        // SingleValueConcentrations
        public ICollection<ConcentrationSingleValue> SingleValueConcentrations {
            get => GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).SingleValueConcentrations;
            set => GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).SingleValueConcentrations = value;
        }

        public IDictionary<(Food Food, Compound Substance), SingleValueConcentrationModel> MeasuredSubstanceSingleValueConcentrations {
            get => GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).MeasuredSubstanceSingleValueConcentrations;
            set => GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).MeasuredSubstanceSingleValueConcentrations = value;
        }

        public IDictionary<(Food Food, Compound Substance), SingleValueConcentrationModel> ActiveSubstanceSingleValueConcentrations {
            get => GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).ActiveSubstanceSingleValueConcentrations;
            set => GetOrCreateModuleOutputData<SingleValueConcentrationsOutputData>(ActionType.SingleValueConcentrations).ActiveSubstanceSingleValueConcentrations = value;
        }

        // SingleValueConsumptions
        public ICollection<SingleValueConsumptionModel> SingleValueConsumptionModels {
            get => GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).SingleValueConsumptionModels;
            set => GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).SingleValueConsumptionModels = value;
        }

        public ConsumptionIntakeUnit SingleValueConsumptionIntakeUnit {
            get => GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).SingleValueConsumptionIntakeUnit;
            set => GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).SingleValueConsumptionIntakeUnit = value;
        }

        public BodyWeightUnit SingleValueConsumptionBodyWeightUnit {
            get => GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).SingleValueConsumptionBodyWeightUnit;
            set => GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).SingleValueConsumptionBodyWeightUnit = value;
        }

        public ICollection<PopulationConsumptionSingleValue> FoodConsumptionSingleValues {
            get => GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).FoodConsumptionSingleValues;
            set => GetOrCreateModuleOutputData<SingleValueConsumptionsOutputData>(ActionType.SingleValueConsumptions).FoodConsumptionSingleValues = value;
        }

        // SingleValueDietaryExposures
        public TargetUnit SingleValueDietaryExposureUnit {
            get => GetOrCreateModuleOutputData<SingleValueDietaryExposuresOutputData>(ActionType.SingleValueDietaryExposures).SingleValueDietaryExposureUnit;
            set => GetOrCreateModuleOutputData<SingleValueDietaryExposuresOutputData>(ActionType.SingleValueDietaryExposures).SingleValueDietaryExposureUnit = value;
        }

        public ICollection<ISingleValueDietaryExposure> SingleValueDietaryExposureResults {
            get => GetOrCreateModuleOutputData<SingleValueDietaryExposuresOutputData>(ActionType.SingleValueDietaryExposures).SingleValueDietaryExposureResults;
            set => GetOrCreateModuleOutputData<SingleValueDietaryExposuresOutputData>(ActionType.SingleValueDietaryExposures).SingleValueDietaryExposureResults = value;
        }

        // SingleValueRisks
        public ICollection<SingleValueRiskCalculationResult> SingleValueRiskCalculationResults {
            get => GetOrCreateModuleOutputData<SingleValueRisksOutputData>(ActionType.SingleValueRisks).SingleValueRiskCalculationResults;
            set => GetOrCreateModuleOutputData<SingleValueRisksOutputData>(ActionType.SingleValueRisks).SingleValueRiskCalculationResults = value;
        }

        // SingleValueNonDietaryExposures
        public ICollection<ISingleValueNonDietaryExposure> SingleValueNonDietaryExposuresResults {
            get => GetOrCreateModuleOutputData<SingleValueNonDietaryExposuresOutputData>(ActionType.SingleValueNonDietaryExposures).SingleValueNonDietaryExposuresResults;
            set => GetOrCreateModuleOutputData<SingleValueNonDietaryExposuresOutputData>(ActionType.SingleValueNonDietaryExposures).SingleValueNonDietaryExposuresResults = value;
        }

        public IDictionary<string, ExposureScenario> SingleValueNonDietaryExposureScenarios {
            get => GetOrCreateModuleOutputData<SingleValueNonDietaryExposuresOutputData>(ActionType.SingleValueNonDietaryExposures).SingleValueNonDietaryExposureScenarios;
            set => GetOrCreateModuleOutputData<SingleValueNonDietaryExposuresOutputData>(ActionType.SingleValueNonDietaryExposures).SingleValueNonDietaryExposureScenarios = value;
        }

        public IDictionary<string, ExposureDeterminantCombination> SingleValueNonDietaryExposureDeterminantCombinations {
            get => GetOrCreateModuleOutputData<SingleValueNonDietaryExposuresOutputData>(ActionType.SingleValueNonDietaryExposures).SingleValueNonDietaryExposureDeterminantCombinations;
            set => GetOrCreateModuleOutputData<SingleValueNonDietaryExposuresOutputData>(ActionType.SingleValueNonDietaryExposures).SingleValueNonDietaryExposureDeterminantCombinations = value;
        }

        public IList<ExposureEstimate> SingleValueNonDietaryExposureEstimates {
            get => GetOrCreateModuleOutputData<SingleValueNonDietaryExposuresOutputData>(ActionType.SingleValueNonDietaryExposures).SingleValueNonDietaryExposureEstimates;
            set => GetOrCreateModuleOutputData<SingleValueNonDietaryExposuresOutputData>(ActionType.SingleValueNonDietaryExposures).SingleValueNonDietaryExposureEstimates = value;
        }

        // SubstanceApprovals
        public IDictionary<Compound, SubstanceApproval> SubstanceApprovals {
            get => GetOrCreateModuleOutputData<SubstanceApprovalsOutputData>(ActionType.SubstanceApprovals).SubstanceApprovals;
            set => GetOrCreateModuleOutputData<SubstanceApprovalsOutputData>(ActionType.SubstanceApprovals).SubstanceApprovals = value;
        }

        // SubstanceAuthorisations
        public IDictionary<(Food Food, Compound Substance), SubstanceAuthorisation> SubstanceAuthorisations {
            get => GetOrCreateModuleOutputData<SubstanceAuthorisationsOutputData>(ActionType.SubstanceAuthorisations).SubstanceAuthorisations;
            set => GetOrCreateModuleOutputData<SubstanceAuthorisationsOutputData>(ActionType.SubstanceAuthorisations).SubstanceAuthorisations = value;
        }

        // SubstanceConversions
        public ICollection<SubstanceConversion> SubstanceConversions {
            get => GetOrCreateModuleOutputData<SubstanceConversionsOutputData>(ActionType.SubstanceConversions).SubstanceConversions;
            set => GetOrCreateModuleOutputData<SubstanceConversionsOutputData>(ActionType.SubstanceConversions).SubstanceConversions = value;
        }

        public Compound ReferenceSubstance {
            get => GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).ReferenceSubstance;
            set => GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).ReferenceSubstance = value;
        }

        public ICollection<Compound> AllCompounds {
            get => GetOrCreateModuleOutputData<SubstancesOutputData>(ActionType.Substances).AllCompounds;
            set => GetOrCreateModuleOutputData<SubstancesOutputData>(ActionType.Substances).AllCompounds = value;
        }

        public Compound CumulativeCompound {
            get => GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).CumulativeCompound;
            set => GetOrCreateModuleOutputData<RelativePotencyFactorsOutputData>(ActionType.RelativePotencyFactors).CumulativeCompound = value;
        }

        // TargetExposures
        public ExposureUnitTriple ExternalExposureUnit {
            get => GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).ExternalExposureUnit;
            set => GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).ExternalExposureUnit = value;
        }

        public TargetUnit TargetExposureUnit {
            get => GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).TargetExposureUnit;
            set => GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).TargetExposureUnit = value;
        }

        public ICollection<ExposurePathType> ExposureRoutes {
            get => GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).ExposureRoutes;
            set => GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).ExposureRoutes = value;
        }

        public ICollection<AggregateIndividualDayExposure> AggregateIndividualDayExposures {
            get => GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).AggregateIndividualDayExposures;
            set => GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).AggregateIndividualDayExposures = value;
        }

        public ICollection<AggregateIndividualExposure> AggregateIndividualExposures {
            get => GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).AggregateIndividualExposures;
            set => GetOrCreateModuleOutputData<TargetExposuresOutputData>(ActionType.TargetExposures).AggregateIndividualExposures = value;
        }

        // TestSystems
        public ICollection<TestSystem> TestSystems {
            get => GetOrCreateModuleOutputData<TestSystemsOutputData>(ActionType.TestSystems).TestSystems;
            set => GetOrCreateModuleOutputData<TestSystemsOutputData>(ActionType.TestSystems).TestSystems = value;
        }

        // TotalDietStudyCompositions
        public ILookup<Food, TDSFoodSampleComposition> TdsFoodCompositions {
            get => GetOrCreateModuleOutputData<TotalDietStudyCompositionsOutputData>(ActionType.TotalDietStudyCompositions).TdsFoodCompositions;
            set => GetOrCreateModuleOutputData<TotalDietStudyCompositionsOutputData>(ActionType.TotalDietStudyCompositions).TdsFoodCompositions = value;
        }

        // UnitVariabilityFactors
        public Dictionary<Food, FoodUnitVariabilityInfo> UnitVariabilityDictionary {
            get => GetOrCreateModuleOutputData<UnitVariabilityFactorsOutputData>(ActionType.UnitVariabilityFactors).UnitVariabilityDictionary;
            set => GetOrCreateModuleOutputData<UnitVariabilityFactorsOutputData>(ActionType.UnitVariabilityFactors).UnitVariabilityDictionary = value;
        }

        public ICollection<IestiSpecialCase> IestiSpecialCases {
            get => GetOrCreateModuleOutputData<UnitVariabilityFactorsOutputData>(ActionType.UnitVariabilityFactors).IestiSpecialCases;
            set => GetOrCreateModuleOutputData<UnitVariabilityFactorsOutputData>(ActionType.UnitVariabilityFactors).IestiSpecialCases = value;
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
