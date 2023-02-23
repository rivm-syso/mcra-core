using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.ActiveSubstanceAllocation;
using MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation;
using MCRA.Simulation.Calculators.FoodExtrapolationsCalculation;
using MCRA.Simulation.Calculators.WaterConcentrationsExtrapolation;

namespace MCRA.Simulation.Actions.Concentrations {
    public sealed class ConcentrationsModuleSettings :
        IFocalCommodityMeasurementReplacementCalculatorFactorySettings, 
        IActiveSubstanceAllocationSettings,
        IFoodExtrapolationCandidatesCalculatorSettings,
        IWaterConcentrationsExtrapolationCalculatorSettings {

        private readonly ProjectDto _project;

        public ConcentrationsModuleSettings(ProjectDto project) {
            _project = project;

            // TODO: replace hit summarizer settings
            _ = _project.ConcentrationModelSettings.ConcentrationsTier;

        }

        // Sample filters/subsets

        public bool FilterConcentrationLimitExceedingSamples {
            get {
                return _project.ConcentrationModelSettings.FilterConcentrationLimitExceedingSamples;
            }
        }

        public double ConcentrationLimitFilterFractionExceedanceThreshold {
            get {
                return _project.ConcentrationModelSettings.ConcentrationLimitFilterFractionExceedanceThreshold;
            }
        }

        public bool IsSamplePropertySubset {
            get {
                return _project.SubsetSettings.SampleSubsetSelection;
            }
        }

        public LocationSubsetDefinitionDto LocationSubsetDefinition {
            get {
                return _project.LocationSubsetDefinition;
            }
        }

        public PeriodSubsetDefinitionDto PeriodSubsetDefinition {
            get {
                return _project.PeriodSubsetDefinition;
            }
        }

        public SamplesSubsetDefinitionDto RegionSubsetDefinition {
            get {
                return _project.SamplesSubsetDefinitions
                    .FirstOrDefault(r => r.IsRegionSubset());
            }
        }

        public SamplesSubsetDefinitionDto ProductionMethodSubsetDefinition {
            get {
                return _project.SamplesSubsetDefinitions
                    .FirstOrDefault(r => r.IsProductionMethodSubset());
            }
        }

        public List<SamplesSubsetDefinitionDto> AdditionalSamplePropertySubsetDefinitions {
            get {
                return _project.SamplesSubsetDefinitions
                    .Where(r => !r.IsRegionSubset() && !r.IsProductionMethodSubset())
                    .ToList();
            }
        }

        // Focal food concentrations

        public bool FocalCommodity {
            get {
                return _project.AssessmentSettings.FocalCommodity;
            }
        }

        public List<FocalFoodDto> FocalFoods {
            get {
                return _project.FocalFoods;
            }
        }

        public FocalCommodityReplacementMethod FocalCommodityReplacementMethod {
            get {
                return _project.ConcentrationModelSettings.FocalCommodityReplacementMethod;
            }
        }

        public bool IsFocalCommodityMeasurementReplacement() {
            return FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue
                || FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
                || FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.MeasurementRemoval;
        }

        public double FocalCommodityScenarioOccurrencePercentage {
            get {
                return _project.ConcentrationModelSettings.FocalCommodityScenarioOccurrencePercentage;
            }
        }

        public double FocalCommodityConcentrationAdjustmentFactor {
            get {
                return _project.ConcentrationModelSettings.FocalCommodityConcentrationAdjustmentFactor;
            }
        }

        public bool UseDeterministicSubstanceConversionsForFocalCommodity {
            get {
                return _project.ConcentrationModelSettings.UseDeterministicSubstanceConversionsForFocalCommodity;
            }
        }

        // Concentration extrapolation

        public bool ExtrapolateConcentrations {
            get {
                return _project.ConcentrationModelSettings.ExtrapolateConcentrations;
            }
        }

        public int ThresholdForExtrapolation {
            get {
                return _project.ConcentrationModelSettings.ThresholdForExtrapolation;
            }
        }

        public bool ConsiderAuthorisationsForExtrapolations {
            get {
                return _project.ConcentrationModelSettings.ConsiderAuthorisationsForExtrapolations;
            }
        }

        public bool ConsiderMrlForExtrapolations {
            get {
                return _project.ConcentrationModelSettings.ConsiderMrlForExtrapolations;
            }
        }

        // Water imputation

        public bool ImputeWaterConcentrations {
            get {
                return _project.ConcentrationModelSettings.ImputeWaterConcentrations;
            }
        }

        public double WaterConcentrationValue {
            get {
                return _project.ConcentrationModelSettings.WaterConcentrationValue;
            }
        }

        public string CodeWater {
            get {
                return _project.ConcentrationModelSettings.CodeWater;
            }
        }

        public bool RestrictWaterImputationToAuthorisedUses {
            get {
                return _project.ConcentrationModelSettings.RestrictWaterImputationToAuthorisedUses;
            }
        }

        public bool RestrictWaterImputationToMostPotentSubstances {
            get {
                return _project.ConcentrationModelSettings.RestrictWaterImputationToMostPotentSubstances;
            }
        }

        // Active substance allocation

        public bool UseComplexResidueDefinitions {
            get {
                return _project.ConcentrationModelSettings.UseComplexResidueDefinitions;
            }
        }

        public SubstanceTranslationAllocationMethod ReplacementMethod {
            get {
                return _project.ConcentrationModelSettings.SubstanceTranslationAllocationMethod;
            }
        }

        public bool UseSubstanceAuthorisations {
            get {
                return _project.ConcentrationModelSettings.ConsiderAuthorisationsForSubstanceConversion;
            }
        }

        public bool RetainAllAllocatedSubstancesAfterAllocation {
            get {
                return _project.ConcentrationModelSettings.RetainAllAllocatedSubstancesAfterAllocation;
            }
        }

        public bool TryFixDuplicateAllocationInconsistencies { 
            get {
                return _project.ConcentrationModelSettings.TryFixDuplicateAllocationInconsistencies;
            }
        }
    }
}

