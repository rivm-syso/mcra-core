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
using MCRA.Simulation.Calculators.CombinedExternalExposureCalculation;
using MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.AirExposureGenerators;
using MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.ConsumerProductExposureGenerators;
using MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.DietExposureGenerator;
using MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.DustExposureGenerators;
using MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.NonDietaryExposureGenerators;
using MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.SoilExposureGenerators;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation;
using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Objects;
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

            var requireDust = ModuleConfig.ExposureType == ExposureType.Chronic &&
                ModuleConfig.ExposureSources.Contains(ExposureSource.Dust);
            _actionInputRequirements[ActionType.DustExposures].IsRequired = requireDust;
            _actionInputRequirements[ActionType.DustExposures].IsVisible = requireDust;

            var requireSoil = ModuleConfig.ExposureType == ExposureType.Chronic &&
                ModuleConfig.ExposureSources.Contains(ExposureSource.Soil);
            _actionInputRequirements[ActionType.SoilExposures].IsRequired = requireSoil;
            _actionInputRequirements[ActionType.SoilExposures].IsVisible = requireSoil;

            var requireAir = ModuleConfig.ExposureType == ExposureType.Chronic &&
                ModuleConfig.ExposureSources.Contains(ExposureSource.Air);
            _actionInputRequirements[ActionType.AirExposures].IsRequired = requireAir;
            _actionInputRequirements[ActionType.AirExposures].IsVisible = requireAir;

            var requireConsumerProduct = ModuleConfig.ExposureType == ExposureType.Chronic &&
                ModuleConfig.ExposureSources.Contains(ExposureSource.ConsumerProducts);
            _actionInputRequirements[ActionType.ConsumerProductExposures].IsRequired = requireConsumerProduct;
            _actionInputRequirements[ActionType.ConsumerProductExposures].IsVisible = requireConsumerProduct;
            _actionInputRequirements[ActionType.ConsumerProducts].IsRequired = requireConsumerProduct;
            _actionInputRequirements[ActionType.ConsumerProducts].IsVisible = requireConsumerProduct;

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

            // Compute results
            var result = compute(
                data,
                new CompositeProgressState(progressReport.CancellationToken)
            );

            // TODO, MCR analysis on target (internal) concentrations needs to be implemented
            var substances = data.ActiveSubstances;
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

            // Compute results
            var result = compute(
                data,
                new CompositeProgressState(progressReport.CancellationToken)
            );

            // TODO: find a better way to compute uncertainty factorials
            var substances = data.ActiveSubstances;
            if (substances.Count == 1 || data.CorrectedRelativePotencyFactors != null) {
                // Compute factorial responses
                var uncertaintyFactorialResponses = new List<double>();
                if (ModuleConfig.ExposureType == ExposureType.Acute) {
                    uncertaintyFactorialResponses = (substances.Count > 1)
                        ? [.. result.AggregateIndividualDayExposures
                            .Select(c => c.GetTotalExposureAtTarget(
                                data.TargetExposureUnit.Target,
                                data.CorrectedRelativePotencyFactors,
                                data.MembershipProbabilities
                            ))]
                        : [.. result.AggregateIndividualDayExposures
                            .Select(c => c.GetSubstanceExposure(
                                data.TargetExposureUnit.Target, substances.First()))];
                } else {
                    uncertaintyFactorialResponses = (substances.Count > 1)
                        ? [.. result.AggregateIndividualExposures
                            .Select(c => c.GetTotalExposureAtTarget(
                                data.TargetExposureUnit.Target,
                                data.CorrectedRelativePotencyFactors,
                                data.MembershipProbabilities
                            ))]
                        : [.. result.AggregateIndividualExposures
                            .Select(c => c.GetSubstanceExposure(
                                data.TargetExposureUnit.Target, substances.First()))];
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
            CompositeProgressState progressReport
        ) {
            var result = new TargetExposuresActionResult();
            var localProgress = progressReport.NewProgressState(20);

            ExposureUnitTriple externalExposureUnit;
            ICollection<IIndividualDay> referenceIndividualDays;
            switch (ModuleConfig.IndividualReferenceSet) {
                case ExposureSource.Diet:
                    externalExposureUnit = data.DietaryExposureUnit.ExposureUnit;
                    referenceIndividualDays = [.. data.DietaryIndividualDayIntakes.Cast<IIndividualDay>()];
                    break;
                case ExposureSource.Air:
                    externalExposureUnit = data.AirExposureUnit;
                    referenceIndividualDays = [.. data.IndividualAirExposures.Cast<IIndividualDay>()];
                    break;
                case ExposureSource.Soil:
                    externalExposureUnit = data.SoilExposureUnit;
                    referenceIndividualDays = [.. data.IndividualSoilExposures.Cast<IIndividualDay>()];
                    break;
                case ExposureSource.Dust:
                    externalExposureUnit = data.DustExposureUnit;
                    referenceIndividualDays = data.IndividualDustExposures.
                        Select(r => new SimulatedIndividualDay(r.SimulatedIndividual))
                        .Cast<IIndividualDay>()
                        .ToList();
                    break;
                case ExposureSource.ConsumerProducts:
                    externalExposureUnit = data.ConsumerProductExposureUnit;
                    referenceIndividualDays = data.ConsumerProductIndividualExposures
                        .Select(r => new SimulatedIndividualDay(r.SimulatedIndividual))
                        .Cast<IIndividualDay>()
                        .ToList();
                    break;
                default:
                    throw new NotImplementedException();
            }

            var targetUnit = data.TargetExposureUnit;
            if (targetUnit == null) {
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
            }

            var externalExposureCollections = new List<ExternalExposureCollection>();

            // Align non-dietary exposures
            if (ModuleConfig.ExposureSources.Contains(ExposureSource.OtherNonDiet)) {
                localProgress.Update("Matching dietary and non-dietary exposures");
                var nonDietaryExposuresGenerator = NonDietaryExposureGeneratorFactory
                    .Create(
                        data.NonDietaryExposures,
                        ModuleConfig.NonDietaryPopulationAlignmentMethod,
                        ModuleConfig.IsCorrelationBetweenIndividuals,
                        ModuleConfig.NonDietaryAgeAlignment,
                        ModuleConfig.NonDietarySexAlignment
                    );
                var seedNonDietaryExposuresSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.BME_DrawNonDietaryExposures);
                var nonDietaryExposureCollection = nonDietaryExposuresGenerator?
                    .Generate(
                        referenceIndividualDays,
                        data.NonDietaryExposures.Keys,
                        data.NonDietaryExposureUnit,
                        data.ActiveSubstances,
                        ModuleConfig.ExposureRoutes,
                        ModuleConfig.ExposureType,
                        seedNonDietaryExposuresSampling
                    );
                externalExposureCollections.Add(nonDietaryExposureCollection);
            }

            // Align dust exposures
            if (ModuleConfig.ExposureSources.Contains(ExposureSource.Dust)) {
                localProgress.Update("Matching dust exposures");
                var dustExposureCalculator = DustExposureGeneratorFactory.Create(
                    ModuleConfig.DustPopulationAlignmentMethod,
                    ModuleConfig.DustAgeAlignment,
                    ModuleConfig.DustSexAlignment,
                    ModuleConfig.DustAgeAlignmentMethod,
                    ModuleConfig.DustAgeBins
                );
                var seedDustExposuresSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_DrawDustExposures);
                var dustExposureCollection = dustExposureCalculator
                    .Generate(
                        referenceIndividualDays,
                        data.ActiveSubstances,
                        data.IndividualDustExposures,
                        data.DustExposureUnit.SubstanceAmountUnit,
                        seedDustExposuresSampling
                    );
                externalExposureCollections.Add(dustExposureCollection);
            }

            // Align soil exposures
            if (ModuleConfig.ExposureSources.Contains(ExposureSource.Soil)) {
                localProgress.Update("Matching soil exposures");
                var soilExposureCalculator = SoilExposureGeneratorFactory.Create(
                    ModuleConfig.SoilPopulationAlignmentMethod,
                    ModuleConfig.SoilAgeAlignment,
                    ModuleConfig.SoilSexAlignment,
                    ModuleConfig.SoilAgeAlignmentMethod,
                    ModuleConfig.SoilAgeBins
                );
                var seedSoilExposuresSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DUE_DrawSoilExposures);
                var soilExposureCollection = soilExposureCalculator
                    .Generate(
                        referenceIndividualDays,
                        data.ActiveSubstances,
                        data.IndividualSoilExposures,
                        data.SoilExposureUnit.SubstanceAmountUnit,
                        seedSoilExposuresSampling
                    );
                externalExposureCollections.Add(soilExposureCollection);
            }

            // Align air exposures
            if (ModuleConfig.ExposureSources.Contains(ExposureSource.Air)) {
                localProgress.Update("Matching air exposures");
                var airExposureCalculator = AirExposureGeneratorFactory.Create(
                    ModuleConfig.AirPopulationAlignmentMethod,
                    ModuleConfig.AirAgeAlignment,
                    ModuleConfig.AirSexAlignment,
                    ModuleConfig.AirAgeAlignmentMethod,
                    ModuleConfig.AirAgeBins
                );

                var seedAirExposuresSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.AIE_DrawAirExposures);
                var airExposureCollection = airExposureCalculator
                    .Generate(
                        referenceIndividualDays,
                        data.ActiveSubstances,
                        data.IndividualAirExposures,
                        data.AirExposureUnit.SubstanceAmountUnit,
                        seedAirExposuresSampling
                    );
                externalExposureCollections.Add(airExposureCollection);
            }

            // Align consumer product exposures
            if (ModuleConfig.ExposureSources.Contains(ExposureSource.ConsumerProducts)) {
                localProgress.Update("Matching consumer product exposures");
                ExternalExposureCollection cpExposureCollection = null;
                if (ModuleConfig.IndividualReferenceSet == ExposureSource.ConsumerProducts) {
                    cpExposureCollection = new ExternalExposureCollection {
                        SubstanceAmountUnit = targetUnit.SubstanceAmountUnit,
                        ExposureSource = ExposureSource.ConsumerProducts,
                        ExternalIndividualDayExposures = data.ConsumerProductIndividualExposures
                            .AsParallel()
                            .Select(c => ExternalIndividualDayExposure.FromConsumerProductIndividualIntake(c, ModuleConfig.ExposureRoutes))
                            .Cast<IExternalIndividualDayExposure>()
                            .ToList()
                    };
                    externalExposureCollections.Add(cpExposureCollection);
                } else {
                    var cpExposureCalculator = ConsumerProductExposureGeneratorFactory.Create(
                        ModuleConfig.ConsumerProductPopulationAlignmentMethod,
                        ModuleConfig.ConsumerProductAgeAlignment,
                        ModuleConfig.ConsumerProductSexAlignment,
                        ModuleConfig.ConsumerProductAgeAlignmentMethod,
                        ModuleConfig.ConsumerProductAgeBins
                    );

                    var seedCPExposuresSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.CPE_ConsumerProductExposureDeterminants);
                    cpExposureCollection = cpExposureCalculator
                        .Generate(
                            referenceIndividualDays,
                            data.ActiveSubstances,
                            data.ConsumerProductIndividualExposures,
                            data.ConsumerProductExposureUnit.SubstanceAmountUnit,
                            seedCPExposuresSampling
                        );
                    externalExposureCollections.Add(cpExposureCollection);
                }
            }

            // Align dietary exposures
            if (ModuleConfig.ExposureSources.Contains(ExposureSource.Diet)) {
                localProgress.Update("Matching dietary exposures");
                ExternalExposureCollection dietExposureCollection = null;
                if (ModuleConfig.IndividualReferenceSet == ExposureSource.Diet) {
                    dietExposureCollection = new ExternalExposureCollection {
                        SubstanceAmountUnit = data.DietaryExposureUnit.SubstanceAmountUnit,
                        ExposureSource = ExposureSource.Diet,
                        ExternalIndividualDayExposures = data.DietaryIndividualDayIntakes
                            .AsParallel()
                            .Select(ExternalIndividualDayExposure.FromDietaryIndividualDayIntake)
                            .Cast<IExternalIndividualDayExposure>()
                            .ToList()
                    };
                } else {
                    var dietExposureCalculator = DietExposureGeneratorFactory.Create(
                        ModuleConfig.DietPopulationAlignmentMethod,
                        ModuleConfig.DietAgeAlignment,
                        ModuleConfig.DietSexAlignment,
                        ModuleConfig.DietAgeAlignmentMethod,
                        ModuleConfig.DietAgeBins
                    );
                    var seedDietExposuresSampling = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DIE_DrawDietExposures);
                    dietExposureCollection = dietExposureCalculator
                        .Generate(
                            referenceIndividualDays,
                            data.ActiveSubstances,
                            data.DietaryIndividualDayIntakes,
                            data.DietaryExposureUnit.SubstanceAmountUnit,
                            seedDietExposuresSampling
                        );
                }
                externalExposureCollections.Add(dietExposureCollection);
            }

            // Combine all external exposures
            var combinedExternalIndividualDayExposures = CombinedExternalExposuresCalculator
                .CreateCombinedIndividualDayExposures(
                    externalExposureCollections,
                    externalExposureUnit,
                    ModuleConfig.ExposureType,
                    progressReport.CancellationToken
                );

            // Create kinetic model calculators
            var kineticModelCalculatorFactory = new KineticConversionCalculatorFactory(
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
                NonStationaryPeriod = ModuleConfig.NonStationaryPeriodInDays,
                UseParameterVariability = ModuleConfig.UseParameterVariability,
                SpecifyEvents = ModuleConfig.SpecifyEvents,
                SelectedEvents = [.. ModuleConfig.SelectedEvents],
                OutputResolutionTimeUnit = ModuleConfig.PbkOutputResolutionTimeUnit,
                OutputResolutionStepSize = ModuleConfig.PbkOutputResolutionStepSize,
                PbkSimulationMethod = ModuleConfig.PbkSimulationMethod,
                AllowUseSurrogateMatrix = ModuleConfig.AllowUseSurrogateMatrix,
                SurrogateBiologicalMatrix = ModuleConfig.SurrogateBiologicalMatrix,
                LifetimeYears = ModuleConfig.LifetimeYears,
                BodyWeightCorrected = ModuleConfig.BodyWeightCorrected,
            };
            var kineticModelCalculators = kineticModelCalculatorFactory
                .CreateHumanKineticModels(data.ActiveSubstances, pbkSimulationSettings);

            localProgress.Update("Computing internal exposures", 20);

            // Create internal concentrations calculator
            var kineticConversionCalculatorProvider = new KineticConversionCalculatorProvider(kineticModelCalculators);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticConversionCalculatorProvider);

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
                var kineticConversionFactorCalculator = new KineticConversionFactorsCalculator(kineticConversionCalculatorProvider);
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
                var externalIndividualExposures = CombinedExternalExposuresCalculator
                    .CreateCombinedExternalIndividualExposures(
                        [.. combinedExternalIndividualDayExposures],
                        ModuleConfig.ExposureRoutes
                    );
                result.ExternalIndividualExposures = externalIndividualExposures;
            } else {
                // Create aggregate individual exposures
                var externalIndividualExposures = CombinedExternalExposuresCalculator
                    .CreateCombinedExternalIndividualExposures(
                        [.. combinedExternalIndividualDayExposures],
                        ModuleConfig.ExposureRoutes
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
                var kineticConversionFactorCalculator = new KineticConversionFactorsCalculator(kineticConversionCalculatorProvider);
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
            result.KineticConversionCalculators = kineticModelCalculators;

            localProgress.Update(100);

            return result;
        }
    }
}