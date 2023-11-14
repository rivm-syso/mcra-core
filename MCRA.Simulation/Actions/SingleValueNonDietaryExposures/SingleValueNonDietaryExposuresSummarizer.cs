using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.SingleValueNonDietaryExposures {
    public enum SingleValueNonDietaryExposures {
        //No sub-sections
    }
    public sealed class SingleValueNonDietaryExposuresSummarizer : ActionResultsSummarizerBase<SingleValueNonDietaryExposuresActionResult> {

        public override ActionType ActionType => ActionType.SingleValueNonDietaryExposures;

        public override void Summarize(
            ProjectDto project,
            SingleValueNonDietaryExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
        }
    }
}
