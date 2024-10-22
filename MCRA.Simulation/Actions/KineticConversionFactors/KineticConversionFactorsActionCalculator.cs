using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.KineticConversionFactors {

    [ActionType(ActionType.KineticConversionFactors)]
    public sealed class KineticConversionFactorsActionCalculator : ActionCalculatorBase<IKineticConversionFactorsActionResult> {
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
            return summarizer.Summarize(_isCompute, _project);
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

        protected override void summarizeActionResult(
            IKineticConversionFactorsActionResult actionResult,
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

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (data.KineticConversionFactorModels != null && factorialSet.Contains(UncertaintySource.KineticConversionFactors)) {
                localProgress.Update("Resampling kinetic conversion factors.");
                if (data.KineticConversionFactorModels?.Any() ?? false) {
                    var random = uncertaintySourceGenerators[UncertaintySource.KineticConversionFactors];
                    foreach (var model in data.KineticConversionFactorModels) {
                        model.ResampleModelParameters(random);
                    }
                }
            }
            localProgress.Update(100);
        }
    }
}
