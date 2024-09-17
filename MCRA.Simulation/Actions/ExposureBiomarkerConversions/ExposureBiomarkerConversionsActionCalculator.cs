using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion.ExposureBiomarkerConversionModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ExposureBiomarkerConversions {

    [ActionType(ActionType.ExposureBiomarkerConversions)]
    public class ExposureBiomarkerConversionsActionCalculator : ActionCalculatorBase<IExposureBiomarkerConversionsActionResult> {
        private ExposureBiomarkerConversionsModuleConfig ModuleConfig => (ExposureBiomarkerConversionsModuleConfig)_moduleSettings;

        public ExposureBiomarkerConversionsActionCalculator(ProjectDto project) : base(project) {
            _actionDataLinkRequirements[ScopingType.ExposureBiomarkerConversions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void verify() {
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ExposureBiomarkerConversionsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute ,_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.ExposureBiomarkerConversions = subsetManager.AllExposureBiomarkerConversions;
            data.ExposureBiomarkerConversionModels = data.ExposureBiomarkerConversions?
                .Select(c => ExposureBiomarkerConversionCalculatorFactory
                    .Create(c, ModuleConfig.EBCSubgroupDependent)
                ).ToList();
        }

        protected override void summarizeActionResult(IExposureBiomarkerConversionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ExposureBiomarkerConversionsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
