using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Data.Compiled.Wrappers.ISampleOriginInfo;
using MCRA.Data.Compiled.Wrappers.UnitVariability;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.FoodExtrapolationsCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Simulation.Calculators.ModelledFoodsCalculation;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation {
    public sealed class ActionData {

        public HashSet<ActionType> LoadedDataTypes { get; private set; } = new HashSet<ActionType>();

        public ConcentrationUnit ConcentrationUnit {
            get {
                return ReferenceCompound?.ConcentrationUnit ?? ConcentrationUnit.mgPerKg;
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

        public ConsumptionIntakeUnit SingleValueConsumptionIntakeUnit { get; set; }

        public BodyWeightUnit SingleValueConsumptionBodyWeightUnit { get; set; }
        public TargetUnit DietaryExposureUnit { get; set; }
        public TargetUnit SingleValueDietaryExposureUnit { get; set; }

        public ExposureUnit NonDietaryExposureUnit {
            get {
                if (NonDietaryExposureSets?.Any() ?? false) {
                    return NonDietaryExposureSets.First().NonDietarySurvey.ExposureUnit;
                } else {
                    return ExposureUnit.mgPerDay;
                }
            }
        }
        public IntakeModelType DesiredIntakeModelType { get; set; }
        public TargetUnit ExternalExposureUnit { get; set; }
        public TargetUnit TargetExposureUnit { get; set; }

        public ICollection<Food> AllFoods { get; set; }
        public IDictionary<string, Food> AllFoodsByCode { get; set; }

        public ICollection<Compound> AllCompounds { get; set; }
        public ICollection<Compound> ActiveSubstances { get; set; }
        public ICollection<Compound> MeasuredSubstances { get; set; }
        public Compound ReferenceCompound { get; set; }

        public ICollection<Effect> AllEffects { get; set; }
        public Effect SelectedEffect { get; set; }
        public ConcentrationUnit EffectUnit { get; set; }

        public AdverseOutcomePathwayNetwork AdverseOutcomePathwayNetwork { get; set; }
        public ICollection<Effect> RelevantEffects { get; set; }

        public IDictionary<string, Response> Responses { get; set; }
        public ICollection<TestSystem> TestSystems { get; set; }

        public Population SelectedPopulation { get; set; }

        public IndividualProperty Cofactor { get; set; }
        public IndividualProperty Covariable { get; set; }

        public IDictionary<(Food Food, Compound Substance), SubstanceAuthorisation> SubstanceAuthorisations { get; set; }

        public IDictionary<Compound, SubstanceApproval> SubstanceApprovals { get; set; }    

        public IDictionary<(Food Food, Compound Substance), OccurrenceFraction> OccurrenceFractions { get; set; }
        public Dictionary<Food, List<MarginalOccurrencePattern>> MarginalOccurrencePatterns { get; set; }
        public ICollection<OccurrencePattern> RawAgriculturalUses { get; set; }

        public IDictionary<Compound, double> CorrectedRelativePotencyFactors { get; set; }

        public IDictionary<(Food Food, Compound Substance), CompoundResidueCollection> CompoundResidueCollections { get; set; }
        public Dictionary<Food, CompoundResidueCollection> CumulativeCompoundResidueCollections { get; set; }

        public ILookup<Food, FoodSample> FoodSamples { get; set; }

        public IDictionary<Food, List<ISampleOrigin>> SampleOriginInfos { get; set; }

        public ICollection<SampleCompoundCollection> MeasuredSubstanceSampleCollections { get; set; }
        public ICollection<SampleCompoundCollection> ActiveSubstanceSampleCollections { get; set; }
        public ICollection<SampleCompoundCollection> MonteCarloSubstanceSampleCollections { get; set; }

        public ICollection<(Food Food, Compound Substance)> FocalCommodityCombinations { get; set; }
        public ICollection<FoodSample> FocalCommoditySamples { get; set; }
        public ICollection<SampleCompoundCollection> FocalCommoditySubstanceSampleCollections { get; set; }

        public ICollection<Food> ModelledFoods { get; set; }
        public ILookup<Food, ModelledFoodInfo> ModelledFoodInfos { get; set; }

        public IDictionary<(Food Food, Compound Substance), ConcentrationDistribution> ConcentrationDistributions { get; set; }
        public IDictionary<(Food Food, Compound Substance), SingleValueConcentrationModel> MeasuredSubstanceSingleValueConcentrations { get; set; }
        public IDictionary<(Food Food, Compound Substance), SingleValueConcentrationModel> ActiveSubstanceSingleValueConcentrations { get; set; }
        public ICollection<ConcentrationSingleValue> SingleValueConcentrations { get; set; }

        public FoodSurvey FoodSurvey { get; set; }
        public ICollection<Food> FoodsAsEaten { get; set; }
        public ICollection<PopulationConsumptionSingleValue> FoodConsumptionSingleValues { get; set; }

        // Consumptions
        public ICollection<Individual> ConsumerIndividuals { get; set; }
        public ICollection<IndividualDay> ConsumerIndividualDays { get; set; }
        public ICollection<FoodConsumption> SelectedFoodConsumptions { get; set; }

        // Consumptions by modelled food
        public ICollection<Individual> ModelledFoodConsumers { get; set; }
        public ICollection<IndividualDay> ModelledFoodConsumerDays { get; set; }
        public ICollection<ConsumptionsByModelledFood> ConsumptionsByModelledFood { get; set; }

        public ICollection<FoodTranslation> FoodRecipes { get; set; }

        public IDictionary<Food, ICollection<Food>> FoodExtrapolations { get; set; }
        public ICollection<FoodSubstanceExtrapolationCandidates> ExtrapolationCandidates { get; set; }

        public ICollection<ProcessingType> ProcessingTypes { get; set; }
        public ICollection<ProcessingFactor> ProcessingFactors { get; set; }
        public ProcessingFactorModelCollection ProcessingFactorModels { get; set; }

        public Dictionary<Food, FoodUnitVariabilityInfo> UnitVariabilityDictionary { get; set; }
        public ICollection<IestiSpecialCase> IestiSpecialCases { get; set; }

        public ICollection<FoodConversionResult> FoodConversionResults { get; set; }

        public IDictionary<(Food Food, Compound Substance), ConcentrationModel> ConcentrationModels { get; set; }
        public IDictionary<Food, ConcentrationModel> CumulativeConcentrationModels { get; set; }
        public ICollection<SingleValueConsumptionModel> SingleValueConsumptionModels { get; set; }
        public IDictionary<(Food Food, Compound Substance), ConcentrationLimit> MaximumConcentrationLimits { get; set; }

        public ICollection<NonDietaryExposureSource> NonDietaryExposureSources { get; set; }

        public ICollection<NonDietaryExposureSet> NonDietaryExposureSets { get; set; }
        public IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> NonDietaryExposures { get; set; }
        public ICollection<ExposureRouteType> NonDietaryExposureRoutes { get; set; }

        public ICollection<ExposureRouteType> ExposureRoutes { get; set; }
        public IDictionary<(ExposureRouteType, Compound), double> AbsorptionFactors { get; set; }
        public ICollection<KineticModelInstance> KineticModelInstances { get; set; }
        public ICollection<KineticAbsorptionFactor> KineticAbsorptionFactors { get; set; }

        public ICollection<HumanMonitoringSurvey> HbmSurveys { get; set; }
        public ICollection<Individual> HbmIndividuals { get; set; }
        public ICollection<HumanMonitoringSample> HbmSamples { get; set; }
        public ICollection<HumanMonitoringSamplingMethod> HbmSamplingMethods { get; set; }
        public ICollection<HumanMonitoringSampleSubstanceCollection> HbmSampleSubstanceCollections { get; set; }
        public ConcentrationUnit HbmConcentrationUnit { get; set; }

        public ICollection<HbmIndividualDayConcentration> HbmIndividualDayConcentrations { get; set; }
        public ICollection<HbmCumulativeIndividualConcentration> HbmCumulativeIndividualConcentrations { get; set; }
        public ICollection<HbmCumulativeIndividualDayConcentration> HbmCumulativeIndividualDayConcentrations { get; set; }
        public ICollection<HbmIndividualConcentration> HbmIndividualConcentrations { get; set; }

        public ILookup<Effect, EffectRepresentation> AllEffectRepresentations { get; set; }
        public ICollection<EffectRepresentation> FocalEffectRepresentations { get; set; }

        public ICollection<DoseResponseExperiment> AvailableDoseResponseExperiments { get; set; }
        public ICollection<DoseResponseExperiment> SelectedResponseExperiments { get; set; }
        public ICollection<DoseResponseModel> DoseResponseModels { get; set; }

        public ICollection<Data.Compiled.Objects.PointOfDeparture> PointsOfDeparture { get; set; }

        public IDictionary<Compound, IHazardCharacterisationModel> HazardCharacterisations { get; set; }
        public TargetUnit HazardCharacterisationsUnit { get; set; }
        public TargetUnit HbmTargetConcentrationUnit { get; set; }

        public ICollection<InterSpeciesFactor> InterSpeciesFactors { get; set; }
        public IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> InterSpeciesFactorModels { get; set; }

        public ICollection<IntraSpeciesFactor> IntraSpeciesFactors { get; set; }
        public IDictionary<(Effect, Compound), IntraSpeciesFactorModel> IntraSpeciesFactorModels { get; set; }

        public ICollection<MolecularDockingModel> MolecularDockingModels { get; set; }
        public ICollection<QsarMembershipModel> QsarMembershipModels { get; set; }
        public ICollection<ActiveSubstanceModel> AvailableActiveSubstanceModels { get; set; }
        public IDictionary<Compound, double> MembershipProbabilities { get; set; }

        public IDictionary<Compound, RelativePotencyFactor> RawRelativePotencyFactors { get; set; }
        public ICollection<SubstanceConversion> SubstanceConversions { get; set; }
        public ICollection<DeterministicSubstanceConversionFactor> DeterministicSubstanceConversionFactors { get; set; }

        public ILookup<Food, TDSFoodSampleComposition> TdsFoodCompositions { get; set; }
        public IDictionary<(Food Food, Compound Substance), double> TdsReductionFactors { get; set; }
        public ICollection<Food> TdsReductionScenarioAnalysisFoods { get; set; }

        public ScreeningResult ScreeningResult { get; set; }
        public ICollection<SimulatedIndividualDay> SimulatedIndividualDays { get; set; }

        public ICollection<DietaryIndividualDayIntake> DietaryIndividualDayIntakes { get; set; }
        public List<DietaryIndividualIntake> DietaryObservedIndividualMeans { get; set; }
        public List<DietaryIndividualIntake> DietaryModelAssistedIntakes { get; set; }
        public List<ModelAssistedIntake> DrillDownDietaryIndividualIntakes { get; set; }
        public IIntakeModel DietaryExposuresIntakeModel { get; set; }

        public ICollection<ModelBasedIntakeResult> DietaryModelBasedIntakeResults { get; set; }
        public ICollection<AggregateIndividualDayExposure> AggregateIndividualDayExposures { get; set; }
        public ICollection<AggregateIndividualExposure> AggregateIndividualExposures { get; set; }

        public ICollection<ISingleValueDietaryExposure> SingleValueDietaryExposureResults { get; set; }

        public ICollection<SingleValueRiskCalculationResult> SingleValueRiskCalculationResults { get; set; }

        public ICollection<IndividualEffect> CumulativeIndividualEffects { get; set; }

        public ICollection<MarketShare> MarketShares { get; set; }

        private Compound _cumulativeCompound;
        public Compound CumulativeCompound {
            get {
                if (_cumulativeCompound == null) {
                    _cumulativeCompound = new Compound() {
                        Code = "equivalents",
                        Name = $"_{ReferenceCompound?.Name}Eq",
                    };
                }
                return _cumulativeCompound;
            }
            set {
                _cumulativeCompound = value;
            }
        }

        public IDictionary<(Food, Compound, ProcessingType), ProcessingFactor> ProcessingFactorsDictionary { get; set; }

        /// <summary>
        /// Creates a new instance with references to the same objects as the original.
        /// </summary>
        /// <param name="resampleIndividuals">Sets whether to resample the Individuals set</param>
        /// <param name="resampleConcentrations">Sets whether to resample the concentrations set</param>
        public ActionData Copy() {
            var newDataSource = new ActionData() {
                AbsorptionFactors = this.AbsorptionFactors,
                ActiveSubstances = this.ActiveSubstances,
                ActiveSubstanceSingleValueConcentrations = this.ActiveSubstanceSingleValueConcentrations,
                ActiveSubstanceSampleCollections = this.ActiveSubstanceSampleCollections,
                AllCompounds = this.AllCompounds,
                AllEffects = this.AllEffects,
                AllEffectRepresentations = this.AllEffectRepresentations,
                AllFoods = this.AllFoods,
                AllFoodsByCode = this.AllFoodsByCode,
                SelectedPopulation = this.SelectedPopulation,
                AvailableActiveSubstanceModels = this.AvailableActiveSubstanceModels,
                AdverseOutcomePathwayNetwork = this.AdverseOutcomePathwayNetwork,
                SubstanceAuthorisations = this.SubstanceAuthorisations,

                Cofactor = this.Cofactor,
                Covariable = this.Covariable,
                ConcentrationDistributions = this.ConcentrationDistributions,
                ConcentrationModels = this.ConcentrationModels,
                ConsumerIndividuals = this.ConsumerIndividuals,
                ConsumerIndividualDays = this.ConsumerIndividualDays,
                ConsumptionsByModelledFood = this.ConsumptionsByModelledFood,
                CorrectedRelativePotencyFactors = this.CorrectedRelativePotencyFactors,
                CompoundResidueCollections = this.CompoundResidueCollections,
                CumulativeCompound = this.CumulativeCompound,
                CumulativeConcentrationModels = this.CumulativeConcentrationModels,
                CumulativeCompoundResidueCollections = this.CumulativeCompoundResidueCollections,
                CumulativeIndividualEffects = this.CumulativeIndividualEffects,

                DesiredIntakeModelType = this.DesiredIntakeModelType,
                DietaryIndividualDayIntakes = this.DietaryIndividualDayIntakes,
                DietaryObservedIndividualMeans = this.DietaryObservedIndividualMeans,
                DietaryModelAssistedIntakes = this.DietaryModelAssistedIntakes,
                DietaryExposuresIntakeModel = this.DietaryExposuresIntakeModel,
                DietaryExposureUnit = this.DietaryExposureUnit,
                DietaryModelBasedIntakeResults = this.DietaryModelBasedIntakeResults,
                DoseResponseModels = this.DoseResponseModels,
                DrillDownDietaryIndividualIntakes = this.DrillDownDietaryIndividualIntakes,
                DeterministicSubstanceConversionFactors = this.DeterministicSubstanceConversionFactors,

                EffectUnit = this.EffectUnit,
                ExternalExposureUnit = this.ExternalExposureUnit,
                ExposureRoutes = this.ExposureRoutes,

                FocalCommodityCombinations = this.FocalCommodityCombinations,
                FocalCommoditySamples = this.FocalCommoditySamples,
                FocalCommoditySubstanceSampleCollections = this.FocalCommoditySubstanceSampleCollections,
                FocalEffectRepresentations = this.FocalEffectRepresentations,
                FoodSurvey = this.FoodSurvey,
                FoodConversionResults = this.FoodConversionResults,
                FoodConsumptionSingleValues = this.FoodConsumptionSingleValues,
                FoodsAsEaten = this.FoodsAsEaten,
                FoodRecipes = this.FoodRecipes,
                FoodExtrapolations = this.FoodExtrapolations,

                HazardCharacterisations = this.HazardCharacterisations,
                HazardCharacterisationsUnit = this.HazardCharacterisationsUnit,

                HbmSamplingMethods = this.HbmSamplingMethods,
                HbmSurveys = this.HbmSurveys,
                HbmIndividuals = this.HbmIndividuals,
                HbmSamples = this.HbmSamples,
                HbmConcentrationUnit = this.HbmConcentrationUnit,
                HbmIndividualDayConcentrations = this.HbmIndividualDayConcentrations,
                HbmIndividualConcentrations = this.HbmIndividualConcentrations,
                HbmTargetConcentrationUnit = this.HbmTargetConcentrationUnit,
                HbmCumulativeIndividualConcentrations = this.HbmCumulativeIndividualConcentrations,
                HbmCumulativeIndividualDayConcentrations = this.HbmCumulativeIndividualDayConcentrations,

                IestiSpecialCases = this.IestiSpecialCases,
                InterSpeciesFactors = this.InterSpeciesFactors,
                IntraSpeciesFactors = this.IntraSpeciesFactors,
                IntraSpeciesFactorModels = this.IntraSpeciesFactorModels,
                InterSpeciesFactorModels = this.InterSpeciesFactorModels,

                KineticModelInstances = this.KineticModelInstances,
                KineticAbsorptionFactors = this.KineticAbsorptionFactors,

                MarginalOccurrencePatterns = this.MarginalOccurrencePatterns,
                MaximumConcentrationLimits = this.MaximumConcentrationLimits,
                MarketShares = this.MarketShares,
                ModelledFoodConsumers = this.ModelledFoodConsumers,
                ModelledFoodConsumerDays = this.ModelledFoodConsumerDays,
                MeasuredSubstances = this.MeasuredSubstances,
                MeasuredSubstanceSampleCollections = this.MeasuredSubstanceSampleCollections,
                MeasuredSubstanceSingleValueConcentrations = this.MeasuredSubstanceSingleValueConcentrations,
                MembershipProbabilities = this.MembershipProbabilities,
                ModelledFoods = this.ModelledFoods,
                MolecularDockingModels = this.MolecularDockingModels,
                MonteCarloSubstanceSampleCollections = this.MonteCarloSubstanceSampleCollections,

                NonDietaryExposureSources = this.NonDietaryExposureSources,
                NonDietaryExposures = this.NonDietaryExposures,
                NonDietaryExposureSets = this.NonDietaryExposureSets,
                NonDietaryExposureRoutes = this.NonDietaryExposureRoutes,

                OccurrenceFractions = this.OccurrenceFractions,

                PointsOfDeparture = this.PointsOfDeparture,
                ProcessingTypes = this.ProcessingTypes,
                ProcessingFactorModels = this.ProcessingFactorModels,
                ProcessingFactorsDictionary = this.ProcessingFactorsDictionary,
                ProcessingFactors = this.ProcessingFactors,

                QsarMembershipModels = this.QsarMembershipModels,

                RawAgriculturalUses = this.RawAgriculturalUses,
                RawRelativePotencyFactors = this.RawRelativePotencyFactors,
                ReferenceCompound = this.ReferenceCompound,
                SubstanceConversions = this.SubstanceConversions,
                RelevantEffects = this.RelevantEffects,
                Responses = this.Responses,

                SampleOriginInfos = this.SampleOriginInfos,
                FoodSamples = this.FoodSamples,
                ScreeningResult = this.ScreeningResult,
                SelectedFoodConsumptions = this.SelectedFoodConsumptions,
                SelectedResponseExperiments = this.SelectedResponseExperiments,
                SelectedEffect = this.SelectedEffect,
                SingleValueConsumptionModels = this.SingleValueConsumptionModels,
                SingleValueDietaryExposureResults = this.SingleValueDietaryExposureResults,
                SingleValueDietaryExposureUnit = this.SingleValueDietaryExposureUnit,
                SingleValueConcentrations = this.SingleValueConcentrations,
                SimulatedIndividualDays = this.SimulatedIndividualDays,

                TestSystems = this.TestSystems,
                TargetExposureUnit = this.TargetExposureUnit,

                TdsFoodCompositions = this.TdsFoodCompositions,
                TdsReductionFactors = this.TdsReductionFactors,
                TdsReductionScenarioAnalysisFoods = this.TdsReductionScenarioAnalysisFoods,

                UnitVariabilityDictionary = this.UnitVariabilityDictionary,
            };

            return newDataSource;
        }
    }
}
