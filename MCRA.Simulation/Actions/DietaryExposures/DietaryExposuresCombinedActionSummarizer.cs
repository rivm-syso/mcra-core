using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.DietaryExposures {
    public class DietaryExposuresCombinedActionSummarizer {

        public ActionType ActionType => ActionType.DietaryExposures;

        public void Summarize(
            ICollection<DietaryExposureModel> exposureModels,
            SectionHeader header
        ) {
            var section = new CombinedDietaryExposurePercentilesSection();
            var subHeader = header.AddSubSectionHeaderFor(section, $"{ActionType.GetDisplayName()} - Comparison", 0);
            section.Summarize(exposureModels);
            subHeader.SaveSummarySection(section);
        }
    }
}
