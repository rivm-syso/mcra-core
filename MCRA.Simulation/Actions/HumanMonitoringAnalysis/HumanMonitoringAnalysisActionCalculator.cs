using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmConcentrationModelCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.NonDetectsImputationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Units;
using MCRA.Utils.ExtensionMethods;
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
            var useKineticConversionFactors = _project.HumanMonitoringSettings.KineticConversionMethod == KineticConversionType.KineticConversion
                && _project.HumanMonitoringSettings.ImputeHbmConcentrationsFromOtherMatrices;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = isCumulative || isRiskBasedMcr;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = isCumulative || isRiskBasedMcr;
            _actionInputRequirements[ActionType.KineticModels].IsRequired = useKineticConversionFactors;
            _actionInputRequirements[ActionType.KineticModels].IsVisible = useKineticConversionFactors;
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

            var standardisedSubstanceCollection = imputedMissingValuesSubstanceCollection;

            var timeScaleUnit = settings.ExposureType == ExposureType.Chronic
                ? TimeScaleUnit.SteadyState : TimeScaleUnit.Peak;

            var hbmTargetBiologicalMatrix = _project.HumanMonitoringSettings.TargetMatrix;

            // TODO: this code temporary replaces the old single concentration unit value on ActionData level.
            //       We should use the concentration units per collection.
            var hbmConcentrationUnit = data.HbmSampleSubstanceCollections.FirstOrDefault().TargetConcentrationUnit;

            // Standardize blood concentrations (express soluble substances per lipid content)
            if (settings.StandardiseBlood) {
                var substancesExcludedFromLipidStandardisation = settings.StandardiseBloodExcludeSubstances ? settings.StandardiseBloodExcludedSubstancesSubset : new();
                var lipidContentCorrector = BloodCorrectionCalculatorFactory.Create(settings.StandardiseBloodMethod, substancesExcludedFromLipidStandardisation);
                standardisedSubstanceCollection = lipidContentCorrector
                    .ComputeTotalLipidCorrection(
                        imputedMissingValuesSubstanceCollection,
                        hbmConcentrationUnit
                    );
            }

            // Normalise by specific gravity or standardise by creatinine concentration
            if (settings.StandardiseUrine) {
                var substancesExcludedFromUrineStandardisation = settings.StandardiseUrineExcludeSubstances ? settings.StandardiseUrineExcludedSubstancesSubset : new();
                var urineCorrectorCalculator = UrineCorrectionCalculatorFactory.Create(settings.StandardiseUrineMethod, substancesExcludedFromUrineStandardisation);
                standardisedSubstanceCollection = urineCorrectorCalculator
                    .ComputeResidueCorrection(
                        standardisedSubstanceCollection,
                        hbmConcentrationUnit
                    );
            }

            // Target units from data
            var targetUnits = standardisedSubstanceCollection
                .Where(r => r.SamplingMethod.BiologicalMatrix == hbmTargetBiologicalMatrix)
                .Select(r => new TargetUnit(
                        r.TargetConcentrationUnit.GetSubstanceAmountUnit(),
                        r.TargetConcentrationUnit.GetConcentrationMassUnit(),
                        timeScaleUnit,
                        hbmTargetBiologicalMatrix,
                        r.ExpressionType
                    )
                )
                .ToList();

            // Compute hbm individual day concentration collections (per combination of matrix and expression type)
            var monitoringIndividualDayConcentrationCalculator = new HbmMainIndividualDayConcentrationsCalculator(hbmTargetBiologicalMatrix);
            var monitoringIndividualDayCollections = monitoringIndividualDayConcentrationCalculator
                .Calculate(
                    standardisedSubstanceCollection,
                    individualDays,
                    data.ActiveSubstances ?? data.AllCompounds,
                    targetUnits
                );

            // Apply matrix concentration conversion
            // TODO: get HBM concentration unit and update calculator to align concentrations
            // with the selected target unit. Reason: in the future, the data coming from the human monitoring data module
            // might be in a different unit than the desired target unit, e.g. HBM data in mg/L and target
            // unit in ug/L. When this becomes the case, the monitoringIndividualDayCalculator should be updated to account
            // for this unit conversion.
            if (settings.ImputeHbmConcentrationsFromOtherMatrices) {

                // Here we assume that we have selected one matrix to which we want to convert all concentrations.
                // However, notice that we could still end up with multiple target units (max. two), because
                // of different expression types (e.g., blood concentrations as ug/L and ug/g lipids).

                var matrixConversionCalculator = TargetMatrixConversionCalculatorFactory.Create(
                    kineticConversionType: settings.KineticConversionMethod,
                    kineticConversionFactors: data.KineticConversionFactors,
                    targetUnits: targetUnits,
                    conversionFactor: settings.HbmBetweenMatrixConversionFactor
                );

                var monitoringOtherIndividualDayCalculator = new HbmOtherIndividualDayConcentrationsCalculator(matrixConversionCalculator);
                monitoringIndividualDayCollections = monitoringOtherIndividualDayCalculator
                    .Calculate(
                        monitoringIndividualDayCollections,
                        standardisedSubstanceCollection,
                        individualDays,
                        data.ActiveSubstances ?? data.AllCompounds);
            }

            // Remove all individualDays containing missing values.
            var individualDayCollections = new List<HbmIndividualDayCollection>();
            foreach (var collection in monitoringIndividualDayCollections) {
                var remainingSubstances = collection.HbmIndividualDayConcentrations
                    .SelectMany(c => c.ConcentrationsBySubstance.Keys)
                    .Distinct()
                    .ToList();
                var records = collection.HbmIndividualDayConcentrations
                    .Select(individualDay => {
                        var individualDaySubstances = individualDay.ConcentrationsBySubstance.Keys.ToList();
                        return !remainingSubstances.Except(individualDaySubstances).Any() ? individualDay : null;
                    })
                    .Where(c => c != null)
                    .ToList();
                individualDayCollections.Add(new HbmIndividualDayCollection() {
                    TargetUnit = collection.TargetUnit,
                    HbmIndividualDayConcentrations = records,
                });
            }

            // Throw an exception if we all individual days were removed due to missing substance concentrations
            if (!individualDayCollections.SelectMany(c => c.HbmIndividualDayConcentrations).Any()) {
                throw new Exception("All HBM individual day records were removed for having non-imputed missing substance concentrations.");
            }

            // For chronic assessments, compute average individual concentrations
            List<HbmIndividualCollection> individualCollections = null;
            if (settings.ExposureType == ExposureType.Chronic) {
                var individualConcentrationsCalculator = new HbmIndividualConcentrationsCalculator();
                individualCollections = individualConcentrationsCalculator
                    .Calculate(
                        data.ActiveSubstances,
                        individualDayCollections
                    );
            }

            // Compute cumulative concentrations
            List<HbmCumulativeIndividualCollection> cumulativeIndividualCollections = null;
            List<HbmCumulativeIndividualDayCollection> cumulativeIndividualDayCollections = null;
            if (data.CorrectedRelativePotencyFactors != null) {
                if (settings.ExposureType == ExposureType.Chronic) {
                    // For cumulative assessments, compute cumulative individual concentrations
                    var hbmCumulativeIndividualCalculator = new HbmCumulativeIndividualConcentrationCalculator();
                    cumulativeIndividualCollections = hbmCumulativeIndividualCalculator.Calculate(
                        individualCollections,
                        data.ActiveSubstances,
                        data.CorrectedRelativePotencyFactors
                    );
                } else {
                    // For cumulative assessments, compute cumulative individual day concentrations
                    var hbmCumulativeIndividualDayCalculator = new HbmCumulativeIndividualDayConcentrationCalculator();
                    cumulativeIndividualDayCollections = hbmCumulativeIndividualDayCalculator.Calculate(
                        individualDayCollections,
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
                result.ExposureMatrix = exposureMatrixBuilder.Compute(
                    individualDayCollections,
                    individualCollections
                );
                result.DriverSubstances = DriverSubstanceCalculator.CalculateExposureDrivers(result.ExposureMatrix);
            }
            localProgress.Update(100);
            result.HbmIndividualDayConcentrations = individualDayCollections;
            result.HbmIndividualConcentrations = individualCollections;
            result.HbmCumulativeIndividualCollections = cumulativeIndividualCollections;
            result.HbmCumulativeIndividualDayCollections = cumulativeIndividualDayCollections;
            result.HbmConcentrationModels = concentrationModels;
            return result;
        }

        protected override void updateSimulationData(ActionData data, HumanMonitoringAnalysisActionResult result) {
            data.HbmIndividualDayCollections = result.HbmIndividualDayConcentrations;
            data.HbmIndividualCollections = result.HbmIndividualConcentrations;
            data.HbmCumulativeIndividualCollections = result.HbmCumulativeIndividualCollections;
            data.HbmCumulativeIndividualDayCollections = result.HbmCumulativeIndividualDayCollections;
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
