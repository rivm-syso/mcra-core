using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueConsumptionsDataSummarySectionView : SectionView<SingleValueConsumptionsDataSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => double.IsNaN(c.Percentile))) {
                hiddenProperties.Add("Percentile");
            }
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Reference))) {
                hiddenProperties.Add("Reference");
            }
            if (Model.Records.All(c => string.IsNullOrEmpty(c.PopulationName))) {
                hiddenProperties.Add("PopulationName");
            }
            if (Model.Records.All(c => string.IsNullOrEmpty(c.PopulationId))) {
                hiddenProperties.Add("PopulationId");
            }
            sb.AppendDescriptionParagraph($"Total {Model.Records.Count} single value consumption estimates.");
            sb.AppendTable(
                Model,
                Model.Records,
                "SingleValueConsumptionsDataTable",
                ViewBag,
                caption: "Single value consumptions data table.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
