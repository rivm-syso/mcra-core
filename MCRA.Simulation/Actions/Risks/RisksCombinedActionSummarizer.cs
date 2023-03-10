using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.Risks {
    public class RisksCombinedActionSummarizer {

        public ActionType ActionType => ActionType.Risks;

        public void Summarize(
            ICollection<RiskModel> riskModels,
            SectionHeader header
        ) {
            var section = new CombinedRiskPercentilesSection();
            var subHeader = header.AddSubSectionHeaderFor(section, $"{ActionType.GetDisplayName()} - Comparison", 0);
            section.Summarize(riskModels, riskModels.FirstOrDefault().RiskMetric);
            subHeader.SaveSummarySection(section);
        }
    }
}
