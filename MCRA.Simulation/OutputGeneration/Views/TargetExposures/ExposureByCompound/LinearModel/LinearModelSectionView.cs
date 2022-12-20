using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class LinearModelSectionView : SectionView<LinearModelSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendParagraph("For the following substances a linear model based on absorption factors is used.");
            sb.AppendTable(
               Model,
               Model.Records,
               "LinearModelSubstancesTable",
               ViewBag,
               caption: "Linear model substances.",
               saveCsv: true,
               header: true
            );
        }
    }
}
