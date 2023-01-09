using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions.ActionComparison;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.IndividualTargetExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;
using MCRA.Simulation.OutputGeneration;
using System.Collections.Generic;
using System.Linq;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.TargetExposures {

    [ActionType(ActionType.TargetExposures)]
    public class TargetExposuresActionCalculator : ActionCalculatorBase<TargetExposuresActionResult> {

        public TargetExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isCumulative = _project.AssessmentSettings.MultipleSubstances && _project.AssessmentSettings.Cumulative;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = isCumulative;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = isCumulative;
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
            var settings = new TargetExposuresModuleSettings(_project);
            var substances = data.ActiveSubstances;

            // Determine exposure routes
            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary };
            if (settings.Aggregate) {
                exposureRoutes.AddRange(data.NonDietaryExposureRoutes);
            }

            // Get external and target exposure units
            var externalExposureUnit = data.DietaryExposureUnit;
            var compartment = settings.TargetDoseLevel == TargetLevelType.External ? "bw" : null;
            var unit = TargetUnit.CreateDietaryExposureUnit(data.ConsumptionUnit, data.ConcentrationUnit, data.BodyWeightUnit, false);
            var targetExposureUnit = new TargetUnit(unit.SubstanceAmount, unit.ConcentrationMassUnit, compartment, unit.TimeScaleUnit);
            targetExposureUnit.SetTimeScale(settings.TargetDoseLevel, settings.ExposureType);

            // Create kinetic model calculators
            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators = null;
            if (settings.TargetDoseLevel == TargetLevelType.Internal) {
                //TODO ook op grond van kineticModelInputSubstances
                var kineticModelCalculatorFactory = new KineticModelCalculatorFactory(data.AbsorptionFactors, data.KineticModelInstances);
                kineticModelCalculators = kineticModelCalculatorFactory.CreateHumanKineticModels(substances);
            }

            // Create target exposures calculator and compute target exposures
            var targetCalculator = TargetExposuresCalculatorFactory.Create(
                settings.TargetDoseLevel,
                kineticModelCalculators
            );
            // Create non-dietary exposure calculator
            NonDietaryExposureGenerator nonDietaryIntakeCalculator = null;
            if (settings.Aggregate) {
                var nonDietaryExposureGeneratorFactory = new NonDietaryExposureGeneratorFactory(settings);
                nonDietaryIntakeCalculator = nonDietaryExposureGeneratorFactory.Create();
            }
            nonDietaryIntakeCalculator?.Initialize(data.NonDietaryExposures, externalExposureUnit, data.BodyWeightUnit);

            var individualTargetExposureCalculatorFactory = new IndividualTargetExposureCalculatorFactory(settings);
            var targetExposuresCalculator = individualTargetExposureCalculatorFactory.Create();

            var result = targetExposuresCalculator.Compute(
                substances,
                data.NonDietaryExposures,
                data.DietaryIndividualDayIntakes,
                data.ReferenceCompound,
                data.DietaryModelAssistedIntakes,
                nonDietaryIntakeCalculator,
                kineticModelCalculators,
                targetCalculator,
                exposureRoutes,
                externalExposureUnit,
                targetExposureUnit,
                Simulation.IsBackwardCompatibilityMode
                    ? _project.MonteCarloSettings.RandomSeed
                    : RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.BME_DrawNonDietaryExposures),
                Simulation.IsBackwardCompatibilityMode
                    ? _project.MonteCarloSettings.RandomSeed
                    : RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.BME_DrawKineticModelParameters),
                settings.FirstModelThenAdd,
                data.KineticModelInstances,
                data.SelectedPopulation,
                progressReport
            );

            result.ExternalExposureUnit = data.DietaryExposureUnit;

            if (substances.Count > 1 && data.CorrectedRelativePotencyFactors != null) {
                var exposureMatrixBuilder = new ExposureMatrixBuilder(
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    settings.ExposureType,
                    false
                );
                result.ExposureMatrix = exposureMatrixBuilder.Compute(result.AggregateIndividualDayExposures, result.AggregateIndividualExposures);
                result.DriverSubstances = DriverSubstanceCalculator.CalculateExposureDrivers(result.ExposureMatrix);
            }
            return result;
        }

        protected override void summarizeActionResult(TargetExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(0);
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
            var settings = new TargetExposuresModuleSettings(_project);

            var substances = data.ActiveSubstances;
            var individualTargetExposureCalculatorFactory = new IndividualTargetExposureCalculatorFactory(settings);
            var targetExposuresCalculator = individualTargetExposureCalculatorFactory.Create();

            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators = null;
            if (settings.TargetDoseLevel == TargetLevelType.Internal) {
                var kineticModelCalculatorFactory = new KineticModelCalculatorFactory(data.AbsorptionFactors, data.KineticModelInstances);
                kineticModelCalculators = kineticModelCalculatorFactory.CreateHumanKineticModels(substances);
            }
            var externalExposureUnit = data.DietaryExposureUnit;
            var targetExposureUnit = data.TargetExposureUnit;

            NonDietaryExposureGenerator nonDietaryIntakeCalculator = null;
            if (settings.Aggregate) {
                var nonDietaryExposureGeneratorFactory = new NonDietaryExposureGeneratorFactory(settings);
                nonDietaryIntakeCalculator = nonDietaryExposureGeneratorFactory.Create();
            }
            nonDietaryIntakeCalculator?.Initialize(data.NonDietaryExposures, externalExposureUnit, data.BodyWeightUnit);

            var targetCalculator = TargetExposuresCalculatorFactory
                .Create(settings.TargetDoseLevel, kineticModelCalculators);
            var result = targetExposuresCalculator.Compute(
                substances,
                data.NonDietaryExposures,
                data.DietaryIndividualDayIntakes,
                data.ReferenceCompound,
                data.DietaryModelAssistedIntakes,
                nonDietaryIntakeCalculator,
                kineticModelCalculators,
                targetCalculator,
                data.ExposureRoutes,
                externalExposureUnit,
                targetExposureUnit,
                Simulation.IsBackwardCompatibilityMode
                    ? _project.MonteCarloSettings.RandomSeed
                    : RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.BME_DrawNonDietaryExposures),
                Simulation.IsBackwardCompatibilityMode
                    ? _project.MonteCarloSettings.RandomSeed
                    : RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.BME_DrawKineticModelParameters),
                settings.FirstModelThenAdd,
                data.KineticModelInstances,
                data.SelectedPopulation,
                progressReport
            );

            // TODO: find a better way to compute uncertainty factorials
            if (substances.Count == 1 || data.CorrectedRelativePotencyFactors != null) {
                // Compute factorial responses
                var uncertaintyFactorialResponses = new List<double>();
                if (settings.ExposureType == ExposureType.Acute) {
                    uncertaintyFactorialResponses = (substances.Count > 1)
                        ? result.AggregateIndividualDayExposures
                            .Select(c => c.TotalConcentrationAtTarget(data.CorrectedRelativePotencyFactors, data.MembershipProbabilities, false))
                            .ToList()
                        : result.AggregateIndividualDayExposures
                            .Select(c => c.GetSubstanceTotalExposurePerMassUnit(substances.First(), false))
                            .ToList();
                } else {
                    uncertaintyFactorialResponses = (substances.Count > 1)
                        ? result.AggregateIndividualExposures
                            .Select(c => c.TotalConcentrationAtTarget(data.CorrectedRelativePotencyFactors, data.MembershipProbabilities, false))
                            .ToList()
                        : result.AggregateIndividualExposures
                            .Select(c => c.GetSubstanceTotalExposurePerMassUnit(substances.First(), false))
                            .ToList();
                }

                // Save factorial results
                result.FactorialResult = new TargetExposuresFactorialResult() {
                    Percentages = _project.OutputDetailSettings.SelectedPercentiles,
                    Percentiles = uncertaintyFactorialResponses?
                        .PercentilesWithSamplingWeights(null, _project.OutputDetailSettings.SelectedPercentiles).ToList()
                };
            }

            return result;
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, TargetExposuresActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var summarizer = new TargetExposuresSummarizer();
            summarizer.SummarizeUncertain(
                header,
                actionResult,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                data.ActiveSubstances,
                data.KineticModelInstances,
                data.ExposureRoutes,
                data.NonDietaryExposureRoutes,
                _project.AssessmentSettings.ExposureType,
                _project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                _project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                _project.SubsetSettings.IsPerPerson,
                _project.AssessmentSettings.Aggregate
            );
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
    }
}