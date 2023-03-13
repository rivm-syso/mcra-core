using DocumentFormat.OpenXml.Office2019.Excel.RichData2;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmConcentrationModelCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.NonDetectsImputationCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {

    [ActionType(ActionType.HumanMonitoringAnalysis)]
    public class HumanMonitoringAnalysisActionCalculator : ActionCalculatorBase<HumanMonitoringAnalysisActionResult> {

        public HumanMonitoringAnalysisActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isMultiple = _project.AssessmentSettings.MultipleSubstances;
            var isCumulative = isMultiple && _project.AssessmentSettings.Cumulative;
            var isRiskBasedMcr = _project.MixtureSelectionSettings.IsMcrAnalysis
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
            var hbmTargetUnit = new TargetUnit(
                data.HbmConcentrationUnit.GetSubstanceAmountUnit(),
                data.HbmConcentrationUnit.GetConcentrationMassUnit(),
                _project.KineticModelSettings.CodeCompartment,
                _project.AssessmentSettings.ExposureType == ExposureType.Chronic ? TimeScaleUnit.SteadyState : TimeScaleUnit.Peak
            );
            var settings = new HbmIndividualDayConcentrationsCalculatorSettings(_project.HumanMonitoringSettings);

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
            var nonDetectsImputationCalculator = new HbmNonDetectsImputationCalculator(settings);
            var imputedNonDetectsSubstanceCollection = nonDetectsImputationCalculator
                .ImputeNonDetects(
                    data.HbmSampleSubstanceCollections,
                     settings.NonDetectImputationMethod != NonDetectImputationMethod.ReplaceByLimit
                        ? concentrationModels : null,
                    new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.HBM_CensoredValueImputation))
                );

            // Impute missing values
            var missingValueImputationCalculator = HbmMissingValueImputationCalculatorFactory
                .Create(_project.HumanMonitoringSettings.MissingValueImputationMethod);
            var imputedMissingValuesSubstanceCollection = missingValueImputationCalculator
                .ImputeMissingValues(
                    imputedNonDetectsSubstanceCollection,
                    settings.MissingValueCutOff,
                    Simulation.IsBackwardCompatibilityMode
                        ? GetRandomGenerator(_project.MonteCarloSettings.RandomSeed)
                        : new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.HBM_MissingValueImputation))
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

            // TODO: get HBM concentration unit and update calculator to align concentrations
            // with the selected target unit. Reason: in the future, the data coming from the human monitoring data module
            // might be in a different unit than the desired target unit, e.g. HBM data in mg/L and target
            // unit in ug/L. When this becomes the case, the monitoringIndividualDayCalculator should be updated to account
            // for this unit conversion.
            var matrixConcentrationConversionCalculator = new SimpleBiologicalMatrixConcentrationConversionCalculator(
                conversionFactor: settings.HbmBetweenMatrixConversionFactor
            );
            var monitoringIndividualDayCalculator = new HbmIndividualDayConcentrationsCalculator(
                settings,
                matrixConcentrationConversionCalculator
            );
            var monitoringDayConcentrations = monitoringIndividualDayCalculator
                .Calculate(
                    imputedMissingValuesSubstanceCollection,
                    individualDays,
                    data.ActiveSubstances ?? data.AllCompounds,
                    _project.KineticModelSettings.CodeCompartment
                );

            //Remove all individualDays containing missing values.
            var remainingSubstances = monitoringDayConcentrations
                .SelectMany(c => c.ConcentrationsBySubstance.Keys)
                .Distinct()
                .ToList();
            var individualDayConcentrations = monitoringDayConcentrations
                .Select(individualDay => {
                    var individualDaySubstances = individualDay.ConcentrationsBySubstance.Keys.ToList();
                    return remainingSubstances.Except(individualDaySubstances).Count() == 0 ? individualDay : null;
                })
                .Where(c => c != null)
                .ToList();

            List<HbmIndividualConcentration> individualConcentrations = null;
            List<HbmCumulativeIndividualConcentration> cumulativeIndividualConcentrations = null;
            List<HbmCumulativeIndividualDayConcentration> cumulativeIndividualDayConcentrations = null;

            if (_project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                // For chronic assessments, compute average individual concentrations
                var individualConcentrationsCalculator = new HbmIndividualConcentrationsCalculator();
                individualConcentrations = individualConcentrationsCalculator
                    .Calculate(
                        data.ActiveSubstances,
                        individualDayConcentrations
                    );
                if (data.CorrectedRelativePotencyFactors != null) {
                    // For cumulative assessments, compute cumulative individual concentrations
                    var hbmCumulativeIndividualCalculator = new HbmCumulativeIndividualConcentrationCalculator();
                    cumulativeIndividualConcentrations = hbmCumulativeIndividualCalculator.Calculate(
                        individualConcentrations,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors
                    );
                }
            } else if (_project.AssessmentSettings.ExposureType == ExposureType.Acute
                && data.CorrectedRelativePotencyFactors != null
            ) {
                // For cumulative assessments, compute cumulative individual day concentrations
                var hbmCumulativeIndividualDayCalculator = new HbmCumulativeIndividualDayConcentrationCalculator();
                cumulativeIndividualDayConcentrations = hbmCumulativeIndividualDayCalculator.Calculate(
                    individualDayConcentrations,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors
                );
            }
            var result = new HumanMonitoringAnalysisActionResult();
            if (_project.MixtureSelectionSettings.IsMcrAnalysis
                && data.ActiveSubstances.Count > 1
            ) {
                var exposureMatrixBuilder = new ExposureMatrixBuilder(
                    data.ActiveSubstances,
                    data.ReferenceCompound == null ? data.ActiveSubstances.ToDictionary(r => r, r => 1D) : data.CorrectedRelativePotencyFactors,
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
            result.HbmTargetUnit = hbmTargetUnit;
            result.HbmCumulativeIndividualConcentrations = cumulativeIndividualConcentrations;
            result.HbmCumulativeIndividualDayConcentrations = cumulativeIndividualDayConcentrations;
            result.HbmConcentrationModels = concentrationModels;
            return result;
        }

        protected override void updateSimulationData(ActionData data, HumanMonitoringAnalysisActionResult result) {
            data.HbmIndividualDayConcentrations = result.HbmIndividualDayConcentrations;
            data.HbmIndividualConcentrations = result.HbmIndividualConcentrations;
            data.HbmTargetConcentrationUnit = result.HbmTargetUnit;
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
