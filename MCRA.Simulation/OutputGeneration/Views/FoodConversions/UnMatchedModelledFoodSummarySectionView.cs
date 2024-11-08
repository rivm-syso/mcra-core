using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UnMatchedModelledFoodSummarySectionView : SectionView<UnMatchedModelledFoodSummarySection> {
        public override
            void RenderSectionHtml(StringBuilder sb) {

            //Render HTML


            if (Model.Records.Count > 0) {
                var hiddenProperties = new List<string>() {
                    "CodeParent",
                    "NameUnprocessedFood",
                    "DefaultUnitWeightRac",
                    "LocationUnitWeightsRac",
                    "CodeUnprocessedFood",
                    "DefaultUnitWeightEp",
                    "LocationUnitWeightsEp"
                };
                sb.AppendDescriptionParagraph($"Number of modelled foods not found in conversion: {Model.Records.Count}");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "UnmatchedModelledFoodsTable",
                    ViewBag,
                    caption: "Unmatched modelled foods.",
                    header: true,
                    saveCsv: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("All modelled foods are part of the foods as eaten (found in conversion)");
            }
        }
    }
}
