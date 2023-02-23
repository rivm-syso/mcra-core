using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class MarketSharesSummarySectionView : SectionView<MarketSharesSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            //Render HTML
            sb.AppendDescriptionParagraph($"Number of marketshares: {Model.Records.Count}");
            sb.AppendTable(
               Model,
               Model.Records,
               "MarketSharesTable",
               ViewBag,
               caption: "Market shares.",
               saveCsv: true,
               header: true
            );
        }
    }
}
