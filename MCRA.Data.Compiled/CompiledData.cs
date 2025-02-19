using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Compiled {

    /// <summary>
    /// This class contains all data structures for the compiled data source manager and writer
    /// </summary>
    public class CompiledData {

        public IDictionary<SourceTableGroup, HashSet<string>> Scope { get; set; }

        public IDictionary<string, Food> AllFoods { get; set; }
        public IDictionary<string, Food> AllFocalCommodityFoods { get; set; }
        public IDictionary<string, Facet> AllFacets { get; set; }
        public IDictionary<string, FacetDescriptor> AllFacetDescriptors { get; set; }
        public IDictionary<string, FoodFacet> AllFoodFacets { get; set; }
        public IList<FoodTranslation> AllFoodTranslations { get; set; }
        public IDictionary<string, Compound> AllSubstances { get; set; }
        public ICollection<SubstanceAuthorisation> AllSubstanceAuthorisations { get; set; }
        public ICollection<SubstanceApproval> AllSubstanceApprovals { get; set; }
        public ICollection<OccurrencePattern> AllOccurrencePatterns { get; set; }
        public ICollection<OccurrenceFrequency> AllOccurrenceFrequencies { get; set; }
        public IDictionary<string, Effect> AllEffects { get; set; }
        public IDictionary<string, DietaryExposureModel> AllDietaryExposureModels { get; set; }
        public IDictionary<string, TargetExposureModel> AllTargetExposureModels { get; set; }
        public IDictionary<string, RiskModel> AllRiskModels { get; set; }
        public IList<EffectRepresentation> AllEffectRepresentations { get; set; }
        public IList<ConcentrationLimit> AllMaximumConcentrationLimits { get; set; }
        public IList<SubstanceConversion> AllSubstanceConversions { get; set; }
        public IDictionary<string, ActiveSubstanceModel> AllActiveSubstanceModels { get; set; }
        public IDictionary<string, FoodSurvey> AllFoodSurveys { get; set; }
        public IDictionary<string, Individual> AllIndividuals { get; set; }
        public IDictionary<string, IndividualProperty> AllDietaryIndividualProperties { get; set; }
        public IList<FoodConsumption> AllFoodConsumptions { get; set; }
        public IDictionary<string, IndividualProperty> AllPopulationIndividualProperties { get; set; }
        public IDictionary<string, AnalyticalMethod> AllAnalyticalMethods { get; set; }
        public IDictionary<string, AnalyticalMethod> AllFocalFoodAnalyticalMethods { get; set; }
        public IDictionary<string, AnalyticalMethod> AllHumanMonitoringAnalyticalMethods { get; set; }
        public IDictionary<string, FoodSample> AllFoodSamples { get; set; }
        public ICollection<string> AllSampleLocations { get; set; }
        public ICollection<string> AllSampleRegions { get; set; }
        public ICollection<string> AllSampleProductionMethods { get; set; }
        public ICollection<int> AllSampleYears { get; set; }
        public IDictionary<string, SampleProperty> AllAdditionalSampleProperties { get; set; }
        public IDictionary<string, SampleProperty> AllFocalSampleProperties { get; set; }
        public IDictionary<string, FoodSample> AllFocalFoodSamples { get; set; }
        public IList<Objects.PointOfDeparture> AllPointsOfDeparture { get; set; }
        public IList<HazardCharacterisation> AllHazardCharacterisations { get; set; }
        public IDictionary<string, List<RelativePotencyFactor>> AllRelativePotencyFactors { get; set; }
        public IDictionary<string, ProcessingType> AllProcessingTypes { get; set; }
        public IList<ProcessingFactor> AllProcessingFactors { get; set; }
        public IList<ProcessingFactor> DefaultProcessingFactors { get; set; }
        public IDictionary<string, NonDietaryExposureSource> AllNonDietaryExposureSources { get; set; }
        public IList<NonDietaryExposureSet> NonDietaryExposureSets { get; set; }
        public IList<TDSFoodSampleComposition> AllTDSFoodSampleCompositions { get; set; }
        public IDictionary<Food, ICollection<Food>> AllFoodExtrapolations { get; set; }
        public IList<ConcentrationDistribution> AllConcentrationDistributions { get; set; }
        public IDictionary<string, TestSystem> AllTestSystems { get; set; }
        public IDictionary<string, Response> AllResponses { get; set; }
        public IDictionary<string, DoseResponseExperiment> AllDoseResponseExperiments { get; set; }
        public IDictionary<string, DoseResponseModel> AllDoseResponseModels { get; set; }
        public ICollection<InterSpeciesFactor> AllInterSpeciesFactors { get; set; }
        public ICollection<IntraSpeciesFactor> AllIntraSpeciesFactors { get; set; }
        public IDictionary<string, AdverseOutcomePathwayNetwork> AllAdverseOutcomePathwayNetworks { get; set; }
        public ICollection<HumanMonitoringSamplingMethod> HumanMonitoringSamplingMethods { get; set; }
        public IDictionary<string, HumanMonitoringSurvey> AllHumanMonitoringSurveys { get; set; }
        public IDictionary<string, Individual> AllHumanMonitoringIndividuals { get; set; }
        public IDictionary<string, IndividualProperty> AllHumanMonitoringIndividualProperties { get; set; }
        public IDictionary<string, HumanMonitoringSample> AllHumanMonitoringSamples { get; set; }
        public IList<KineticModelInstance> AllKineticModelInstances { get; set; }
        public IList<KineticConversionFactor> AllKineticConversionFactors { get; set; }
        public IList<SimpleAbsorptionFactor> AllAbsorptionFactors { get; set; }
        public IDictionary<string, MolecularDockingModel> AllMolecularDockingModels { get; set; }
        public IDictionary<string, QsarMembershipModel> AllQsarMembershipModels { get; set; }
        public List<UnitVariabilityFactor> AllUnitVariabilityFactors { get; set; }
        public List<IestiSpecialCase> AllIestiSpecialCases { get; set; }
        public IDictionary<string, Population> AllPopulations { get; set; }
        public IList<MarketShare> AllMarketShares { get; set; }
        public IList<DeterministicSubstanceConversionFactor> AllDeterministicSubstanceConversionFactors { get; set; }
        public IList<PopulationConsumptionSingleValue> AllPopulationConsumptionSingleValues { get; set; }
        public IList<ConcentrationSingleValue> AllConcentrationSingleValues { get; set; }
        public ICollection<ExposureBiomarkerConversion> AllExposureBiomarkerConversions { get; set; }
        public IDictionary<string, ExposureScenario> AllSingleValueNonDietaryExposureScenarios { get; set; }
        public IDictionary<string, ExposureDeterminantCombination> AllSingleValueNonDietaryExposureDeterminantCombinations { get; set; }
        public IList<ExposureEstimate> AllSingleValueNonDietaryExposureEstimates { get; set; }
        public IList<DustConcentrationDistribution> AllDustConcentrationDistributions { get; set; }
        public IList<DustIngestion> AllDustIngestions { get; set; }
        public IList<DustBodyExposureFraction> AllDustBodyExposureFractions { get; set; }
        public IList<DustAdherenceAmount> AllDustAdherenceAmounts { get; set; }
        public IList<DustAvailabilityFraction> AllDustAvailabilityFractions { get; set; }
        public IList<ExposureEffectFunction> AllExposureEffectFunctions { get; set; }
        public IDictionary<string, PbkModelDefinition> AllPbkModelDefinitions { get; set; }
        public IList<SoilConcentrationDistribution> AllSoilConcentrationDistributions { get; set; }
        public IList<SoilIngestion> AllSoilIngestions { get; set; }
        public IList<BaselineBodIndicator> AllBaselineBodIndicators { get; set; }
        #region Methods

        public Food GetOrAddFood(string id, string name = null) {
            if (!AllFoods.TryGetValue(id, out Food item)) {
                item = new Food { Code = id, Name = name };
                AllFoods.Add(id, item);
            }
            return item;
        }

        public NonDietaryExposureSource GetOrAddNonDietaryExposureSource(string id, string name = null) {
            if (!AllNonDietaryExposureSources.TryGetValue(id, out var item)) {
                item = new NonDietaryExposureSource { Code = id, Name = name };
                AllNonDietaryExposureSources.Add(id, item);
            }
            return item;
        }

        public Compound GetOrAddSubstance(string id, string name = null) {
            if (!AllSubstances.TryGetValue(id, out Compound item)) {
                item = new Compound { Code = id, Name = name };
                AllSubstances.Add(id, item);
            }
            return item;
        }

        public Effect GetOrAddEffect(string id, string name = null) {
            if (!AllEffects.TryGetValue(id, out Effect item)) {
                item = new Effect { Code = id, Name = name };
                AllEffects.Add(id, item);
            }
            return item;
        }

        public FoodSurvey GetOrAddFoodSurvey(string id, string description = null) {
            if (!AllFoodSurveys.TryGetValue(id, out FoodSurvey item)) {
                item = new FoodSurvey { Code = id, Description = description };
                AllFoodSurveys.Add(id, item);
            }
            return item;
        }

        public TestSystem GetOrAddTestSystem(string id, string name = null) {
            if (!AllTestSystems.TryGetValue(id, out TestSystem item)) {
                item = new TestSystem { Code = id, Name = name };
                AllTestSystems.Add(id, item);
            }
            return item;
        }

        public ProcessingType GetOrAddProcessingType(string id, string name = null) {
            if (!AllProcessingTypes.TryGetValue(id, out ProcessingType item)) {
                item = new ProcessingType { Code = id, Name = name };
                AllProcessingTypes.Add(id, item);
            }
            return item;
        }

        public Population GetOrAddPopulation(string id, string name = null) {
            if (!AllPopulations.TryGetValue(id, out Population item)) {
                item = new Population { Code = id, Name = name };
                AllPopulations.Add(id, item);
            }
            return item;
        }

        public HumanMonitoringSurvey GetOrAddHumanMonitoringSurvey(string id, string name = null) {
            if (!AllHumanMonitoringSurveys.TryGetValue(id, out HumanMonitoringSurvey item)) {
                item = new HumanMonitoringSurvey { Code = id, Name = name };
                AllHumanMonitoringSurveys.Add(id, item);
            }
            return item;
        }

        #endregion
    }
}
