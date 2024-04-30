using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.DeterministicSubstanceConversionFactors {

    [ActionType(ActionType.DeterministicSubstanceConversionFactors)]
    public class DeterministicSubstanceConversionFactorsActionCalculator : ActionCalculatorBase<IDeterministicSubstanceConversionFactorsActionResult> {

        public DeterministicSubstanceConversionFactorsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.DeterministicSubstanceConversionFactors][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.DeterministicSubstanceConversionFactors = subsetManager.AllDeterministicSubstanceConversionFactors;
        }

        protected override void summarizeActionResult(IDeterministicSubstanceConversionFactorsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new DeterministicSubstanceConversionFactorsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
