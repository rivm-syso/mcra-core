using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ActiveSubstancesTableSectionView : SectionView<ActiveSubstancesTableSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (string.IsNullOrEmpty(Model.Record.Reference)) {
                hiddenProperties.Add(nameof(ActiveSubstanceModelRecord.Reference));
            }
            if (!Model.Record.IsProbabilistic) {
                hiddenProperties.Add(nameof(ActiveSubstanceModelRecord.MembershipScoresMean));
                hiddenProperties.Add(nameof(ActiveSubstanceModelRecord.MembershipScoresMedian));
                hiddenProperties.Add(nameof(ActiveSubstanceModelRecord.ProbableMembershipsCount));
                hiddenProperties.Add(nameof(ActiveSubstanceModelRecord.FractionProbableMemberships));
            }

            //Render HTML
            sb.AppendTable(
                Model,
                Model.Record.MembershipProbabilities,
                "ActiveSubstancesRecordsTable",
                ViewBag,
                caption: "Active substances.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
