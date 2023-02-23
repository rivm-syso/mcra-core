using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ActiveSubstancesTableSectionView : SectionView<ActiveSubstancesTableSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (string.IsNullOrEmpty(Model.Record.Reference)) {
                hiddenProperties.Add("Reference");
            }
            if (!Model.Record.IsProbabilistic) {
                hiddenProperties.Add("MembershipScoresMean");
                hiddenProperties.Add("MembershipScoresMedian");
                hiddenProperties.Add("ProbableMembershipsCount");
                hiddenProperties.Add("FractionProbableMemberships");
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
