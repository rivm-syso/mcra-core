using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ResponseSummarySectionView : SectionView<ResponseSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.GuidelineMethod))) {
                hiddenProperties.Add("GuidelineMethod");
            }

            //Render HTML
            sb.AppendDescriptionParagraph($"Number of responses: {Model.Records.Count}");
            sb.AppendTable(
               Model,
               Model.Records,
               "ResponseRecordsTable",
               ViewBag,
               caption: "Responses summary.",
               saveCsv: true,
               header: true,
               hiddenProperties: hiddenProperties
            );
        }
    }
}
