using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ModelledFoodMarketShareDataSectionView : SectionView<ModelledFoodMarketShareDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendDescriptionParagraph($"Number of modelled food market shares: {Model.Records.Count}");
            sb.AppendTable(
               Model,
               Model.Records,
               "ModelledFoodMarketShareTable",
               ViewBag,
               caption: "Modelled food market share information.",
               saveCsv: true,
               header: true
            );
        }
    }
}
