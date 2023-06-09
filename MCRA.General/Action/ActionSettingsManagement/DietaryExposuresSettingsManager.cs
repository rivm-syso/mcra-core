using MCRA.General.Action.Settings.Dto;
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

        public override void Verify(ProjectDto project) {
        }

        public void SetTier(ProjectDto project, DietaryIntakeCalculationTier tier, bool cascadeInputTiers) {
            SetTier(project, tier.ToString(), cascadeInputTiers);
        }

        protected override string getTierSelectionEnumName() {
            return "DietaryIntakeCalculationTier";
        }

        protected override void setTierSelectionEnumSetting(ProjectDto project, string idTier) {
            if (Enum.TryParse(idTier, out DietaryIntakeCalculationTier tier)) {
                project.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier = tier;
            }
        }

        protected override void setSetting(ProjectDto project, SettingsItemType settingsItem, string rawValue) {
            switch (settingsItem) {
                case SettingsItemType.ExposureType:
                    Enum.TryParse(rawValue, out ExposureType exposureType);
                    project.AssessmentSettings.ExposureType = exposureType;
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
                    Enum.TryParse(rawValue, out UnitVariabilityModelType unitVariabilityModel);
                    project.UnitVariabilitySettings.UnitVariabilityModel = unitVariabilityModel;
                    break;
                case SettingsItemType.UnitVariabilityType:
                    Enum.TryParse(rawValue, out UnitVariabilityType unitVariabilityType);
                    project.UnitVariabilitySettings.UnitVariabilityType = unitVariabilityType;
                    break;
                case SettingsItemType.EstimatesNature:
                    Enum.TryParse(rawValue, out EstimatesNature estimatesNature);
                    project.UnitVariabilitySettings.EstimatesNature = estimatesNature;
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
                    Enum.TryParse(rawValue, out DietaryExposuresDetailsLevel dietaryExposuresDetailsLevel);
                    project.DietaryIntakeCalculationSettings.DietaryExposuresDetailsLevel = dietaryExposuresDetailsLevel;
                    break;
                case SettingsItemType.IsPerPerson:
                    project.SubsetSettings.IsPerPerson = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.IntakeModelType:
                    Enum.TryParse(rawValue, out IntakeModelType intakeModelType);
                    project.IntakeModelSettings.IntakeModelType = intakeModelType;
                    break;
                case SettingsItemType.FirstModelThenAdd:
                    project.IntakeModelSettings.FirstModelThenAdd = parseBoolSetting(rawValue);
                    break;
                case SettingsItemType.CovariateModelType:
                    Enum.TryParse(rawValue, out CovariateModelType covariateModelType);
                    project.FrequencyModelSettings.CovariateModelType = covariateModelType;
                    break;
                case SettingsItemType.FrequencyModelCovariateModelType:
                    Enum.TryParse(rawValue, out CovariateModelType frequencyModelCovariateModelType);
                    project.FrequencyModelSettings.CovariateModelType = frequencyModelCovariateModelType;
                    break;
                case SettingsItemType.TotalDietStudy:
                    project.AssessmentSettings.TotalDietStudy = parseBoolSetting(rawValue);
                    break;
                default:
                    throw new Exception($"Error: {settingsItem} not defined for module {ActionType}.");
            }
        }

        public static List<DietaryIntakeCalculationTier> AvailableTiers(ProjectDto project) {
            var result = new List<DietaryIntakeCalculationTier>();
            if (!project.AssessmentSettings.TotalDietStudy) {
                result.Add(DietaryIntakeCalculationTier.EfsaOptimistic);
                result.Add(DietaryIntakeCalculationTier.EfsaPessimisticAcute);
                result.Add(DietaryIntakeCalculationTier.EfsaPessimisticChronic);
                result.Add(DietaryIntakeCalculationTier.Ec2018DietaryCraAcuteTier1);
                result.Add(DietaryIntakeCalculationTier.Ec2018DietaryCraAcuteTier2);
                result.Add(DietaryIntakeCalculationTier.Ec2018DietaryCraChronicTier1);
                result.Add(DietaryIntakeCalculationTier.Ec2018DietaryCraChronicTier2);
                result.Add(DietaryIntakeCalculationTier.Efsa2022DietaryCraAcuteTier1);
                result.Add(DietaryIntakeCalculationTier.Efsa2022DietaryCraAcuteTier2);
                result.Add(DietaryIntakeCalculationTier.Efsa2022DietaryCraChronicTier1);
                result.Add(DietaryIntakeCalculationTier.Efsa2022DietaryCraChronicTier2);

            }
            if (project.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier == DietaryIntakeCalculationTier.EfsaPessimistic) {
                result.Add(DietaryIntakeCalculationTier.EfsaPessimistic);
            }
            if (project.AssessmentSettings.ExposureType == ExposureType.Acute) {
                //AvailableDietaryIntakeCalculationTiers.Add(DietaryIntakeCalculationTier.IESTI);
            }
            result.Add(DietaryIntakeCalculationTier.Custom);
            return result;
        }

        public static List<IntakeModelType> AvailableIntakeModelTypes(ProjectDto project) {
            var result = new List<IntakeModelType>();
            result.Add(IntakeModelType.BBN);
            result.Add(IntakeModelType.LNN0);
            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                result.Add(IntakeModelType.LNN);
                result.Add(IntakeModelType.OIM);
                result.Add(IntakeModelType.ISUF);
            }
            return result;
        }
    }
}
