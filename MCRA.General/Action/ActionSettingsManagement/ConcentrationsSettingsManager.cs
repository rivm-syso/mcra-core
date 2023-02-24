using MCRA.General.Action.Settings.Dto;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ConcentrationsSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.Concentrations;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
        }

        public override void Verify(ProjectDto project) {
            SetTier(project, project.ConcentrationModelSettings.ConcentrationsTier, false);
        }

        public void SetTier(ProjectDto project, ConcentrationsTier tier, bool cascadeInputTiers) {
            SetTier(project, tier.ToString(), cascadeInputTiers);
        }

        protected override string getTierSelectionEnumName() {
            return "ConcentrationsTier";
        }

        protected override void setTierSelectionEnumSetting(ProjectDto project, string idTier) {
            if (Enum.TryParse(idTier, out ConcentrationsTier tier)) {
                project.ConcentrationModelSettings.ConcentrationsTier = tier;
            }
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.UseComplexResidueDefinitions:
                    project.ConcentrationModelSettings.UseComplexResidueDefinitions = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.SubstanceTranslationAllocationMethod:
                    Enum.TryParse(rawValue, out SubstanceTranslationAllocationMethod substanceTranslationAllocationMethod);
                    project.ConcentrationModelSettings.SubstanceTranslationAllocationMethod = substanceTranslationAllocationMethod;
                    break;
                case SettingsItemType.RetainAllAllocatedSubstancesAfterAllocation:
                    project.ConcentrationModelSettings.RetainAllAllocatedSubstancesAfterAllocation = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.ConsiderAuthorisationsForSubstanceConversion:
                    project.ConcentrationModelSettings.ConsiderAuthorisationsForSubstanceConversion = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.TryFixDuplicateAllocationInconsistencies:
                    project.ConcentrationModelSettings.TryFixDuplicateAllocationInconsistencies = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.ExtrapolateConcentrations:
                    project.ConcentrationModelSettings.ExtrapolateConcentrations = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.ThresholdForExtrapolation:
                    project.ConcentrationModelSettings.ThresholdForExtrapolation = parseIntSetting(rawValue);
                    break;
                case SettingsItemType.ConsiderMrlForExtrapolations:
                    project.ConcentrationModelSettings.ConsiderMrlForExtrapolations = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.ConsiderAuthorisationsForExtrapolations:
                    project.ConcentrationModelSettings.ConsiderAuthorisationsForExtrapolations = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.ImputeWaterConcentrations:
                    project.ConcentrationModelSettings.ImputeWaterConcentrations = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.CodeWater:
                    project.ConcentrationModelSettings.CodeWater = rawValue;
                    break;
                case SettingsItemType.WaterConcentrationValue:
                    project.ConcentrationModelSettings.WaterConcentrationValue = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.RestrictWaterImputationToMostPotentSubstances:
                    project.ConcentrationModelSettings.RestrictWaterImputationToMostPotentSubstances = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.RestrictWaterImputationToAuthorisedUses:
                    project.ConcentrationModelSettings.RestrictWaterImputationToAuthorisedUses = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.RestrictWaterImputationToApprovedSubstances:
                    project.ConcentrationModelSettings.RestrictWaterImputationToApprovedSubstances = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.ReSampleConcentrations:
                    project.UncertaintyAnalysisSettings.ReSampleConcentrations = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.FilterConcentrationLimitExceedingSamples:
                    project.ConcentrationModelSettings.FilterConcentrationLimitExceedingSamples = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.ConcentrationLimitFilterFractionExceedanceThreshold:
                    project.ConcentrationModelSettings.ConcentrationLimitFilterFractionExceedanceThreshold = parseDoubleSetting(rawValue);
                    break;
                case SettingsItemType.FocalCommodity:
                    project.AssessmentSettings.FocalCommodity = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.FocalCommodityReplacementMethod:
                    Enum.TryParse(rawValue, out FocalCommodityReplacementMethod focalCommodityReplacementMethod);
                    project.ConcentrationModelSettings.FocalCommodityReplacementMethod = focalCommodityReplacementMethod;
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for this module.");
            }
        }
    }
}
