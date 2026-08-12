using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ConsumerProductExposureEstimates {

    [ActionType(ActionType.ConsumerProductExposureEstimates)]
    public class ConsumerProductExposureEstimatesActionCalculator(ProjectDto project) : ActionCalculatorBase<IConsumerProductExposureEstimatesActionResult>(project) {

        private ConsumerProductExposureEstimatesModuleConfig ModuleConfig => (ConsumerProductExposureEstimatesModuleConfig)_moduleSettings;

        protected override void verify() {
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var assessments = subsetManager.AllConsumerProductExposureAssessments
                .Where(c => c.Value.ExposureType == ModuleConfig.ExposureType)
                .ToDictionary(c => c.Key, c => c.Value);
            var checkEstimateType = !assessments.Values
                .SelectMany(c => c.ExposureSets.Select(c => c.EstimateType))
                .Contains(ConsumerProductExposureEstimateType.Systemic);
            if (checkEstimateType) {
                throw new Exception("Consumer product exposure assessments must be of type 'Systemic'.");
            }
            data.ConsumerProductExposureAssessments = assessments;
        }

        protected override void summarizeActionResult(IConsumerProductExposureEstimatesActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing consumer product exposure estimates", 0);
            var summarizer = new ConsumerProductExposureEstimatesSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
