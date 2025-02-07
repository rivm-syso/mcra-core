using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
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

        private readonly ConcentrationsModuleConfig _moduleConfig;

        public ConcentrationsModuleSettings(ConcentrationsModuleConfig config) {
            _moduleConfig = config;

            // TODO: replace hit summarizer settings
            _ = _moduleConfig.SelectedTier;

        }

        // Sample filters/subsets

        public bool FilterConcentrationLimitExceedingSamples {
            get {
                return _moduleConfig.FilterConcentrationLimitExceedingSamples;
            }
        }

        public double ConcentrationLimitFilterFractionExceedanceThreshold {
            get {
                return _moduleConfig.ConcentrationLimitFilterFractionExceedanceThreshold;
            }
        }

        public bool IsSamplePropertySubset {
            get {
                return _moduleConfig.SampleSubsetSelection;
            }
        }

        public LocationSubsetDefinition LocationSubsetDefinition {
            get {
                return _moduleConfig.LocationSubsetDefinition;
            }
        }

        public PeriodSubsetDefinition PeriodSubsetDefinition {
            get {
                return _moduleConfig.PeriodSubsetDefinition;
            }
        }

        public SamplesSubsetDefinition RegionSubsetDefinition {
            get {
                return _moduleConfig.SamplesSubsetDefinitions
                    .FirstOrDefault(r => r.IsRegionSubset);
            }
        }

        public SamplesSubsetDefinition ProductionMethodSubsetDefinition {
            get {
                return _moduleConfig.SamplesSubsetDefinitions
                    .FirstOrDefault(r => r.IsProductionMethodSubset);
            }
        }

        public List<SamplesSubsetDefinition> AdditionalSamplePropertySubsetDefinitions {
            get {
                return _moduleConfig.SamplesSubsetDefinitions
                    .Where(r => !r.IsRegionSubset && !r.IsProductionMethodSubset)
                    .ToList();
            }
        }

        // Focal food concentrations

        public bool FocalCommodity {
            get {
                return _moduleConfig.FocalCommodity;
            }
        }

        public List<FocalFood> FocalFoods {
            get {
                return _moduleConfig.FocalFoods;
            }
        }

        public FocalCommodityReplacementMethod FocalCommodityReplacementMethod {
            get {
                return _moduleConfig.FocalCommodityReplacementMethod;
            }
        }

        public bool IsFocalCommodityMeasurementReplacement => _moduleConfig.IsFocalCommodityMeasurementReplacement;

        public double FocalCommodityScenarioOccurrencePercentage {
            get {
                return _moduleConfig.FocalCommodityScenarioOccurrencePercentage;
            }
        }

        public double FocalCommodityConcentrationAdjustmentFactor {
            get {
                return _moduleConfig.FocalCommodityConcentrationAdjustmentFactor;
            }
        }

        public bool UseDeterministicSubstanceConversionsForFocalCommodity {
            get {
                return _moduleConfig.UseDeterministicSubstanceConversionsForFocalCommodity;
            }
        }

        public bool FilterProcessedFocalCommoditySamples {
            get {
                return _moduleConfig.FilterProcessedFocalCommoditySamples;
            }
        }

        public double FocalCommodityProposedConcentrationLimit {
            get {
                return _moduleConfig.FocalCommodityProposedConcentrationLimit;
            }
        }

        public bool FocalCommodityIncludeProcessedDerivatives => _moduleConfig.FocalCommodityIncludeProcessedDerivatives;

        // Concentration extrapolation

        public bool ExtrapolateConcentrations {
            get {
                return _moduleConfig.ExtrapolateConcentrations;
            }
        }

        public int ThresholdForExtrapolation {
            get {
                return _moduleConfig.ThresholdForExtrapolation;
            }
        }

        public bool ConsiderAuthorisationsForExtrapolations {
            get {
                return _moduleConfig.ConsiderAuthorisationsForExtrapolations;
            }
        }

        public bool ConsiderMrlForExtrapolations {
            get {
                return _moduleConfig.ConsiderMrlForExtrapolations;
            }
        }

        // Water imputation

        public bool ImputeWaterConcentrations {
            get {
                return _moduleConfig.ImputeWaterConcentrations;
            }
        }

        public double WaterConcentrationValue {
            get {
                return _moduleConfig.WaterConcentrationValue;
            }
        }

        public string CodeWater {
            get {
                return _moduleConfig.CodeWater;
            }
        }

        public bool RestrictWaterImputationToAuthorisedUses {
            get {
                return _moduleConfig.RestrictWaterImputationToAuthorisedUses;
            }
        }

        public bool RestrictWaterImputationToMostPotentSubstances {
            get {
                return _moduleConfig.RestrictWaterImputationToMostPotentSubstances;
            }
        }

        public bool RestrictWaterImputationToApprovedSubstances {
            get {
                return _moduleConfig.RestrictWaterImputationToApprovedSubstances;
            }
        }

        // Active substance allocation

        public bool UseComplexResidueDefinitions {
            get {
                return _moduleConfig.UseComplexResidueDefinitions;
            }
        }

        public SubstanceTranslationAllocationMethod ReplacementMethod {
            get {
                return _moduleConfig.SubstanceTranslationAllocationMethod;
            }
        }

        public bool UseSubstanceAuthorisations {
            get {
                return _moduleConfig.ConsiderAuthorisationsForSubstanceConversion;
            }
        }

        public bool RetainAllAllocatedSubstancesAfterAllocation {
            get {
                return _moduleConfig.RetainAllAllocatedSubstancesAfterAllocation;
            }
        }

        public bool TryFixDuplicateAllocationInconsistencies {
            get {
                return _moduleConfig.TryFixDuplicateAllocationInconsistencies;
            }
        }
    }
}

