using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using System.Collections.Generic;

namespace MCRA.Simulation.Actions.Risks {
    public class RisksCombinedActionSummarizer {

        public ActionType ActionType => ActionType.Risks;

        public void Summarize(
            ICollection<RiskModel> exposureModels,
            SectionHeader header
        ) {
            var section = new CombinedRiskPercentilesSection();
            var subHeader = header.AddSubSectionHeaderFor(section, $"{ActionType.GetDisplayName()} - Comparison", 0);
            section.Summarize(exposureModels);
            subHeader.SaveSummarySection(section);
        }
    }
}
