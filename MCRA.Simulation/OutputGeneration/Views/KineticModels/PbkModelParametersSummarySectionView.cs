using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PbkModelParametersSummarySectionView : SectionView<PbkModelParametersSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> {
                "SubstanceCode",
                "SubstanceName"
            };
            sb.AppendTable(
                Model,
                Model.Records,
                "PBKModelParametersTable",
                ViewBag,
                header: true,
                caption: "PBK model parameters.",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
