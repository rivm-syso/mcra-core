﻿using MCRA.Data.Compiled.Wrappers;
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
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
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

            var requireNonDietary = ModuleConfig.ExposureSources.Contains(ExposureSource.OtherNonDiet);
            _actionInputRequirements[ActionType.NonDietaryExposures].IsRequired = requireNonDietary;
            _actionInputRequirements[ActionType.NonDietaryExposures].IsVisible = requireNonDietary;

            var requireDietary = ModuleConfig.ExposureSources.Contains(ExposureSource.Diet);
            _actionInputRequirements[ActionType.DietaryExposures].IsRequired = requireDietary;
            _actionInputRequirements[ActionType.DietaryExposures].IsVisible = requireDietary;

            var requireDust = ModuleConfig.ExposureType == ExposureType.Chronic &
                ModuleConfig.ExposureSources.Contains(ExposureSource.Dust);
            _actionInputRequirements[ActionType.DustExposures].IsRequired = requireDust;
            _actionInputRequirements[ActionType.DustExposures].IsVisible = requireDust;

            var requireSoil = ModuleConfig.ExposureType == ExposureType.Chronic &
                ModuleConfig.ExposureSources.Contains(ExposureSource.Soil);
            _actionInputRequirements[ActionType.SoilExposures].IsRequired = requireSoil;
            _actionInputRequirements[ActionType.SoilExposures].IsVisible = requireSoil;

            var requireAir = ModuleConfig.ExposureType == ExposureType.Chronic &
                ModuleConfig.ExposureSources.Contains(ExposureSource.Air);
            _actionInputRequirements[ActionType.AirExposures].IsRequired = requireAir;
            _actionInputRequirements[ActionType.AirExposures].IsVisible = requireAir;

            _actionInputRequirements[ActionType.KineticModels].IsRequired = false;
            _actionInputRequirements[ActionType.KineticModels].IsVisible = ModuleConfig.RequireAbsorptionFactors;

            _actionInputRequirements[ActionType.KineticConversionFactors].IsRequired = ModuleConfig.RequireKineticConversionFactors;
            _actionInputRequirements[ActionType.KineticConversionFactors].IsVisible = ModuleConfig.RequireKineticConversionFactors;

            _actionInputRequirements[ActionType.PbkModels].IsRequired = ModuleConfig.RequirePbkModels;
            _actionInputRequirements[ActionType.PbkModels].IsVisible = ModuleConfig.RequirePbkModels;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new TargetExposuresSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new TargetExposuresSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override TargetExposuresActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            var substances = data.ActiveSubstances;

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

            var substances = data.ActiveSubstances;

            // Compute results
            var result = compute(
                data,
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
                    Percentages = [.. ModuleConfig.SelectedPercentiles],
                    Percentiles = uncertaintyFactorialResponses?
                        .PercentilesWithSamplingWeights(null, ModuleConfig.SelectedPercentiles).ToList()
                };
            }

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResultUncertain(
            UncertaintyFactorialSet factorialSet,
            TargetExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            CompositeProgressState progressReport
        ) {
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
                .Where(r => (r as TargetExposuresActionComparisonData).TargetExposureModels?.Count > 0)
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
            TargetUnit targetUnit,
            CompositeProgressState progressReport
        ) {
            var result = new TargetExposuresActionResult();

            var localProgress = progressReport.NewProgressState(20);

            var externalExposureUnit = data.DietaryExposureUnit.ExposureUnit;

            ICollection<IIndividualDay> referenceIndividualDays = null;
            switch (ModuleConfig.IndividualReferenceSet) {
                case ExposureSource.Diet:
                    referenceIndividualDays = data.DietaryIndividualDayIntakes
                        .Cast<IIndividualDay>()
                        .ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            var externalExposureCollections = new List<ExternalExposureCollection>();

            // Collect non-dietary exposures
            if (ModuleConfig.ExposureSources.Contains(ExposureSource.OtherNonDiet)) {
                localProgress.Update("Matching dietary and non-dietary exposures");

                var nonDietaryIntakeCalculator = NonDietaryExposureGeneratorFactory.Create(
                    ModuleConfig.NonDietaryPopulationAlignmentMethod,
                    ModuleConfig.IsCorrelationBetweenIndividuals
                );
                nonDietaryIntakeCalculator.Initialize(
                    data.NonDietaryExposures,
                    [.. ModuleConfig.ExposureRoutes],
                    externalExposureUnit
                );
                var seedNonDietaryExposuresSampling = RandomUtils
                    .CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.BME_DrawNonDietaryExposures);

                var nonDietaryIndividualDayIntakes = ModuleConfig.ExposureType == ExposureType.Acute
                    ? nonDietaryIntakeCalculator?
                        .GenerateAcuteNonDietaryIntakes(
                            referenceIndividualDays,
                            data.ActiveSubstances,
                            data.NonDietaryExposures.Keys,
                            seedNonDietaryExposuresSampling,
                            progressReport.CancellationToken
                        )
                    : nonDietaryIntakeCalculator?
                        .GenerateChronicNonDietaryIntakes(
                            referenceIndividualDays,
                            data.ActiveSubstances,
                            data.NonDietaryExposures.Keys,
                            seedNonDietaryExposuresSampling,
                            progressReport.CancellationToken
                        );

                var nonDietaryExternalIndividualDayExposures = nonDietaryIndividualDayIntakes
                    .Cast<IExternalIndividualDayExposure>()
                    .ToList();

                var nonDietaryExposureCollection = new ExternalExposureCollection {
                    ExposureUnit = ExposureUnitTriple.FromExposureUnit(data.NonDietaryExposureUnit),
                    ExposureSource = ExposureSource.OtherNonDiet,
                    ExternalIndividualDayExposures = nonDietaryExternalIndividualDayExposures
                };
                externalExposureCollections.Add(nonDietaryExposureCollection);
            }
            localProgress.Update(20);

            // Collect dust exposures
            if (ModuleConfig.ExposureSources.Contains(ExposureSource.Dust)) {
                localProgress.Update("Matching dietary and dust exposures");

                var dustExposureCalculator = DustExposureGeneratorFactory.Create(ModuleConfig.DustPopulationAlignmentMethod);
                var seedDustExposuresSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_DrawDustExposures);
                var dustIndividualDayExposures = dustExposureCalculator
                    .GenerateDustIndividualDayExposures(
                        referenceIndividualDays,
                        data.ActiveSubstances,
                        data.IndividualDustExposures,
                        seedDustExposuresSampling,
                        progressReport.CancellationToken
                    )
                    .Cast<IExternalIndividualDayExposure>()
                    .ToList();

                var dustExposureCollection = new ExternalExposureCollection {
                    ExposureUnit = new ExposureUnitTriple(data.DustExposureUnit.SubstanceAmountUnit, ConcentrationMassUnit.PerUnit, TimeScaleUnit.PerDay),
                    ExposureSource = ExposureSource.Dust,
                    ExternalIndividualDayExposures = dustIndividualDayExposures
                };
                externalExposureCollections.Add(dustExposureCollection);
            }
            localProgress.Update(20);

            // Collect soil exposures
            if (ModuleConfig.ExposureSources.Contains(ExposureSource.Soil)) {
                localProgress.Update("Matching dietary and soil exposures");

                var soilExposureCalculator = SoilExposureGeneratorFactory.Create(ModuleConfig.SoilPopulationAlignmentMethod);
                var seedSoilExposuresSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_DrawSoilExposures);
                var soilIndividualDayExposures = soilExposureCalculator
                    .GenerateSoilIndividualDayExposures(
                        referenceIndividualDays,
                        data.ActiveSubstances,
                        data.IndividualSoilExposures,
                        seedSoilExposuresSampling,
                        progressReport.CancellationToken
                    )
                    .Cast<IExternalIndividualDayExposure>()
                    .ToList();

                var soilExposureCollection = new ExternalExposureCollection {
                    ExposureUnit = new ExposureUnitTriple(data.SoilExposureUnit.SubstanceAmountUnit, ConcentrationMassUnit.PerUnit, TimeScaleUnit.PerDay),
                    ExposureSource = ExposureSource.Soil,
                    ExternalIndividualDayExposures = soilIndividualDayExposures
                };
                externalExposureCollections.Add(soilExposureCollection);
            }
            localProgress.Update(30);

            // Create air exposure calculator
            ICollection<AirIndividualDayExposure> airIndividualDayExposures = null;
            if (ModuleConfig.ExposureSources.Contains(ExposureSource.Air)) {
                localProgress.Update("Matching dietary and air exposures");

                var airExposureCalculator = AirExposureGeneratorFactory.Create(ModuleConfig.AirPopulationAlignmentMethod);
                var seedAirExposuresSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.AIE_DrawAirExposures);

                // Generate air exposures
                airIndividualDayExposures = airExposureCalculator
                    .GenerateAirIndividualDayExposures(
                        referenceIndividualDays,
                        data.ActiveSubstances,
                        data.IndividualAirExposures,
                        seedAirExposuresSampling,
                        progressReport.CancellationToken
                    );

                var airExternalIndividualDayExposures = airIndividualDayExposures
                    .Cast<IExternalIndividualDayExposure>()
                    .ToList();

                var airExposureCollection = new ExternalExposureCollection {
                    ExposureUnit = new ExposureUnitTriple(data.AirExposureUnit.SubstanceAmountUnit, ConcentrationMassUnit.PerUnit, TimeScaleUnit.PerDay),
                    ExposureSource = ExposureSource.Air,
                    ExternalIndividualDayExposures = airExternalIndividualDayExposures
                };
                externalExposureCollections.Add(airExposureCollection);
            }
            localProgress.Update(20);


            var dietaryExposures = ModuleConfig.ExposureSources.Contains(ExposureSource.Diet)
                ? data.DietaryIndividualDayIntakes
                : null;

            // Combine all external exposures
            var combinedExternalIndividualDayExposures = AggregateIntakeCalculator
                .CreateCombinedIndividualDayExposures(
                    dietaryExposures,
                    externalExposureCollections,
                    externalExposureUnit,
                    ModuleConfig.ExposureType
                );

            // Create kinetic model calculators
            var kineticModelCalculatorFactory = new KineticModelCalculatorFactory(
                data.KineticModelInstances,
                data.KineticConversionFactorModels,
                data.AbsorptionFactors,
                ModuleConfig.TargetDoseLevelType,
                ModuleConfig.InternalModelType
            );
            var pbkSimulationSettings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = ModuleConfig.NumberOfDays,
                UseRepeatedDailyEvents = ModuleConfig.ExposureEventsGenerationMethod == ExposureEventsGenerationMethod.DailyAverageEvents,
                NumberOfOralDosesPerDay = ModuleConfig.NumberOfDosesPerDayNonDietaryOral,
                NumberOfDermalDosesPerDay = ModuleConfig.NumberOfDosesPerDayNonDietaryDermal,
                NumberOfInhalationDosesPerDay = ModuleConfig.NumberOfDosesPerDayNonDietaryInhalation,
                NonStationaryPeriod = ModuleConfig.NonStationaryPeriod,
                UseParameterVariability = ModuleConfig.UseParameterVariability,
                SpecifyEvents = ModuleConfig.SpecifyEvents,
                SelectedEvents = [.. ModuleConfig.SelectedEvents]
            };

            var kineticModelCalculators = kineticModelCalculatorFactory
                .CreateHumanKineticModels(data.ActiveSubstances, pbkSimulationSettings);

            localProgress.Update("Computing internal exposures");

            // Create internal concentrations calculator
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);
            var kineticConversionFactorCalculator = new KineticConversionFactorsCalculator(kineticModelCalculators);
            var seedKineticModelParameterSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.BME_DrawKineticModelParameters);
            var kineticModelParametersRandomGenerator = new McraRandomGenerator(seedKineticModelParameterSampling);
            if (ModuleConfig.ExposureType == ExposureType.Acute) {
                // Compute target exposures
                var aggregateIndividualDayExposures = targetExposuresCalculator
                    .ComputeAcute(
                        combinedExternalIndividualDayExposures,
                        data.ActiveSubstances,
                        ModuleConfig.ExposureRoutes,
                        externalExposureUnit,
                        [targetUnit],
                        kineticModelParametersRandomGenerator,
                        progressReport.NewProgressState(80)
                    );
                result.AggregateIndividualDayExposures = aggregateIndividualDayExposures;

                // Compute kinetic conversion factors
                kineticModelParametersRandomGenerator.Reset();
                var kineticConversionFactors = kineticConversionFactorCalculator
                    .ComputeKineticConversionFactors(
                        data.ActiveSubstances,
                        ModuleConfig.ExposureRoutes,
                        combinedExternalIndividualDayExposures,
                        externalExposureUnit,
                        targetUnit,
                        kineticModelParametersRandomGenerator
                    );
                result.KineticConversionFactors = kineticConversionFactors;
                // Create aggregate individual exposures for acute
                var externalIndividualExposures = AggregateIntakeCalculator
                    .CreateCombinedExternalIndividualExposures(
                        [.. combinedExternalIndividualDayExposures]
                    );
                result.ExternalIndividualExposures = externalIndividualExposures;
            } else {
                // Create aggregate individual exposures
                var externalIndividualExposures = AggregateIntakeCalculator
                    .CreateCombinedExternalIndividualExposures(
                        [.. combinedExternalIndividualDayExposures]
                    );
                result.ExternalIndividualExposures = externalIndividualExposures;

                // Compute target exposures
                var aggregateIndividualExposures = targetExposuresCalculator
                    .ComputeChronic(
                        externalIndividualExposures,
                        data.ActiveSubstances,
                        ModuleConfig.ExposureRoutes,
                        externalExposureUnit,
                        [targetUnit],
                        kineticModelParametersRandomGenerator,
                        progressReport.NewProgressState(80)
                    );
                result.AggregateIndividualExposures = aggregateIndividualExposures;

                // Compute kinetic conversion factors
                kineticModelParametersRandomGenerator.Reset();
                var kineticConversionFactors = kineticConversionFactorCalculator
                    .ComputeKineticConversionFactors(
                        data.ActiveSubstances,
                        ModuleConfig.ExposureRoutes,
                        externalIndividualExposures,
                        externalExposureUnit,
                        targetUnit,
                        kineticModelParametersRandomGenerator
                    );
                result.KineticConversionFactors = kineticConversionFactors;
            }

            result.ExternalIndividualDayExposures = combinedExternalIndividualDayExposures;
            result.ExternalExposureUnit = externalExposureUnit;
            result.TargetExposureUnit = targetUnit;
            result.ExposureRoutes = ModuleConfig.ExposureRoutes;
            result.ExternalExposureCollections = externalExposureCollections;
            result.KineticModelCalculators = kineticModelCalculators;

            localProgress.Update(100);

            return result;
        }
    }
}