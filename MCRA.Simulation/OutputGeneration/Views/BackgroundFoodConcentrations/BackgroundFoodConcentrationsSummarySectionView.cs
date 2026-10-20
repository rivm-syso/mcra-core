using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class BackgroundFoodConcentrationsSummarySectionView : SectionView<BackgroundFoodConcentrationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            // Description
            var description = $"The scope contains {Model.Records.Count} background food concentrations.";
            sb.AppendDescriptionParagraph(description);

            // Table
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Reference))) {
                hiddenProperties.Add(nameof(BackgroundFoodConcentrationRecord.Reference));
            }
     
            sb.AppendTable(
               Model,
               Model.Records,
               "BackgroundFoodConcentrationsTable",
               ViewBag,
               caption: "Background food concentrations.",
               saveCsv: true,
               header: true,
               sortable: true,
               displayLimit: 20,
               hiddenProperties: hiddenProperties
            );
        }
    }
}
