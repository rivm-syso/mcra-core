using MCRA.Data.Compiled.Wrappers;
using MCRA.Data.Management;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.ActionComparison;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.MatchIndividualExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;
using ExpressionType = MCRA.General.ExpressionType;

namespace MCRA.Simulation.Actions.TargetExposures {

    [ActionType(ActionType.TargetExposures)]
    public class TargetExposuresActionCalculator : ActionCalculatorBase<TargetExposuresActionResult> {
        private TargetExposuresModuleConfig ModuleConfig => (TargetExposuresModuleConfig)_moduleSettings;

        public TargetExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isCumulative = ModuleConfig.MultipleSubstances && ModuleConfig.Cumulative;
            var isRiskBasedMcr = ModuleConfig.MultipleSubstances
                && ModuleConfig.McrAnalysis
                && ModuleConfig.McrExposureApproachType == ExposureApproachType.RiskBased;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = isCumulative || isRiskBasedMcr;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = isCumulative || isRiskBasedMcr;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = isCumulative;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = isCumulative;

            var requireNonDietary = ModuleConfig.ExposureSources.Contains(ExposureSource.OtherNonDietary);
            _actionInputRequirements[ActionType.NonDietaryExposures].IsRequired = requireNonDietary;
            _actionInputRequirements[ActionType.NonDietaryExposures].IsVisible = requireNonDietary;

            var requireDietary = ModuleConfig.ExposureSources.Contains(ExposureSource.DietaryExposures);
            _actionInputRequirements[ActionType.DietaryExposures].IsRequired = requireDietary;
            _actionInputRequirements[ActionType.DietaryExposures].IsVisible = requireDietary;

            var requireDust = ModuleConfig.ExposureSources.Contains(ExposureSource.DustExposures);
            _actionInputRequirements[ActionType.DustExposures].IsRequired = requireDust;
            _actionInputRequirements[ActionType.DustExposures].IsVisible = requireDust;

            // For internal (systemic) dose require absorption factors
            var requireAbsorptionFactors = ModuleConfig.TargetDoseLevelType == TargetLevelType.Systemic;
            _actionInputRequirements[ActionType.KineticModels].IsRequired = false;
            _actionInputRequirements[ActionType.KineticModels].IsVisible = requireAbsorptionFactors;

            // For internal (target) concentrations require either kinetic conversion factors or PBK models
            var isInternalDose = ModuleConfig.TargetDoseLevelType == TargetLevelType.Internal;
            var requireConversionFactors = isInternalDose
                && (ModuleConfig.InternalModelType == InternalModelType.ConversionFactorModel
                    || ModuleConfig.InternalModelType == InternalModelType.PBKModel
                );
            _actionInputRequirements[ActionType.KineticConversionFactors].IsRequired = requireConversionFactors;
            _actionInputRequirements[ActionType.KineticConversionFactors].IsVisible = requireConversionFactors;

            var requirePbkModels = isInternalDose &&
                (ModuleConfig.InternalModelType == InternalModelType.PBKModel
                || ModuleConfig.InternalModelType == InternalModelType.PBKModelOnly
            );
            _actionInputRequirements[ActionType.PbkModels].IsRequired = requirePbkModels;
            _actionInputRequirements[ActionType.PbkModels].IsVisible = requirePbkModels;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new TargetExposuresSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new TargetExposuresSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override TargetExposuresActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            var settings = new TargetExposuresModuleSettings(ModuleConfig);
            var substances = data.ActiveSubstances;

            // Determine exposure routes
            var exposureRoutes = ModuleConfig.ExposureRoutes;

            // TODO: get external exposure unit from selected reference source
            var externalExposureUnit = data.DietaryExposureUnit.ExposureUnit;

            TargetUnit targetUnit;
            if (ModuleConfig.TargetDoseLevelType == TargetLevelType.Systemic) {
                targetUnit = TargetUnit.FromSystemicExposureUnit(externalExposureUnit);
            } else if (ModuleConfig.TargetDoseLevelType == TargetLevelType.Internal) {
                // Determine target (from compartment selection) and appropriate internal exposure unit
                var biologicalMatrix = BiologicalMatrixConverter
                    .FromString(ModuleConfig.CodeCompartment, BiologicalMatrix.WholeBody);

                ExpressionType expressionType;
                if (biologicalMatrix.IsUrine() && ModuleConfig.StandardisedNormalisedUrine) {
                    if (ModuleConfig.SelectedExpressionType == ExpressionType.Creatinine) {
                        expressionType = ExpressionType.Creatinine;
                    } else {
                        expressionType = ExpressionType.SpecificGravity;
                    }
                } else if (biologicalMatrix.IsBlood() && ModuleConfig.StandardisedBlood) {
                    expressionType = ExpressionType.Lipids;
                } else {
                    expressionType = ExpressionType.None;
                }

                var target = new ExposureTarget(biologicalMatrix, expressionType);
                targetUnit = new TargetUnit(
                    target,
                    ExposureUnitTriple.CreateDefaultExposureUnit(target, ModuleConfig.ExposureType)
                );
            } else {
                var msg = "Cannot compute internal exposures for target level 'external'.";
                throw new Exception(msg);
            }

            // Compute results
            var result = compute(
                data,
                settings,
                targetUnit,
                new CompositeProgressState(progressReport.CancellationToken)
            );

            // TODO, MCR analysis on target (internal) concentrations needs to be implemented
            if (ModuleConfig.McrAnalysis
                && substances.Count > 1
                && data.CorrectedRelativePotencyFactors != null
            ) {
                var exposureMatrixBuilder = new ExposureMatrixBuilder(
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    ModuleConfig.ExposureType,
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
            var summarizer = new TargetExposuresSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
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
            var settings = new TargetExposuresModuleSettings(ModuleConfig);

            var substances = data.ActiveSubstances;

            // Compute results
            var result = compute(
                data,
                settings,
                data.TargetExposureUnit,
                new CompositeProgressState(progressReport.CancellationToken)
            );

            // TODO: find a better way to compute uncertainty factorials
            if (substances.Count == 1 || data.CorrectedRelativePotencyFactors != null) {
                // Compute factorial responses
                var uncertaintyFactorialResponses = new List<double>();
                if (ModuleConfig.ExposureType == ExposureType.Acute) {
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
                    Percentages = ModuleConfig.SelectedPercentiles.ToArray(),
                    Percentiles = uncertaintyFactorialResponses?
                        .PercentilesWithSamplingWeights(null, ModuleConfig.SelectedPercentiles).ToList()
                };
            }

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, TargetExposuresActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new TargetExposuresSummarizer(ModuleConfig);
            summarizer.SummarizeUncertain(
                header,
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
            outputWriter.WriteOutputData(ModuleConfig, data, result, rawDataWriter);
        }

        protected override void writeOutputDataUncertain(IRawDataWriter rawDataWriter, ActionData data, TargetExposuresActionResult result, int idBootstrap) {
            var outputWriter = new TargetExposuresOutputWriter();
            outputWriter.UpdateOutputData(ModuleConfig, rawDataWriter, data, result, idBootstrap);
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
            TargetUnit targetUnit,
            CompositeProgressState progressReport
        ) {
            var result = new TargetExposuresActionResult();

            var localProgress = progressReport.NewProgressState(20);

            var externalExposureUnit = data.DietaryExposureUnit.ExposureUnit;

            var referenceIndividuals = MatchIndividualExposure
                .GetReferenceIndividuals(
                    data,
                    ModuleConfig.IndividualReferenceSet
                );

            ICollection<IIndividualDay> referenceIndividualDays = null;
            switch (ModuleConfig.IndividualReferenceSet) {
                case ExposureSource.DietaryExposures:
                    referenceIndividualDays = data.DietaryIndividualDayIntakes
                        .Cast<IIndividualDay>()
                        .ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            // Create non-dietary exposure calculator
            List<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes = null;
            if (settings.ExposureSources.Contains(ExposureSource.OtherNonDietary)) {
                localProgress.Update("Matching dietary and non-dietary exposures");

                var nonDietaryExposureGeneratorFactory = new NonDietaryExposureGeneratorFactory(settings);
                var nonDietaryIntakeCalculator = nonDietaryExposureGeneratorFactory.Create();
                nonDietaryIntakeCalculator.Initialize(
                    data.NonDietaryExposures,
                    externalExposureUnit,
                    data.BodyWeightUnit
                );
                var seedNonDietaryExposuresSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.BME_DrawNonDietaryExposures);

                // Collect non-dietary exposures
                nonDietaryIndividualDayIntakes = settings.ExposureType == ExposureType.Acute
                    ? nonDietaryIntakeCalculator?
                        .CalculateAcuteNonDietaryIntakes(
                            referenceIndividualDays,
                            data.ActiveSubstances,
                            data.NonDietaryExposures.Keys,
                            seedNonDietaryExposuresSampling,
                            progressReport.CancellationToken
                        )
                    : nonDietaryIntakeCalculator?
                        .CalculateChronicNonDietaryIntakes(
                            referenceIndividualDays,
                            data.ActiveSubstances,
                            data.NonDietaryExposures.Keys,
                            seedNonDietaryExposuresSampling,
                            progressReport.CancellationToken
                        );
            }
            localProgress.Update(20);

            // Create dust exposure calculator
            List<DustIndividualDayExposure> dustIndividualDayExposures = null;
            if (settings.ExposureSources.Contains(ExposureSource.DustExposures)) {
                localProgress.Update("Matching dietary and dust exposures");

                var dustExposureGeneratorFactory = new DustExposureGeneratorFactory(settings);
                var dustExposureCalculator = dustExposureGeneratorFactory.Create();
                dustExposureCalculator.Initialize(
                    data.IndividualDustExposures,
                    externalExposureUnit,
                    data.BodyWeightUnit
                );
                var seedDustExposuresSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_DrawDustExposures);

                // Collect non-dietary exposures
                dustIndividualDayExposures = dustExposureCalculator?
                    .GenerateDustIndividualDayExposures(
                        referenceIndividualDays,
                        data.ActiveSubstances,
                        data.IndividualDustExposures,
                        seedDustExposuresSampling,
                        progressReport.CancellationToken
                    );
            }
            localProgress.Update(20);

            var dietaryExposures = settings.ExposureSources.Contains(ExposureSource.DietaryExposures)
                ? data.DietaryIndividualDayIntakes
                : null;

            var exposurePathTypes = ModuleConfig.ExposureRoutes.Select(r => r.GetExposurePath()).ToList();

            // Create aggregate individual day exposures
            var combinedExternalIndividualDayExposures = AggregateIntakeCalculator
                .CreateCombinedIndividualDayExposures(
                    dietaryExposures,
                    nonDietaryIndividualDayIntakes,
                    dustIndividualDayExposures,
                    exposurePathTypes
                );

            // Create kinetic model calculators
            var kineticModelCalculatorFactory = new KineticModelCalculatorFactory(
                data.KineticModelInstances,
                data.KineticConversionFactorModels,
                data.AbsorptionFactors,
                ModuleConfig.TargetDoseLevelType,
                ModuleConfig.InternalModelType
            );
            var kineticModelCalculators = kineticModelCalculatorFactory
                .CreateHumanKineticModels(data.ActiveSubstances);

            localProgress.Update("Computing internal exposures");

            // Create internal concentrations calculator
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var kineticConversionFactorCalculator = new KineticConversionFactorsCalculator(kineticModelCalculators);
            var seedKineticModelParameterSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.BME_DrawKineticModelParameters);
            var kineticModelParametersRandomGenerator = new McraRandomGenerator(seedKineticModelParameterSampling);
            if (settings.ExposureType == ExposureType.Acute) {
                // Compute target exposures
                var aggregateIndividualDayExposures = targetExposuresCalculator
                    .ComputeAcute(
                        combinedExternalIndividualDayExposures,
                        data.ActiveSubstances,
                        exposurePathTypes,
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
                        exposurePathTypes,
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
                        exposurePathTypes,
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
                        exposurePathTypes,
                        externalIndividualExposures,
                        externalExposureUnit,
                        targetUnit,
                        kineticModelParametersRandomGenerator
                    );
                result.KineticConversionFactors = kineticConversionFactors;
            }

            result.ExternalExposureUnit = externalExposureUnit;
            result.TargetExposureUnit = targetUnit;
            result.ExposureRoutes = exposurePathTypes;
            result.NonDietaryIndividualDayIntakes = nonDietaryIndividualDayIntakes;
            result.KineticModelCalculators = kineticModelCalculators;

            localProgress.Update(100);

            return result;
        }


    }
}