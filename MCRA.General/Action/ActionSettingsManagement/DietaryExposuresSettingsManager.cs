using MCRA.General.Action.Settings;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class DietaryExposuresSettingsManager : ActionSettingsManagerBase {

        public override ActionType ActionType => ActionType.DietaryExposures;

        public override void initializeSettings(ProjectDto project) {
            Verify(project);
            SetTier(project, project.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier, false);
            var cumulative = project.AssessmentSettings.MultipleSubstances && project.AssessmentSettings.Cumulative;
            project.EffectSettings.RestrictToAvailableHazardDoses = cumulative;
            project.AddCalculationAction(ActionType.Populations);
            if (cumulative) {
                project.AddCalculationAction(ActionType.RelativePotencyFactors);
            }
            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic && project.AssessmentSettings.TotalDietStudy) {
                project.ConversionSettings.UseComposition = false;
            }
            project.AddCalculationAction(ActionType.OccurrencePatterns);
            project.AddCalculationAction(ActionType.OccurrenceFrequencies);
            project.AddCalculationAction(ActionType.ActiveSubstances);
        }

        public override SettingsTemplateType GetTier(ProjectDto project) => project.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier;

        public override void Verify(ProjectDto project) {
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.DietaryIntakeCalculationTier:
                    project.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier = Enum.Parse<SettingsTemplateType>(rawValue, true);
                    break;
                case SettingsItemType.ExposureType:
                    project.AssessmentSettings.ExposureType = Enum.Parse<ExposureType>(rawValue, true);
                    break;
                case SettingsItemType.MultipleSubstances:
                    project.AssessmentSettings.MultipleSubstances = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.Cumulative:
                    project.AssessmentSettings.Cumulative = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IsSampleBased:
                    project.ConcentrationModelSettings.IsSampleBased = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IsCorrelation:
                    project.ConcentrationModelSettings.IsCorrelation = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IsSurveySampling:
                    project.MonteCarloSettings.IsSurveySampling = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.NumberOfMonteCarloIterations:
                    project.MonteCarloSettings.NumberOfMonteCarloIterations = parseIntSetting(rawValue);
                    break;
                case SettingsItemType.IsProcessing:
                    project.ConcentrationModelSettings.IsProcessing = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IsDistribution:
                    project.ConcentrationModelSettings.IsDistribution = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.AllowHigherThanOne:
                    project.ConcentrationModelSettings.AllowHigherThanOne = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.UseUnitVariability:
                    project.UnitVariabilitySettings.UseUnitVariability = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.UnitVariabilityModel:
                    project.UnitVariabilitySettings.UnitVariabilityModel = Enum.Parse<UnitVariabilityModelType>(rawValue, true);
                    break;
                case SettingsItemType.UnitVariabilityType:
                    project.UnitVariabilitySettings.UnitVariabilityType = Enum.Parse<UnitVariabilityType>(rawValue, true);
                    break;
                case SettingsItemType.EstimatesNature:
                    project.UnitVariabilitySettings.EstimatesNature = Enum.Parse<EstimatesNature>(rawValue, true);
                    break;
                case SettingsItemType.DefaultFactorLow:
                    project.UnitVariabilitySettings.DefaultFactorLow = parseIntSetting(rawValue);
                    break;
                case SettingsItemType.DefaultFactorMid:
                    project.UnitVariabilitySettings.DefaultFactorMid = parseIntSetting(rawValue);
                    break;
                case SettingsItemType.CovariateModelling:
                    project.IntakeModelSettings.CovariateModelling = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IsSingleSamplePerDay:
                    project.ConcentrationModelSettings.IsSingleSamplePerDay = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.UseOccurrencePatternsForResidueGeneration:
                    project.AgriculturalUseSettings.UseOccurrencePatternsForResidueGeneration = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.ImputeExposureDistributions:
                    project.DietaryIntakeCalculationSettings.ImputeExposureDistributions = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.DietaryExposuresDetailsLevel:
                    project.DietaryIntakeCalculationSettings.DietaryExposuresDetailsLevel = Enum.Parse<DietaryExposuresDetailsLevel>(rawValue, true);
                    break;
                case SettingsItemType.IsPerPerson:
                    project.SubsetSettings.IsPerPerson = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IntakeModelType:
                    project.IntakeModelSettings.IntakeModelType = Enum.Parse<IntakeModelType>(rawValue, true);
                    break;
                case SettingsItemType.FirstModelThenAdd:
                    project.IntakeModelSettings.FirstModelThenAdd = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.CovariateModelType:
                    project.FrequencyModelSettings.CovariateModelType = Enum.Parse<CovariateModelType>(rawValue, true);
                    break;
                case SettingsItemType.FrequencyModelCovariateModelType:
                    project.FrequencyModelSettings.CovariateModelType = Enum.Parse<CovariateModelType>(rawValue, true);
                    break;
                case SettingsItemType.TotalDietStudy:
                    project.AssessmentSettings.TotalDietStudy = parseBoolSetting(rawValue);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }

        public static List<SettingsTemplateType> AvailableTiers(ProjectDto project) {
            var result = new List<SettingsTemplateType>();
            if (!project.AssessmentSettings.TotalDietStudy) {
                result = McraTemplatesCollection.Instance.GetModuleTemplate(ActionType.DietaryExposures)
                    .Values.Where(v => !v.Deprecated)
                    .Select(v => v.Tier).ToList();
            }
            if (project.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier == SettingsTemplateType.EfsaPessimistic) {
                result.Add(SettingsTemplateType.EfsaPessimistic);
            }
            result.Add(SettingsTemplateType.Custom);
            return result;
        }

        public static List<IntakeModelType> AvailableIntakeModelTypes(ProjectDto project) {
            var result = new List<IntakeModelType> {
                IntakeModelType.BBN,
                IntakeModelType.LNN0
            };
            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                result.Add(IntakeModelType.LNN);
                result.Add(IntakeModelType.OIM);
                result.Add(IntakeModelType.ISUF);
            }
            return result;
        }
    }
}
