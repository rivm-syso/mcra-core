using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.PbpkModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.KineticConversionFactors {

    [ActionType(ActionType.KineticConversionFactors)]
    public sealed class KineticConversionFactorsActionCalculator : ActionCalculatorBase<KineticConversionFactorsActionResult> {
        private KineticConversionFactorsModuleConfig ModuleConfig => (KineticConversionFactorsModuleConfig)_moduleSettings;

        public KineticConversionFactorsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var showActiveSubstances = GetRawDataSources().Any()
                && ModuleConfig.MultipleSubstances
                && !ModuleConfig.FilterByAvailableHazardCharacterisation;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = showActiveSubstances;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = showActiveSubstances;
            _actionDataLinkRequirements[ScopingType.KineticConversionFactors][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            var requirePbkModels = ModuleConfig.IsCompute;
            _actionInputRequirements[ActionType.PbkModels].IsRequired = requirePbkModels;
            _actionInputRequirements[ActionType.PbkModels].IsVisible = requirePbkModels;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleKineticConversionFactors) {
                result.Add(UncertaintySource.KineticConversionFactors);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new KineticConversionFactorsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override void loadData(
            ActionData data,
            SubsetManager subsetManager,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var kineticConversionFactors = subsetManager.AllKineticConversionFactors?.ToList() ?? [];
            var kineticConversionFactorModels =  kineticConversionFactors
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, ModuleConfig.KCFSubgroupDependent)
                )
                .ToList();
            data.KineticConversionFactorModels = kineticConversionFactorModels;
            localProgress.Update(100);
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (data.KineticConversionFactorModels != null && factorialSet.Contains(UncertaintySource.KineticConversionFactors)) {
                localProgress.Update("Resampling kinetic conversion factors.");
                if (data.KineticConversionFactorModels?.Count > 0) {
                    var random = uncertaintySourceGenerators[UncertaintySource.KineticConversionFactors];
                    foreach (var model in data.KineticConversionFactorModels) {
                        model.ResampleModelParameters(random);
                    }
                }
            }
            localProgress.Update(100);
        }

        protected override KineticConversionFactorsActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var result = new KineticConversionFactorsActionResult();
            var localProgress = progressReport.NewProgressState(1);

            var externalExposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            // Determine internal targets and appropriate internal unit
            var internalTargets = ModuleConfig.InternalMatrices
                .Select(r => {
                    var target = new ExposureTarget(r, ExpressionType.None);
                    return new TargetUnit(
                        target,
                        ExposureUnitTriple.CreateDefaultExposureUnit(target, ModuleConfig.ExposureType)
                    );
                })
                .ToList();

            var simulationSettings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = ModuleConfig.NumberOfDays,
                UseRepeatedDailyEvents = ModuleConfig.ExposureEventsGenerationMethod == ExposureEventsGenerationMethod.DailyAverageEvents,
                NumberOfOralDosesPerDay = 1,
                NumberOfDermalDosesPerDay = 1,
                NumberOfInhalationDosesPerDay = 1,
                NonStationaryPeriod = ModuleConfig.NonStationaryPeriod,
                UseParameterVariability = ModuleConfig.UseParameterVariability,
                SpecifyEvents = false,
                SelectedEvents = [],
                OutputResolutionTimeUnit = ModuleConfig.PbkOutputResolutionTimeUnit,
                OutputResolutionStepSize = ModuleConfig.PbkOutputResolutionStepSize,
                PbkSimulationMethod = ModuleConfig.PbkSimulationMethod,
                AllowUseSurrogateMatrix = false,
                SurrogateBiologicalMatrix = BiologicalMatrix.Undefined,
                LifetimeYears = ModuleConfig.LifetimeYears,
                BodyWeightCorrected = ModuleConfig.BodyWeightCorrected,
            };
            var generatorPbkModelSimulation = new McraRandomGenerator(
                RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.KCF_PbkModelSimulation)
            );
            var kcfs = KineticConversionFactorModelCalculator
                .ComputeFromPbk(
                    data.ActiveSubstances,
                    data.KineticModelInstances,
                    simulationSettings,
                    externalExposureUnit,
                    internalTargets,
                    ModuleConfig.ExposureType,
                    ModuleConfig.ExposureRoutes,
                    exposureMin: ModuleConfig.ExposureRangeMinimum,
                    exposureMax: ModuleConfig.ExposureRangeMaximum,
                    numSimulations: ModuleConfig.NumberOfPbkModelSimulations,
                    computeInternalTargetConversionFactors: ModuleConfig.ComputeBetweenInternalTargetConversionFactors,
                    progressState: progressReport.NewProgressState(99),
                    generator: generatorPbkModelSimulation
                );
            result.KineticConversionFactorModels = kcfs;

            localProgress.Update(100);
            return result;
        }

        protected override KineticConversionFactorsActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var result = new KineticConversionFactorsActionResult();
            var localProgress = progressReport.NewProgressState(100);

            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, KineticConversionFactorsActionResult result) {
            data.KineticConversionFactorModels = result.KineticConversionFactorModels;
            base.updateSimulationData(data, result);
        }

        protected override void summarizeActionResult(
            KineticConversionFactorsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new KineticConversionFactorsSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, KineticConversionFactorsActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new KineticConversionFactorsSummarizer(ModuleConfig);
            summarizer.SummarizeUncertain(_actionSettings, actionResult, data, header);
            localProgress.Update(100);
        }
    }
}
