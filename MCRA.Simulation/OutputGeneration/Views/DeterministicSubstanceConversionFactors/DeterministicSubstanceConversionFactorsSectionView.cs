using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DeterministicSubstanceConversionFactorsSectionView : SectionView<DeterministicSubstanceConversionFactorsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model?.Records?.Any() ?? false) {
                sb.AppendDescriptionParagraph($"Total {Model.Records.Count} deterministic substance conversion factors.");
                sb.AppendTable(
                    Model, 
                    Model.Records,
                    "DeterministicSubstanceConversionFactorsTable",
                    ViewBag,
                    caption: "Deterministic substance conversion factors.",
                    header: true,
                    saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph("No deterministic substance conversion factors available.");
            }
        }
    }
}
