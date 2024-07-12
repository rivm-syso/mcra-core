using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DietaryExposures {

    public sealed class DietaryExposuresSettingsSummarizer : ActionModuleSettingsSummarizer<DietaryExposuresModuleConfig> {
        public DietaryExposuresSettingsSummarizer(DietaryExposuresModuleConfig config) : base(config) {
        }

        public override ActionType ActionType => ActionType.DietaryExposures;

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project ) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.SelectedTier, _configuration.SelectedTier);
            section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType);

            if (_configuration.ExposureType == ExposureType.Acute) {
                section.SummarizeSetting(SettingsItemType.NumberOfMonteCarloIterations, _configuration.NumberOfMonteCarloIterations);
                if (_configuration.IsSurveySampling) {
                    section.SummarizeSetting(SettingsItemType.IsSurveySampling, _configuration.IsSurveySampling);
                }
                section.SummarizeSetting("Apply unit variability", _configuration.UseUnitVariability);
                if (_configuration.UseUnitVariability) {
                    section.SubSections.Add(SummarizeUnitVariabilitySettings());
                }
                section.SummarizeSetting(SettingsItemType.IsSampleBased, _configuration.IsSampleBased);
                section.SummarizeSetting(SettingsItemType.IsSingleSamplePerDay, _configuration.IsSingleSamplePerDay);
                if (_configuration.IsSampleBased) {
                    section.SummarizeSetting(SettingsItemType.DefaultConcentrationModel, _configuration.DefaultConcentrationModel);
                } else {
                    section.SummarizeSetting(SettingsItemType.MaximiseCoOccurrenceHighResidues, _configuration.MaximiseCoOccurrenceHighResidues);
                    section.SummarizeSetting(SettingsItemType.NonDetectsHandlingMethod, _configuration.NonDetectsHandlingMethod);
                    section.SummarizeSetting(SettingsItemType.UseOccurrencePatternsForResidueGeneration, _configuration.UseOccurrencePatternsForResidueGeneration);
                }
                section.SummarizeSetting(SettingsItemType.ImputeExposureDistributions, _configuration.ImputeExposureDistributions);
                if (_configuration.IntakeCovariateModelling) {
                    section.SummarizeSetting(SettingsItemType.IntakeCovariateModelling, _configuration.IntakeCovariateModelling);
                }
                section.SummarizeSetting(SettingsItemType.IntakeModelType, _configuration.IntakeModelType, isVisible: false);
            }

            if (_configuration.IntakeFirstModelThenAdd) {
                section.SummarizeSetting(SettingsItemType.IntakeFirstModelThenAdd, _configuration.IntakeFirstModelThenAdd);
                section.SummarizeSetting(SettingsItemType.IsufModelNumberOfIterations, _configuration.NumberOfMonteCarloIterations, isVisible: false);
            }

            if (_configuration.TotalDietStudy) {
                section.SummarizeSetting(SettingsItemType.TotalDietStudy, _configuration.TotalDietStudy);
            } else {
                section.SummarizeSetting(SettingsItemType.IsProcessing, _configuration.IsProcessing);
            }

            section.SummarizeSetting(SettingsItemType.UseReadAcrossFoodTranslations, _configuration.UseReadAcrossFoodTranslations);
            section.SummarizeSetting(SettingsItemType.DietaryExposuresDetailsLevel, _configuration.DietaryExposuresDetailsLevel);

            if (_configuration.ExposureType == ExposureType.Chronic || _configuration.IntakeCovariateModelling) {
                section.SummarizeSetting(SettingsItemType.ReductionToLimitScenario, _configuration.ReductionToLimitScenario);
                section.SummarizeSetting(SettingsItemType.ImputeExposureDistributions, _configuration.ImputeExposureDistributions);
                section.SubSections.Add(SummarizeIntakeModelSettings());
                section.SummarizeSetting(SettingsItemType.IntakeCovariateModelling, _configuration.IntakeCovariateModelling);
                if ((_configuration.IntakeModelType != IntakeModelType.OIM
                    && _configuration.IntakeModelType != IntakeModelType.ISUF)
                    || _configuration.IntakeFirstModelThenAdd
                ) {
                    section.SubSections.Add(SummarizeAmountModelSettings());
                    section.SubSections.Add(SummarizeFrequencyModelSettings());
                } else {
                    section.SummarizeSetting(SettingsItemType.FrequencyModelDispersion, _configuration.FrequencyModelDispersion, isVisible: false);
                    section.SummarizeSetting(SettingsItemType.AmountModelVarianceRatio, _configuration.AmountModelVarianceRatio, isVisible: false);
                    section.SummarizeSetting(SettingsItemType.AmountModelTransformType, _configuration.AmountModelTransformType, isVisible: false);
                    section.SummarizeSetting(SettingsItemType.IsufModelNumberOfIterations, _configuration.NumberOfMonteCarloIterations, isVisible: false);
                }
                if (_configuration.IntakeModelType == IntakeModelType.LNN0 || _configuration.IntakeModelType == IntakeModelType.LNN) {
                    section.SummarizeSetting(SettingsItemType.Cumulative, _configuration.Cumulative, isVisible: false);
                }
            }
            section.SummarizeSetting(SettingsItemType.McrAnalysis, _configuration.McrAnalysis);
            if (_configuration.McrAnalysis) {
                section.SummarizeSetting(SettingsItemType.McrExposureApproachType, _configuration.McrExposureApproachType);
                section.SummarizeSetting(SettingsItemType.McrPlotRatioCutOff, _configuration.McrPlotRatioCutOff);
                section.SummarizeSetting(SettingsItemType.McrPlotPercentiles, _configuration.McrPlotPercentiles);
                section.SummarizeSetting(SettingsItemType.McrPlotMinimumPercentage, _configuration.McrPlotMinimumPercentage);
            }
            section.SummarizeSetting(SettingsItemType.VariabilityDiagnosticsAnalysis, _configuration.VariabilityDiagnosticsAnalysis);
            return section;
        }

        public ActionSettingsSummary SummarizeIntakeModelSettings() {
            var section = new ActionSettingsSummary("Intake model");
            if (_configuration.ExposureType == ExposureType.Chronic && _configuration.IntakeFirstModelThenAdd) {
                section.SummarizeSetting(SettingsItemType.IntakeModelType, "Model-then-add");
                var idx = 0;
                foreach (var intakeModel in _configuration.IntakeModelsPerCategory) {
                    section.SummarizeSetting("Model " + idx, intakeModel.ModelType.GetDisplayAttribute().Name + " (transformation: " + intakeModel.TransformType.GetDisplayAttribute().Name + ")");
                    idx++;
                }
            } else if (_configuration.ExposureType == ExposureType.Chronic || _configuration.IntakeCovariateModelling) {
                if (_configuration.ExposureType == ExposureType.Chronic) {
                    section.SummarizeSetting(SettingsItemType.IntakeCovariateModelling, _configuration.IntakeCovariateModelling);
                }
                section.SummarizeSetting(SettingsItemType.IntakeModelType, _configuration.IntakeModelType);
                if (_configuration.IntakeModelType != IntakeModelType.OIM) {
                    section.SummarizeSetting(SettingsItemType.AmountModelTransformType, _configuration.AmountModelTransformType);
                }
                if (_configuration.IntakeModelType == IntakeModelType.ISUF) {
                    section.SummarizeSetting(SettingsItemType.IsufModelGridPrecision, _configuration.IsufModelGridPrecision);
                    section.SummarizeSetting(SettingsItemType.IsufModelSplineFit, _configuration.IsufModelSplineFit);
                }
            }
            if (_configuration.ExposureType == ExposureType.Chronic && _configuration.IntakeModelType != IntakeModelType.OIM) {
                section.SummarizeSetting(SettingsItemType.NumberOfMonteCarloIterations, _configuration.NumberOfMonteCarloIterations);
            }
            return section;
        }

        public ActionSettingsSummary SummarizeAmountModelSettings() {
            var section = new ActionSettingsSummary("Amount model");
            var cofactorName = _configuration.NameCofactor;
            var covariableName = _configuration.NameCovariable;
            var modelDefinition = string.Empty;
            switch (_configuration.AmountModelCovariateModelType) {
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
            if (_configuration.AmountModelCovariateModelType != CovariateModelType.Constant &&
                _configuration.AmountModelCovariateModelType != CovariateModelType.Cofactor
            ) {
                section.SummarizeSetting(SettingsItemType.AmountModelFunctionType, _configuration.AmountModelFunctionType);
                section.SummarizeSetting(SettingsItemType.AmountModelMaxDegreesOfFreedom, _configuration.AmountModelMaxDegreesOfFreedom);
                section.SummarizeSetting(SettingsItemType.AmountModelMinDegreesOfFreedom, _configuration.AmountModelMinDegreesOfFreedom);
                section.SummarizeSetting(SettingsItemType.AmountModelTestingLevel, _configuration.AmountModelTestingLevel);
                section.SummarizeSetting(SettingsItemType.AmountModelTestingMethod, _configuration.AmountModelTestingMethod);
            }
            section.SummarizeSetting("Cofactor name", _configuration.NameCofactor);
            section.SummarizeSetting("Covariable name", _configuration.NameCovariable);
            section.SummarizeSetting(SettingsItemType.FrequencyModelDispersion, _configuration.FrequencyModelDispersion, isVisible: false);
            section.SummarizeSetting(SettingsItemType.AmountModelVarianceRatio, _configuration.AmountModelVarianceRatio, isVisible: false);
            return section;
        }

        public ActionSettingsSummary SummarizeFrequencyModelSettings() {
            var section = new ActionSettingsSummary("Frequency model");
            var cofactorName = _configuration.NameCofactor;
            var covariableName = _configuration.NameCovariable;
            var modelDefinition = string.Empty;
            switch (_configuration.FrequencyModelCovariateModelType) {
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
            if (_configuration.FrequencyModelCovariateModelType != CovariateModelType.Constant &&
                _configuration.FrequencyModelCovariateModelType != CovariateModelType.Cofactor
            ) {
                section.SummarizeSetting(SettingsItemType.FrequencyModelFunctionType, _configuration.FrequencyModelFunctionType);
                section.SummarizeSetting(SettingsItemType.FrequencyModelMaxDegreesOfFreedom, _configuration.FrequencyModelMaxDegreesOfFreedom);
                section.SummarizeSetting(SettingsItemType.FrequencyModelMinDegreesOfFreedom, _configuration.FrequencyModelMinDegreesOfFreedom);
                section.SummarizeSetting(SettingsItemType.FrequencyModelTestingLevel, _configuration.FrequencyModelTestingLevel);
                section.SummarizeSetting(SettingsItemType.FrequencyModelTestingMethod, _configuration.FrequencyModelTestingMethod);
            }
            return section;
        }

        public ActionSettingsSummary SummarizeUnitVariabilitySettings() {
            var section = new ActionSettingsSummary("Unit variability");
            section.SummarizeSetting(SettingsItemType.UnitVariabilityModel, _configuration.UnitVariabilityModel);
            section.SummarizeSetting(SettingsItemType.EstimatesNature, _configuration.EstimatesNature);
            if (_configuration.UseUnitVariability) {
                section.SummarizeSetting(SettingsItemType.DefaultFactorLow, _configuration.DefaultFactorLow);
                section.SummarizeSetting(SettingsItemType.DefaultFactorMid, _configuration.DefaultFactorMid);
                switch (_configuration.UnitVariabilityModel) {
                    case UnitVariabilityModelType.BetaDistribution:
                        section.SummarizeSetting(SettingsItemType.UnitVariabilityType, _configuration.UnitVariabilityType);
                        break;
                    case UnitVariabilityModelType.LogNormalDistribution:
                        section.SummarizeSetting(SettingsItemType.UnitVariabilityType, _configuration.UnitVariabilityType);
                        section.SummarizeSetting(SettingsItemType.MeanValueCorrectionType, _configuration.MeanValueCorrectionType);
                        break;
                    case UnitVariabilityModelType.BernoulliDistribution:
                        break;
                    default:
                        break;
                }
            }
            section.SummarizeSetting(SettingsItemType.CorrelationType, _configuration.CorrelationType);
            return section;
        }
    }
}
