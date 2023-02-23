using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TdsPotentialReductionFactorsSectionView : SectionView<TdsPotentialReductionFactorsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if(Model.Records.Any(r => r.ReductionFactor < 1)) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "TdsPotentialReductionFactorsTable",
                    ViewBag,
                    header: true,
                    caption: "Potential reductions of reduction to limit scenarios.",
                    saveCsv: true
                );
            } else {
                sb.AppendParagraph("No potential reductions found.");
            }
        }
    }
}
