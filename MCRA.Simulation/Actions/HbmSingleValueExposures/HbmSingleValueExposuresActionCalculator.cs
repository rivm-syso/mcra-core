using MCRA.Data.Management;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.HbmSingleValueExposures {

    [ActionType(ActionType.HbmSingleValueExposures)]
    public class HbmSingleValueExposuresActionCalculator : ActionCalculatorBase<HbmSingleValueExposuresActionResult> {
        private HbmSingleValueExposuresModuleConfig ModuleConfig => (HbmSingleValueExposuresModuleConfig)_moduleSettings;

        public HbmSingleValueExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new HbmSingleValueExposuresSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var allHbmSingleValueSets = subsetManager.GetAllHbmSingleValueExposureSets;
            var surveys = allHbmSingleValueSets
                .Select(c => c.Survey)
                .Distinct();
            if (surveys.Count() > 1) {
                //TODO discuss, allow only one survey in xlsx (like the codebook for HBM monitoring data or use a select setting??
                throw new Exception("Only one survey for point estimates (percentiles) is allowed");
            }
            data.HbmSingleValueExposureSets = allHbmSingleValueSets;
        }

        protected override void summarizeActionResult(HbmSingleValueExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var summarizer = new HbmSingleValueExposuresSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
        }

        protected override void updateSimulationData(ActionData data, HbmSingleValueExposuresActionResult result) {
        }
        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, HbmSingleValueExposuresActionResult result) {
            var outputWriter = new HbmSingleValueExposuresOutputWriter();
            outputWriter.WriteOutputData(_project, data, result, rawDataWriter);
        }
    }
}
