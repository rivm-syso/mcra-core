using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.IntraSpeciesFactors {

    [ActionType(ActionType.IntraSpeciesFactors)]
    public sealed class IntraSpeciesFactorsActionCalculator : ActionCalculatorBase<IIntraSpeciesFactorsActionResult> {
        private IntraSpeciesFactorsModuleConfig ModuleConfig => (IntraSpeciesFactorsModuleConfig)_moduleSettings;

        public IntraSpeciesFactorsActionCalculator(ProjectDto project) : base(project) {
            var showActiveSubstances = ModuleConfig.MultipleSubstances
                && !ModuleConfig.FilterByAvailableHazardCharacterisation;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = showActiveSubstances;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = showActiveSubstances;
            _actionDataLinkRequirements[ScopingType.IntraSpeciesModelParameters][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.IntraSpeciesModelParameters][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = base.GetRandomSources();
            if (ModuleConfig.ReSampleIntraSpecies) {
                result.Add(UncertaintySource.IntraSpecies);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new IntraSpeciesFactorsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override void loadDefaultData(ActionData data) {
            var intraSpeciesFactorModelBuilder = new IntraSpeciesFactorModelBuilder();
            data.IntraSpeciesFactors = null;
            data.IntraSpeciesFactorModels = intraSpeciesFactorModelBuilder.Create(
                data.AllEffects,
                data.ActiveSubstances,
                null,
                ModuleConfig.DefaultIntraSpeciesFactor
            );
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var intraSpeciesFactors = subsetManager.AllIntraSpeciesFactors;
            var intraSpeciesFactorModelBuilder = new IntraSpeciesFactorModelBuilder();
            var intraSpeciesFactorModels = intraSpeciesFactorModelBuilder.Create(
                data.AllEffects,
                data.ActiveSubstances,
                intraSpeciesFactors,
                ModuleConfig.DefaultIntraSpeciesFactor
            );
            data.IntraSpeciesFactors = intraSpeciesFactors;
            data.IntraSpeciesFactorModels = intraSpeciesFactorModels;
            localProgress.Update(100);
        }

        protected override void summarizeActionResult(IIntraSpeciesFactorsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            if (data.IntraSpeciesFactors != null) {
                var summarizer = new IntraSpeciesFactorsSummarizer(ModuleConfig);
                summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            }
            localProgress.Update(100);
        }

        protected override void loadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            if (factorialSet.Contains(UncertaintySource.IntraSpecies)) {
                var intraSpeciesFactorCalculator = new IntraSpeciesFactorModelBuilder();
                data.IntraSpeciesFactorModels = intraSpeciesFactorCalculator.Resample(
                    data.IntraSpeciesFactorModels,
                    uncertaintySourceGenerators[UncertaintySource.IntraSpecies]
                );
            }
            localProgress.Update(100);
        }
    }
}
