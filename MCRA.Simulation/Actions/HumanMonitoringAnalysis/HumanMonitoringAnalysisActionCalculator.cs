using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.General.UnitDefinitions.Enums;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmConcentrationModelCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.NonDetectsImputationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {

    [ActionType(ActionType.HumanMonitoringAnalysis)]
    public class HumanMonitoringAnalysisActionCalculator : ActionCalculatorBase<HumanMonitoringAnalysisActionResult> {

        public HumanMonitoringAnalysisActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isMultiple = _project.AssessmentSettings.MultipleSubstances;
            var isCumulative = isMultiple && _project.AssessmentSettings.Cumulative;
            var isRiskBasedMcr = isMultiple && _project.MixtureSelectionSettings.IsMcrAnalysis
                && _project.MixtureSelectionSettings.McrExposureApproachType == ExposureApproachType.RiskBased;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = isCumulative || isRiskBasedMcr;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = isCumulative || isRiskBasedMcr;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new HumanMonitoringAnalysisSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new HumanMonitoringAnalysisSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override HumanMonitoringAnalysisActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var hbmConcentrationUnit = data.HbmConcentrationUnit;
            var settings = new HumanMonitoringAnalysisModuleSettings(_project);

            // Create HBM concentration models
            var concentrationModelsBuilder = new HbmConcentrationModelBuilder();
            var concentrationModels = settings.NonDetectImputationMethod != NonDetectImputationMethod.ReplaceByLimit
                ? concentrationModelsBuilder.Create(
                    data.HbmSampleSubstanceCollections,
                    settings.NonDetectsHandlingMethod,
                    settings.LorReplacementFactor
                )
                : null;

            // Imputation of censored values
            var nonDetectsImputationCalculator = new HbmNonDetectsImputationCalculator(
                settings.NonDetectImputationMethod,
                settings.NonDetectsHandlingMethod,
                settings.LorReplacementFactor
            );
            var imputedNonDetectsSubstanceCollection = nonDetectsImputationCalculator
                .ImputeNonDetects(
                    data.HbmSampleSubstanceCollections,
                    settings.NonDetectImputationMethod != NonDetectImputationMethod.ReplaceByLimit ? concentrationModels : null,
                    RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.HBM_CensoredValueImputation)
                );

            // Impute missing values
            var missingValueImputationCalculator = HbmMissingValueImputationCalculatorFactory
                .Create(_project.HumanMonitoringSettings.MissingValueImputationMethod);
            var imputedMissingValuesSubstanceCollection = missingValueImputationCalculator
                .ImputeMissingValues(
                    imputedNonDetectsSubstanceCollection,
                    settings.MissingValueCutOff,
                    RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.HBM_MissingValueImputation)
                );

            // TODO: should we create HBM individual days collection in HBM data module?
            var individualDays = data.HbmSampleSubstanceCollections
                .SelectMany(r => r.HumanMonitoringSampleSubstanceRecords.Select(r => (r.Individual, r.Day)))
                .Distinct()
                .Select(r => new IndividualDay() {
                    Individual = r.Individual,
                    IdDay = r.Day
                })
                .ToList();

            var compartmentUnitCollector = new CompartmentUnitCollector(
                settings.ExposureType == ExposureType.Chronic
                    ? TimeScaleUnit.SteadyState
                    : TimeScaleUnit.Peak
                );
            var standardisedSubstanceCollection = imputedMissingValuesSubstanceCollection;

            // Standardize blood concentrations (express soluble substances per lipid content)
            if (settings.StandardiseBlood) {
                var lipidContentCorrector = BloodCorrectionCalculatorFactory.Create(settings.StandardiseBloodMethod);
                standardisedSubstanceCollection = lipidContentCorrector
                    .ComputeTotalLipidCorrection(
                        imputedMissingValuesSubstanceCollection,
                        hbmConcentrationUnit,
                        _project.HumanMonitoringSettings.TargetMatrix,
                        compartmentUnitCollector
                    );
            }

            // Normalise by specific gravity or standardise by creatinine concentration
            if (settings.StandardiseUrine) {
                var urineCorrectorCalculator = UrineCorrectionCalculatorFactory.Create(settings.StandardiseUrineMethod);
                standardisedSubstanceCollection = urineCorrectorCalculator
                    .ComputeResidueCorrection(
                        standardisedSubstanceCollection,
                        hbmConcentrationUnit,
                        _project.HumanMonitoringSettings.TargetMatrix,
                        compartmentUnitCollector
                    );
            }

            if (!settings.StandardiseBlood && !settings.StandardiseUrine) {
                compartmentUnitCollector.EnsureUnit(
                    hbmConcentrationUnit.GetSubstanceAmountUnit(),
                    hbmConcentrationUnit.GetConcentrationMassUnit(),
                    _project.HumanMonitoringSettings.TargetMatrix
                );
            }

            // Apply matrix concentration conversion
            // TODO: get HBM concentration unit and update calculator to align concentrations
            // with the selected target unit. Reason: in the future, the data coming from the human monitoring data module
            // might be in a different unit than the desired target unit, e.g. HBM data in mg/L and target
            // unit in ug/L. When this becomes the case, the monitoringIndividualDayCalculator should be updated to account
            // for this unit conversion.
            var matrixConcentrationConversionCalculator = new SimpleBiologicalMatrixConcentrationConversionCalculator(
                conversionFactor: settings.HbmBetweenMatrixConversionFactor
            );
            var monitoringIndividualDayCalculator = new HbmIndividualDayConcentrationsCalculator(
                settings.ImputeHbmConcentrationsFromOtherMatrices,
                matrixConcentrationConversionCalculator
            );
            var monitoringDayConcentrations = monitoringIndividualDayCalculator
                .Calculate(
                    standardisedSubstanceCollection,
                    individualDays,
                    data.ActiveSubstances ?? data.AllCompounds,
                    _project.HumanMonitoringSettings.TargetMatrix
                );

            // Remove all individualDays containing missing values.
            var remainingSubstances = monitoringDayConcentrations
                .SelectMany(c => c.ConcentrationsBySubstance.Keys)
                .Distinct()
                .ToList();
            var individualDayConcentrations = monitoringDayConcentrations
                .Select(individualDay => {
                    var individualDaySubstances = individualDay.ConcentrationsBySubstance.Keys.ToList();
                    return !remainingSubstances.Except(individualDaySubstances).Any() ? individualDay : null;
                })
                .Where(c => c != null)
                .ToList();

            // Throw an exception if we all individual days were removed due to missing substance concentrations
            if (!individualDayConcentrations.Any()) {
                throw new Exception("All HBM individual day records were removed for having non-imputed missing substance concentrations.");
            }

            // For chronic assessments, compute average individual concentrations
            List<HbmIndividualConcentration> individualConcentrations = null;
            if (settings.ExposureType == ExposureType.Chronic) {
                var individualConcentrationsCalculator = new HbmIndividualConcentrationsCalculator();
                individualConcentrations = individualConcentrationsCalculator
                    .Calculate(
                        data.ActiveSubstances,
                        individualDayConcentrations
                    );
            }

            // Compute cumulative concentrations
            List<HbmCumulativeIndividualConcentration> cumulativeIndividualConcentrations = null;
            List<HbmCumulativeIndividualDayConcentration> cumulativeIndividualDayConcentrations = null;
            if (data.CorrectedRelativePotencyFactors != null) {
                if (settings.ExposureType == ExposureType.Chronic) {
                    // For cumulative assessments, compute cumulative individual concentrations
                    var hbmCumulativeIndividualCalculator = new HbmCumulativeIndividualConcentrationCalculator();
                    cumulativeIndividualConcentrations = hbmCumulativeIndividualCalculator.Calculate(
                        individualConcentrations,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors
                    );
                } else {
                    // For cumulative assessments, compute cumulative individual day concentrations
                    var hbmCumulativeIndividualDayCalculator = new HbmCumulativeIndividualDayConcentrationCalculator();
                    cumulativeIndividualDayConcentrations = hbmCumulativeIndividualDayCalculator.Calculate(
                        individualDayConcentrations,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors
                    );
                }
            }

            var result = new HumanMonitoringAnalysisActionResult();
            if (_project.MixtureSelectionSettings.IsMcrAnalysis
                && data.ActiveSubstances.Count > 1
            ) {
                var exposureMatrixBuilder = new ExposureMatrixBuilder(
                    data.ActiveSubstances,
                    data.ReferenceSubstance == null ? data.ActiveSubstances.ToDictionary(r => r, r => 1D) : data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    _project.AssessmentSettings.ExposureType,
                    _project.SubsetSettings.IsPerPerson,
                    _project.MixtureSelectionSettings.McrExposureApproachType
                 );
                result.ExposureMatrix = exposureMatrixBuilder.Compute(individualDayConcentrations, individualConcentrations);
                result.DriverSubstances = DriverSubstanceCalculator.CalculateExposureDrivers(result.ExposureMatrix);
            }
            localProgress.Update(100);
            result.HbmIndividualDayConcentrations = individualDayConcentrations;
            result.HbmIndividualConcentrations = individualConcentrations;
            result.HbmTargetUnits = compartmentUnitCollector.CollectedTargetUnits;
            result.HbmCumulativeIndividualConcentrations = cumulativeIndividualConcentrations;
            result.HbmCumulativeIndividualDayConcentrations = cumulativeIndividualDayConcentrations;
            result.HbmConcentrationModels = concentrationModels;
            return result;
        }

        protected override void updateSimulationData(ActionData data, HumanMonitoringAnalysisActionResult result) {
            data.HbmIndividualDayConcentrations = result.HbmIndividualDayConcentrations;
            data.HbmIndividualConcentrations = result.HbmIndividualConcentrations;
            data.HbmTargetConcentrationUnits = result.HbmTargetUnits;
            data.HbmCumulativeIndividualConcentrations = result.HbmCumulativeIndividualConcentrations;
            data.HbmCumulativeIndividualDayConcentrations = result.HbmCumulativeIndividualDayConcentrations;
        }

        protected override void summarizeActionResult(HumanMonitoringAnalysisActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing human monitoring analysis results", 0);
            var summarizer = new HumanMonitoringAnalysisSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
