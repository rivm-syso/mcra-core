using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TdsReductionFactorsSectionView : SectionView<TdsReductionFactorsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            if (Model.Records.Any()) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "TDSReductionFactorsTable",
                    ViewBag,
                    header: true,
                    caption: "Reduction factors for reduction to limit scenario.",
                    saveCsv: true
                );
            } else {
                sb.AppendParagraph("No reduction factors found for the specified foods.");
            }

        }
    }
}
