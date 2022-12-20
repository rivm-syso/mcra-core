using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class IntraSpeciesFactorsSummarySectionView : SectionView<IntraSpeciesFactorsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.DefaultIntraSpeciesFactor != 0) {
                sb.AppendDescriptionParagraph($"Default intra-species (safety) factor: {Model.DefaultIntraSpeciesFactor}");
            }
            //Render HTML
            sb.AppendDescriptionParagraph($"Number of intraspecies factors: {Model.Records?.Count ?? 0}");
            sb.AppendTable(
                Model,
                Model.Records,
                "IntraSpeciesFactorRecordsTable",
                ViewBag,
                caption: "Intraspecies factor summary.",
                saveCsv: true,
                header: true
            );
        }
    }
}
