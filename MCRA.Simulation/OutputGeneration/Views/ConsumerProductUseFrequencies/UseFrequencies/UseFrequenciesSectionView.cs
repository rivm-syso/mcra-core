using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UseFrequenciesSectionView : SectionView<UseFrequenciesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (Model.Records.Any()) {
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "ConsumerProductsUseFrequenciesTable",
                   ViewBag,
                   caption: "Use frequency statistics for consumer products.",
                   saveCsv: true,
                   header: true,
                   sortable: true,
                   displayLimit: 20
                );
            }
        }
    }
}
