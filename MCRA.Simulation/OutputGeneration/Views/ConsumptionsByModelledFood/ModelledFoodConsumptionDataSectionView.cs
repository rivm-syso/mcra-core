using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ModelledFoodConsumptionDataSectionView : SectionView<ModelledFoodConsumptionDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var description = $"Number of modelled foods: {Model.Records.Count}.";
            if (Model.UseSamplingWeights) {
                description += " Means and percentiles are weighted with individual sampling weights.";
            }
            sb.AppendDescriptionParagraph(description);

            if (Model.HasHierarchicalData) {
                sb.AppendTable(
                     Model,
                     Model.HierarchySummaryList,
                     "ModelledFoodConsumptionDataTable",
                     ViewBag,
                     "__Id",
                     "__IdParent",
                     "__IsSummaryRecord",
                     caption: "Modelled food consumption statistics.",
                     saveCsv: true,
                     displayLimit: 20,
                     isHierarchical: true
                 );
            } else {
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "ModelledFoodConsumptionDataTable",
                   ViewBag,
                   caption: "Modelled food consumption statistics.",
                   saveCsv: true,
                   header: true
                );
            }
        }
    }
}
