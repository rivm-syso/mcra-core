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
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), 0);
            subHeader.Units = collectUnits(section);
            section.Summarize(riskModels, riskModels.FirstOrDefault().RiskMetric);
            subHeader.SaveSummarySection(section);
        }

        private List<ActionSummaryUnitRecord> collectUnits(CombinedRiskPercentilesSection section) {
            // TODO: T. van Voorthuijsen, 2026-05-05
            //       The values for the uncertainty lower bound and upper bound should come from the action settings, the same as in
            //       RiskSummarizer.CollectUnits. For this, the main loop action and action calculator instantiation should be modified,
            //       see LoopCalculationTaskExecuter, ActionCalculatorProvider.Create
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerBound", $"p{section.UncertaintyLowerBound:#0.##}"),
                new("UpperBound", $"p{section.UncertaintyUpperBound:#0.##}"),
            };
            return result;
        }
    }
}
