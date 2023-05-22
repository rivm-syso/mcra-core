using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DietaryExposures {
    public class DietaryExposuresCombinedActionSummarizer {

        public ActionType ActionType => ActionType.DietaryExposures;

        public void Summarize(
            ICollection<DietaryExposureModel> exposureModels,
            SectionHeader header
        ) {
            var section = new CombinedDietaryExposurePercentilesSection();
            var subHeader = header.AddSubSectionHeaderFor(section, $"{ActionType.GetDisplayName()}", 0);
            section.Summarize(exposureModels);
            subHeader.SaveSummarySection(section);
        }
    }
}
