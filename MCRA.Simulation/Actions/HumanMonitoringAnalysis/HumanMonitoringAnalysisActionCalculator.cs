using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators;
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
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = isCumulative;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = isCumulative;
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

            var nonDetectsImputationCalculator = HbmNonDetectImputationConcentrationModelFactory.Create(settings);
            var (imputedNonDetectsSubstanceCollection, concentrationModels) = nonDetectsImputationCalculator
                .ImputeNonDetects(
                    data.HbmSampleSubstanceCollections,
                    new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.HBM_CensoredValueImputation))
                );

            var missingValueImputationCalculator = HbmMissingValueImputationCalculatorFactory
                .Create(_project.HumanMonitoringSettings.MissingValueImputationMethod);
            var imputedMissingValuesSubstanceCollection = missingValueImputationCalculator
                .ImputeMissingValues(
                    imputedNonDetectsSubstanceCollection,
                    Simulation.IsBackwardCompatibilityMode
                        ? GetRandomGenerator(_project.MonteCarloSettings.RandomSeed)
                        : new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.HBM_MissingValueImputation))
                );

            var monitoringIndividualDayCalculator = new HbmIndividualDayConcentrationsCalculator(settings);

            // TODO: get HBM concentration unit and update calculator to align concentrations
            // with the selected target unit. Reason: in the future, the data coming from the human monitoring data module
            // might be in a different unit than the desired target unit, e.g. HBM data in mg/L and target
            // unit in ug/L. When this becomes the case, the monitoringIndividualDayCalculator should be updated to account
            // for this unit conversion.
            var individualDayConcentrations = monitoringIndividualDayCalculator
                .Calculate(
                    imputedMissingValuesSubstanceCollection,
                    data.HbmSamplingMethods,
                    _project.KineticModelSettings.CodeCompartment
                );

            List<HbmIndividualConcentration> individualConcentrations = null;
            List<HbmCumulativeIndividualConcentration> cumulativeIndividualConcentrations = null;
            List<HbmCumulativeIndividualDayConcentration> cumulativeIndividualDayConcentrations = null;

            if (_project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                var individualConcentrationsCalculator = new HbmIndividualConcentrationsCalculator();
                individualConcentrations = individualConcentrationsCalculator
                    .Calculate(
                        data.ActiveSubstances,
                        individualDayConcentrations
                    );
                if (data.CorrectedRelativePotencyFactors != null) {
                    var monitoringCumulativeIndividualCalculator = new HbmCumulativeIndividualConcentrationCalculator();
                    cumulativeIndividualConcentrations = monitoringCumulativeIndividualCalculator.Calculate(
                        individualConcentrations,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors
                    );
                }
            } else if (_project.AssessmentSettings.ExposureType == ExposureType.Acute
                && data.CorrectedRelativePotencyFactors != null
            ) {
                var monitoringCumulativeIndividualDayCalculator = new HbmCumulativeIndividualDayConcentrationCalculator();
                cumulativeIndividualDayConcentrations = monitoringCumulativeIndividualDayCalculator.Calculate(
                    individualDayConcentrations,
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors
                );
            }
            var result = new HumanMonitoringAnalysisActionResult();
            if (data.ActiveSubstances.Count > 1) {
                var exposureMatrixBuilder = new ExposureMatrixBuilder(
                    data.ActiveSubstances,
                    data.ReferenceCompound == null ? data.ActiveSubstances.ToDictionary(r => r, r => 1D) : data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    _project.AssessmentSettings.ExposureType,
                    _project.SubsetSettings.IsPerPerson,
                    _project.MixtureSelectionSettings.ExposureApproachType
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
