using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.DietaryExposures {

    public sealed class DietaryExposuresSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.DietaryExposures;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType);
            section.SummarizeSetting(SettingsItemType.DietaryIntakeCalculationTier, project.DietaryIntakeCalculationSettings.DietaryIntakeCalculationTier);

            if (project.AssessmentSettings.ExposureType == ExposureType.Acute) {
                var mcs = project.MonteCarloSettings;
                section.SummarizeSetting(SettingsItemType.NumberOfMonteCarloIterations, mcs.NumberOfMonteCarloIterations);
                if (mcs.IsSurveySampling) {
                    section.SummarizeSetting(SettingsItemType.IsSurveySampling, mcs.IsSurveySampling);
                }
                section.SummarizeSetting("Apply unit variability", project.UnitVariabilitySettings.UseUnitVariability);
                if (project.UnitVariabilitySettings.UseUnitVariability) {
                    section.SubSections.Add(SummarizeUnitVariabilitySettings(project));
                }
                section.SummarizeSetting(SettingsItemType.IsSampleBased, project.ConcentrationModelSettings.IsSampleBased);
                section.SummarizeSetting(SettingsItemType.IsSingleSamplePerDay, project.ConcentrationModelSettings.IsSingleSamplePerDay);
                if (project.ConcentrationModelSettings.IsSampleBased) {
                    section.SummarizeSetting(SettingsItemType.DefaultConcentrationModel, project.ConcentrationModelSettings.DefaultConcentrationModel);
                } else {
                    section.SummarizeSetting(SettingsItemType.IsCorrelation, project.ConcentrationModelSettings.IsCorrelation);
                    section.SummarizeSetting(SettingsItemType.NonDetectsHandlingMethod, project.ConcentrationModelSettings.NonDetectsHandlingMethod);
                    section.SummarizeSetting(SettingsItemType.UseOccurrencePatternsForResidueGeneration, project.AgriculturalUseSettings.UseOccurrencePatternsForResidueGeneration);
                }
                section.SummarizeSetting(SettingsItemType.ImputeExposureDistributions, project.DietaryIntakeCalculationSettings.ImputeExposureDistributions);
                if (project.IntakeModelSettings.CovariateModelling) {
                    section.SummarizeSetting(SettingsItemType.CovariateModelling, project.IntakeModelSettings.CovariateModelling);
                }
                section.SummarizeSetting(SettingsItemType.IntakeModelType, project.IntakeModelSettings.IntakeModelType, isVisible: false);
            }

            if (project.IntakeModelSettings.FirstModelThenAdd) {
                section.SummarizeSetting(SettingsItemType.FirstModelThenAdd, project.IntakeModelSettings.FirstModelThenAdd);
                section.SummarizeSetting(SettingsItemType.NumberOfIterations, project.MonteCarloSettings.NumberOfMonteCarloIterations, isVisible: false);
            }

            if (project.AssessmentSettings.TotalDietStudy) {
                section.SummarizeSetting(SettingsItemType.TotalDietStudy, project.AssessmentSettings.TotalDietStudy);
            } else {
                section.SummarizeSetting(SettingsItemType.IsProcessing, project.ConcentrationModelSettings.IsProcessing);
            }

            section.SummarizeSetting(SettingsItemType.UseReadAcrossFoodTranslations, project.ConversionSettings.UseReadAcrossFoodTranslations);
            section.SummarizeSetting(SettingsItemType.DietaryExposuresDetailsLevel, project.DietaryIntakeCalculationSettings.DietaryExposuresDetailsLevel);

            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic || project.IntakeModelSettings.CovariateModelling) {
                section.SummarizeSetting(SettingsItemType.UseScenario, project.ScenarioAnalysisSettings.UseScenario);
                section.SummarizeSetting(SettingsItemType.ImputeExposureDistributions, project.DietaryIntakeCalculationSettings.ImputeExposureDistributions);
                section.SubSections.Add(SummarizeIntakeModelSettings(project));
                section.SummarizeSetting(SettingsItemType.CovariateModelling, project.IntakeModelSettings.CovariateModelling);
                if ((project.IntakeModelSettings.IntakeModelType != IntakeModelType.OIM
                    && project.IntakeModelSettings.IntakeModelType != IntakeModelType.ISUF)
                    || project.IntakeModelSettings.FirstModelThenAdd
                ) {
                    section.SubSections.Add(SummarizeAmountModelSettings(project));
                    section.SubSections.Add(SummarizeFrequencyModelSettings(project));
                } else {
                    section.SummarizeSetting(SettingsItemType.Dispersion, project.IntakeModelSettings.Dispersion, isVisible: false);
                    section.SummarizeSetting(SettingsItemType.VarianceRatio, project.IntakeModelSettings.VarianceRatio, isVisible: false);
                    section.SummarizeSetting(SettingsItemType.VarianceRatio, project.IntakeModelSettings.TransformType, isVisible: false);
                    section.SummarizeSetting(SettingsItemType.NumberOfIterations, project.MonteCarloSettings.NumberOfMonteCarloIterations, isVisible: false);
                }
                if (project.IntakeModelSettings.IntakeModelType == IntakeModelType.LNN0 || project.IntakeModelSettings.IntakeModelType == IntakeModelType.LNN) {
                    section.SummarizeSetting(SettingsItemType.Cumulative, project.AssessmentSettings.Cumulative, isVisible: false);
                }
            }
            section.SummarizeSetting(SettingsItemType.IsMcrAnalysis, project.MixtureSelectionSettings.IsMcrAnalysis);
            if (project.MixtureSelectionSettings.IsMcrAnalysis) {
                section.SummarizeSetting(SettingsItemType.McrExposureApproachType, project.MixtureSelectionSettings.McrExposureApproachType);
            }
            section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioPercentiles, project.OutputDetailSettings.MaximumCumulativeRatioPercentiles);
            section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioMinimumPercentage, project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage);
            section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, project.EffectSettings.TargetDoseLevelType, isVisible: false);
            section.SummarizeSetting(SettingsItemType.VariabilityDiagnosticsAnalysis, project.DietaryIntakeCalculationSettings.VariabilityDiagnosticsAnalysis);
            return section;
        }

        public ActionSettingsSummary SummarizeIntakeModelSettings(ProjectDto project) {
            var section = new ActionSettingsSummary("Intake model");
            var ass = project.AssessmentSettings;
            var ims = project.IntakeModelSettings;
            if (ass.ExposureType == ExposureType.Chronic && ims.FirstModelThenAdd) {
                section.SummarizeSetting(SettingsItemType.IntakeModelType, "Model-then-add");
                var idx = 0;
                foreach (var intakeModel in ims.IntakeModelsPerCategory) {
                    section.SummarizeSetting("Model " + idx, intakeModel.ModelType.GetDisplayAttribute().Name + " (transformation: " + intakeModel.TransformType.GetDisplayAttribute().Name + ")");
                    idx++;
                }
            } else if (ass.ExposureType == ExposureType.Chronic || ims.CovariateModelling) {
                if (ass.ExposureType == ExposureType.Chronic) {
                    section.SummarizeSetting(SettingsItemType.CovariateModelling, ims.CovariateModelling);
                }
                section.SummarizeSetting(SettingsItemType.IntakeModelType, ims.IntakeModelType);
                if (ims.IntakeModelType != IntakeModelType.OIM) {
                    section.SummarizeSetting(SettingsItemType.TransformType, ims.TransformType);
                }
                if (ims.IntakeModelType == IntakeModelType.ISUF) {
                    section.SummarizeSetting(SettingsItemType.GridPrecision, ims.GridPrecision);
                    section.SummarizeSetting(SettingsItemType.SplineFit, ims.SplineFit);
                }
            }
            if (ass.ExposureType == ExposureType.Chronic && ims.IntakeModelType != IntakeModelType.OIM) {
                section.SummarizeSetting(SettingsItemType.NumberOfMonteCarloIterations, project.MonteCarloSettings.NumberOfMonteCarloIterations);
            }
            return section;
        }

        public ActionSettingsSummary SummarizeAmountModelSettings(ProjectDto project) {
            var section = new ActionSettingsSummary("Amount model");
            var ams = project.AmountModelSettings;
            var cofactorName = project.CovariatesSelectionSettings.NameCofactor;
            var covariableName = project.CovariatesSelectionSettings.NameCovariable;
            var modelDefinition = string.Empty;
            switch (ams.CovariateModelType) {
                case CovariateModelType.Constant:
                    modelDefinition = "constant";
                    break;
                case CovariateModelType.Covariable:
                    modelDefinition = covariableName;
                    break;
                case CovariateModelType.Cofactor:
                    modelDefinition = cofactorName;
                    break;
                case CovariateModelType.CovariableCofactor:
                    modelDefinition = $"{covariableName} + {cofactorName}";
                    break;
                case CovariateModelType.CovariableCofactorInteraction:
                    modelDefinition = $"{covariableName} * {cofactorName}";
                    break;
            }
            section.SummarizeSetting("Amounts model", modelDefinition);
            if (ams.CovariateModelType != CovariateModelType.Constant && ams.CovariateModelType != CovariateModelType.Cofactor) {
                section.SummarizeSetting(SettingsItemType.FunctionType, ams.FunctionType);
                section.SummarizeSetting(SettingsItemType.MaxDegreesOfFreedom, ams.MaxDegreesOfFreedom);
                section.SummarizeSetting(SettingsItemType.MinDegreesOfFreedom, ams.MinDegreesOfFreedom);
                section.SummarizeSetting(SettingsItemType.TestingLevel, ams.TestingLevel);
                section.SummarizeSetting(SettingsItemType.TestingMethod, ams.TestingMethod);
            }
            section.SummarizeSetting("Cofactor name", project.CovariatesSelectionSettings.NameCofactor);
            section.SummarizeSetting("Covariable name", project.CovariatesSelectionSettings.NameCovariable);
            section.SummarizeSetting(SettingsItemType.Dispersion, project.IntakeModelSettings.Dispersion, isVisible: false);
            section.SummarizeSetting(SettingsItemType.VarianceRatio, project.IntakeModelSettings.VarianceRatio, isVisible: false);
            return section;
        }

        public ActionSettingsSummary SummarizeFrequencyModelSettings(ProjectDto project) {
            var section = new ActionSettingsSummary("Frequency model");
            var fms = project.FrequencyModelSettings;
            var cofactorName = project.CovariatesSelectionSettings.NameCofactor;
            var covariableName = project.CovariatesSelectionSettings.NameCovariable;
            var modelDefinition = string.Empty;
            switch (fms.CovariateModelType) {
                case CovariateModelType.Constant:
                    modelDefinition = "constant";
                    break;
                case CovariateModelType.Covariable:
                    modelDefinition = covariableName;
                    break;
                case CovariateModelType.Cofactor:
                    modelDefinition = cofactorName;
                    break;
                case CovariateModelType.CovariableCofactor:
                    modelDefinition = $"{covariableName} + {cofactorName}";
                    break;
                case CovariateModelType.CovariableCofactorInteraction:
                    modelDefinition = $"{covariableName} * {cofactorName}";
                    break;
            }
            section.SummarizeSetting("Frequency model", modelDefinition);
            if (fms.CovariateModelType != CovariateModelType.Constant && fms.CovariateModelType != CovariateModelType.Cofactor) {
                section.SummarizeSetting(SettingsItemType.FrequencyModelFunctionType, fms.FunctionType);
                section.SummarizeSetting(SettingsItemType.FrequencyModelMaxDegreesOfFreedom, fms.MaxDegreesOfFreedom);
                section.SummarizeSetting(SettingsItemType.FrequencyModelMinDegreesOfFreedom, fms.MinDegreesOfFreedom);
                section.SummarizeSetting(SettingsItemType.FrequencyModelTestingLevel, fms.TestingLevel);
                section.SummarizeSetting(SettingsItemType.TestingMethod, fms.TestingMethod);
            }
            return section;
        }

        public ActionSettingsSummary SummarizeUnitVariabilitySettings(ProjectDto project) {
            var section = new ActionSettingsSummary("Unit variability");
            var uvs = project.UnitVariabilitySettings;
            section.SummarizeSetting(SettingsItemType.UnitVariabilityModel, uvs.UnitVariabilityModel);
            section.SummarizeSetting(SettingsItemType.EstimatesNature, uvs.EstimatesNature);
            if (uvs.UseUnitVariability) {
                section.SummarizeSetting(SettingsItemType.DefaultFactorLow, uvs.DefaultFactorLow);
                section.SummarizeSetting(SettingsItemType.DefaultFactorMid, uvs.DefaultFactorMid);
                switch (uvs.UnitVariabilityModel) {
                    case UnitVariabilityModelType.BetaDistribution:
                        section.SummarizeSetting(SettingsItemType.UnitVariabilityType, uvs.UnitVariabilityType);
                        break;
                    case UnitVariabilityModelType.LogNormalDistribution:
                        section.SummarizeSetting(SettingsItemType.UnitVariabilityType, uvs.UnitVariabilityType);
                        section.SummarizeSetting(SettingsItemType.MeanValueCorrectionType, uvs.MeanValueCorrectionType);
                        break;
                    case UnitVariabilityModelType.BernoulliDistribution:
                        break;
                    default:
                        break;
                }
            }
            section.SummarizeSetting(SettingsItemType.CorrelationType, uvs.CorrelationType);
            return section;
        }
    }
}
