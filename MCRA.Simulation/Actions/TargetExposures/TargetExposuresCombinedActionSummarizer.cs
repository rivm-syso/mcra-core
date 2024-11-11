using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.TargetExposures {
    public class TargetExposuresCombinedActionSummarizer {

        public ActionType ActionType => ActionType.TargetExposures;

        public void Summarize(
            ICollection<TargetExposureModel> exposureModels,
            SectionHeader header
        ) {
            var section = new CombinedTargetExposurePercentilesSection();
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), 0);
            section.Summarize(exposureModels);
            subHeader.SaveSummarySection(section);
        }
    }
}
