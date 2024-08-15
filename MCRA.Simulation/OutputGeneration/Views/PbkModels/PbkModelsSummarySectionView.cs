using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PbkModelsSummarySectionView : SectionView<PbkModelsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendTable(
               Model,
               Model.Records,
               "KineticModelsSummarySectionTable",
               ViewBag,
               caption: "Kinetic models summary.",
               saveCsv: true,
               header: true
            );
            if (Model.SubstanceGroupRecords?.Any() ?? false) {
                sb.AppendTable(
                    Model,
                    Model.SubstanceGroupRecords,
                    "KineticModelsSubstanceGroupSummarySectionTable",
                    ViewBag,
                    caption: "Kinetic models parent and metabolites.",
                    saveCsv: true,
                    header: true
                );
            }
        }
    }
}
