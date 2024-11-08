using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticModelsSummarySectionView : SectionView<KineticModelsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenPropertiesAF = new List<string> { "CompoundName", "CompoundCode" };
            sb.AppendTable(
               Model,
               Model.AbsorptionFactorRecords,
               "AbsorptionFactorsSummarySectionTable",
               ViewBag,
               caption: "Absorption factors summary.",
               saveCsv: true,
               header: true,
               hiddenProperties: hiddenPropertiesAF
            );
        }
    }
}
