using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureBiomarkerConversionsSummarySectionView : SectionView<ExposureBiomarkerConversionsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Distribution))) {
                hiddenProperties.Add("Distribution");
            }
            if (Model.Records.All(c => double.IsNaN(c.VariabilityUpper))) {
                hiddenProperties.Add("CV uncertainty");
            }
            //Render HTML
            sb.AppendDescriptionParagraph($"Number of exposure biomarker conversions: {Model.Records.Count}");
            sb.AppendTable(
               Model,
               Model.Records,
               "ExposureBiomarkerConversionsTable",
               ViewBag,
               caption: "Exposure biomarker conversions.",
               saveCsv: true,
               header: true
            );
        }
    }
}
