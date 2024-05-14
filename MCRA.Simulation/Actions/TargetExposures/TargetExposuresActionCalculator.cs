using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Data.Management;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.ActionComparison;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.TargetExposures {

    [ActionType(ActionType.TargetExposures)]
    public class TargetExposuresActionCalculator : ActionCalculatorBase<TargetExposuresActionResult> {

        public TargetExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isCumulative = _project.AssessmentSettings.MultipleSubstances && _project.AssessmentSettings.Cumulative;
            var isRiskBasedMcr = _project.AssessmentSettings.MultipleSubstances 
                && _project.EffectSettings.AnalyseMcr
                && _project.EffectSettings.ExposureApproachType == ExposureApproachType.RiskBased;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = isCumulative || isRiskBasedMcr;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = isCumulative || isRiskBasedMcr;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = isCumulative;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = isCumulative;
            _actionInputRequirements[ActionType.NonDietaryExposures].IsRequired = _project.AssessmentSettings.Aggregate;
            _actionInputRequirements[ActionType.NonDietaryExposures].IsVisible = _project.AssessmentSettings.Aggregate;
            _actionInputRequirements[ActionType.KineticModels].IsRequired = false;
            var isTargetLevelInternal = _project.EffectSettings.TargetDoseLevelType == TargetLevelType.Internal;
            _actionInputRequirements[ActionType.KineticModels].IsVisible = isTargetLevelInternal;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new TargetExposuresSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new TargetExposuresSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override TargetExposuresActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            var settings = new TargetExposuresModuleSettings(_project);
            var substances = data.ActiveSubstances;

            // Determine exposure routes
            // TODO for the Cosmos model: Dietary becomes Oral. 
            // Oral routes in non-dietary should be added to the Oral route for dietary.
            var exposureRoutes = new List<ExposurePathType>() { ExposurePathType.Oral };
            if (settings.Aggregate) {
                exposureRoutes.AddRange(data.NonDietaryExposureRoutes.Where(c => c!=ExposurePathType.Oral).ToList());
            }

            // TODO: determine target (from compartment selection) and appropriate
            // internal exposure unit.
            var codeCompartment = _project.KineticModelSettings.CodeCompartment;
            var biologicalMatrix = BiologicalMatrixConverter.FromString(codeCompartment, BiologicalMatrix.WholeBody);
            var target = new ExposureTarget(biologicalMatrix);
            var targetExposureUnit = new TargetUnit(
                target,
                new ExposureUnitTriple(
                    data.DietaryExposureUnit.ExposureUnit.SubstanceAmountUnit,
                    data.DietaryExposureUnit.ExposureUnit.ConcentrationMassUnit,
                    settings.ExposureType == ExposureType.Acute
                        ? TimeScaleUnit.Peak
                        : TimeScaleUnit.SteadyState
                )
            );

            // Compute results
            var result = compute(
                data,
                settings,
                exposureRoutes,
                targetExposureUnit,
                new CompositeProgressState(progressReport.CancellationToken)
            );

            // TODO, MCR analysis on target (internal) concentrations needs to be implemented
            if (_project.EffectSettings.AnalyseMcr
                && substances.Count > 1 
                && data.CorrectedRelativePotencyFactors != null
            ) {
                var exposureMatrixBuilder = new ExposureMatrixBuilder(
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    settings.ExposureType,
                    false
                );
                result.ExposureMatrix = exposureMatrixBuilder
                    .Compute(
                        result.AggregateIndividualDayExposures,
                        result.AggregateIndividualExposures,
                        result.TargetExposureUnit
                    );
                result.DriverSubstances = DriverSubstanceCalculator.CalculateExposureDrivers(result.ExposureMatrix);
            }
            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(TargetExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new TargetExposuresSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void updateSimulationData(ActionData data, TargetExposuresActionResult result) {
            data.AggregateIndividualDayExposures = result.AggregateIndividualDayExposures;
            data.AggregateIndividualExposures = result.AggregateIndividualExposures;
            data.ExposureRoutes = result.ExposureRoutes;
            data.TargetExposureUnit = result.TargetExposureUnit;
            data.ExternalExposureUnit = result.ExternalExposureUnit;
        }

        protected override TargetExposuresActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var settings = new TargetExposuresModuleSettings(_project);

            var substances = data.ActiveSubstances;

            // Compute results
            var result = compute(
                data,
                settings,
                data.ExposureRoutes,
                data.TargetExposureUnit,
                new CompositeProgressState(progressReport.CancellationToken)
            );

            // TODO: find a better way to compute uncertainty factorials
            if (substances.Count == 1 || data.CorrectedRelativePotencyFactors != null) {
                // Compute factorial responses
                var uncertaintyFactorialResponses = new List<double>();
                if (settings.ExposureType == ExposureType.Acute) {
                    uncertaintyFactorialResponses = (substances.Count > 1)
                        ? result.AggregateIndividualDayExposures
                            .Select(c => c.GetTotalExposureAtTarget(
                                data.TargetExposureUnit.Target,
                                data.CorrectedRelativePotencyFactors,
                                data.MembershipProbabilities
                            ))
                            .ToList()
                        : result.AggregateIndividualDayExposures
                            .Select(c => c.GetSubstanceExposure(
                                data.TargetExposureUnit.Target, substances.First()))
                            .ToList();
                } else {
                    uncertaintyFactorialResponses = (substances.Count > 1)
                        ? result.AggregateIndividualExposures
                            .Select(c => c.GetTotalExposureAtTarget(
                                data.TargetExposureUnit.Target,
                                data.CorrectedRelativePotencyFactors,
                                data.MembershipProbabilities
                            ))
                            .ToList()
                        : result.AggregateIndividualExposures
                            .Select(c => c.GetSubstanceExposure(
                                data.TargetExposureUnit.Target, substances.First()))
                            .ToList();
                }

                // Save factorial results
                result.FactorialResult = new TargetExposuresFactorialResult() {
                    Percentages = _project.OutputDetailSettings.SelectedPercentiles,
                    Percentiles = uncertaintyFactorialResponses?
                        .PercentilesWithSamplingWeights(null, _project.OutputDetailSettings.SelectedPercentiles).ToList()
                };
            }

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, TargetExposuresActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new TargetExposuresSummarizer();
            summarizer.SummarizeUncertain(
                header,
                _project,
                actionResult,
                data
            );
            localProgress.Update(100);
        }

        protected override void updateSimulationDataUncertain(ActionData data, TargetExposuresActionResult result) {
            data.AggregateIndividualDayExposures = result.AggregateIndividualDayExposures;
            data.AggregateIndividualExposures = result.AggregateIndividualExposures;
            data.ExposureRoutes = result.ExposureRoutes;
        }

        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, TargetExposuresActionResult result) {
            var outputWriter = new TargetExposuresOutputWriter();
            outputWriter.WriteOutputData(_project, data, result, rawDataWriter);
        }

        protected override void writeOutputDataUncertain(IRawDataWriter rawDataWriter, ActionData data, TargetExposuresActionResult result, int idBootstrap) {
            var outputWriter = new TargetExposuresOutputWriter();
            outputWriter.UpdateOutputData(_project, rawDataWriter, data, result, idBootstrap);
        }

        protected override IActionComparisonData loadActionComparisonData(ICompiledDataManager compiledDataManager) {
            var result = new TargetExposuresActionComparisonData() {
                TargetExposureModels = compiledDataManager.GetAllTargetExposureModels()?.Values
            };
            return result;
        }

        public override void SummarizeUncertaintyFactorial(
            UncertaintyFactorialDesign uncertaintyFactorial,
            List<UncertaintyFactorialResultRecord> factorialResult,
            SectionHeader header
        ) {
            if (!factorialResult.Any(r => r.ResultRecord is TargetExposuresFactorialResult)) {
                return;
            }

            // Get the factorial percentile results and the percentages
            var factorialPercentilesResults = factorialResult
                .Where(r => r.ResultRecord is TargetExposuresFactorialResult)
                .Select(r => (r.Tags, (r.ResultRecord as TargetExposuresFactorialResult).Percentiles))
                .ToList();
            var percentages = (factorialResult
                .First(r => r.ResultRecord is TargetExposuresFactorialResult).ResultRecord as TargetExposuresFactorialResult)
                .Percentages;

            // Compute percentiles factorial
            var percentilesFactorialCalculator = new PercentilesUncertaintyFactorialCalculator();
            var percentilesFactorialResults = percentilesFactorialCalculator.Compute(
                factorialPercentilesResults,
                percentages,
                uncertaintyFactorial.UncertaintySources,
                uncertaintyFactorial.DesignMatrix
            );

            // Summarize percentiles factorial results
            var subHeader = header.GetSubSectionHeader<TargetExposuresSummarySection>();
            var uncertaintyFactorialSection = new UncertaintyFactorialSection();
            var subSubHeader = subHeader.AddSubSectionHeaderFor(uncertaintyFactorialSection, "Uncertainty factorial", 150);
            uncertaintyFactorialSection.Summarize(percentilesFactorialResults, percentages);
            subSubHeader.SaveSummarySection(uncertaintyFactorialSection);
        }

        public override void SummarizeComparison(ICollection<IActionComparisonData> comparisonData, SectionHeader header) {
            var models = comparisonData
                .Where(r => (r as TargetExposuresActionComparisonData).TargetExposureModels?.Any() ?? false)
                .Select(r => {
                    var result = (r as TargetExposuresActionComparisonData).TargetExposureModels.First();
                    result.Code = r.IdResultSet;
                    result.Name = r.NameResultSet;
                    return result;
                })
                .ToList();
            var summarizer = new TargetExposuresCombinedActionSummarizer();
            summarizer.Summarize(models, header);
        }

        /// <summary>
        /// Runs the acute simulation.
        /// </summary>
        private TargetExposuresActionResult compute(
            ActionData data,
            TargetExposuresModuleSettings settings,
            ICollection<ExposurePathType> exposureRoutes,
            TargetUnit targetUnit,
            CompositeProgressState progressReport
        ) {
            var result = new TargetExposuresActionResult();

            var localProgress = progressReport.NewProgressState(20);

            var externalExposureUnit = data.DietaryExposureUnit.ExposureUnit;

            // Create non-dietary exposure calculator
            List<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes = null;
            if (settings.Aggregate) {
                localProgress.Update("Matching dietary and non-dietary exposures");

                var nonDietaryExposureGeneratorFactory = new NonDietaryExposureGeneratorFactory(settings);
                var nonDietaryIntakeCalculator = nonDietaryExposureGeneratorFactory.Create();
                nonDietaryIntakeCalculator.Initialize(
                    data.NonDietaryExposures,
                    externalExposureUnit,
                    data.BodyWeightUnit
                );
                var seedNonDietaryExposuresSampling = RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.BME_DrawNonDietaryExposures);

                // Collect non-dietary exposures
                nonDietaryIndividualDayIntakes = settings.ExposureType == ExposureType.Acute
                    ? nonDietaryIntakeCalculator?
                        .CalculateAcuteNonDietaryIntakes(
                            data.DietaryIndividualDayIntakes.Cast<IIndividualDay>().ToList(),
                            data.ActiveSubstances,
                            data.NonDietaryExposures.Keys,
                            seedNonDietaryExposuresSampling,
                            progressReport.CancellationToken
                        )
                    : nonDietaryIntakeCalculator?
                        .CalculateChronicNonDietaryIntakes(
                            data.DietaryIndividualDayIntakes.Cast<IIndividualDay>().ToList(),
                            data.ActiveSubstances,
                            data.NonDietaryExposures.Keys,
                            seedNonDietaryExposuresSampling,
                            progressReport.CancellationToken
                        );
            }
            localProgress.Update(20);

            // Create aggregate individual day exposures
            var combinedExternalIndividualDayExposures = AggregateIntakeCalculator
                .CreateCombinedIndividualDayExposures(
                    data.DietaryIndividualDayIntakes,
                    nonDietaryIndividualDayIntakes,
                    exposureRoutes
                );

            // Create kinetic model calculators
            var kineticModelCalculatorFactory = new KineticModelCalculatorFactory(
                data.AbsorptionFactors,
                data.KineticModelInstances
            );
            var kineticModelCalculators = kineticModelCalculatorFactory
                .CreateHumanKineticModels(data.ActiveSubstances);

            localProgress.Update("Computing internal exposures");

            // Create internal concentrations calculator
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var kineticConversionFactorCalculator = new KineticConversionFactorsCalculator(kineticModelCalculators);
            var seedKineticModelParameterSampling = RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.BME_DrawKineticModelParameters);
            var kineticModelParametersRandomGenerator = new McraRandomGenerator(seedKineticModelParameterSampling);
            if (settings.ExposureType == ExposureType.Acute) {
                // Compute target exposures
                var aggregateIndividualDayExposures = targetExposuresCalculator
                    .ComputeAcute(
                        combinedExternalIndividualDayExposures,
                        data.ActiveSubstances,
                        exposureRoutes,
                        externalExposureUnit,
                        new List<TargetUnit>() { targetUnit },
                        kineticModelParametersRandomGenerator,
                        progressReport.NewProgressState(80)
                    );
                result.AggregateIndividualDayExposures = aggregateIndividualDayExposures;

                // Compute kinetic conversion factors
                kineticModelParametersRandomGenerator.Reset();
                var kineticConversionFactors = kineticConversionFactorCalculator
                    .ComputeKineticConversionFactors(
                        data.ActiveSubstances,
                        exposureRoutes,
                        combinedExternalIndividualDayExposures,
                        externalExposureUnit,
                        targetUnit,
                        kineticModelParametersRandomGenerator
                    );
                result.KineticConversionFactors = kineticConversionFactors;
            } else {
                // Create aggregate individual exposures
                var externalIndividualExposures = AggregateIntakeCalculator
                    .CreateCombinedExternalIndividualExposures(
                        combinedExternalIndividualDayExposures.ToList()
                    );

                // Compute target exposures
                var aggregateIndividualExposures = targetExposuresCalculator
                    .ComputeChronic(
                        externalIndividualExposures,
                        data.ActiveSubstances,
                        exposureRoutes,
                        externalExposureUnit,
                        new List<TargetUnit>() { targetUnit },
                        kineticModelParametersRandomGenerator,
                        progressReport.NewProgressState(80)
                    );
                result.AggregateIndividualExposures = aggregateIndividualExposures;

                // Compute kinetic conversion factors
                kineticModelParametersRandomGenerator.Reset();
                var kineticConversionFactors = kineticConversionFactorCalculator
                    .ComputeKineticConversionFactors(
                        data.ActiveSubstances,
                        exposureRoutes,
                        externalIndividualExposures,
                        externalExposureUnit,
                        targetUnit,
                        kineticModelParametersRandomGenerator
                    );
                result.KineticConversionFactors = kineticConversionFactors;
            }

            result.ExternalExposureUnit = externalExposureUnit;
            result.TargetExposureUnit = targetUnit;
            result.ExposureRoutes = exposureRoutes;
            result.NonDietaryIndividualDayIntakes = nonDietaryIndividualDayIntakes;
            result.KineticModelCalculators = kineticModelCalculators;

            localProgress.Update(100);

            return result;
        }
    }
}