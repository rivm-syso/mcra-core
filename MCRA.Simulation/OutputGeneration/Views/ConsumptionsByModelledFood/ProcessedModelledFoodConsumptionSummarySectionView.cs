using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ProcessedModelledFoodConsumptionSummarySectionView : SectionView<ProcessedModelledFoodConsumptionSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var description = $"Number of modelled food and processing type combinations: {Model.Records.Count}.";
            if (Model.UseSamplingWeights) {
                description += " Means and percentiles are weighted with individual sampling weights.";
            }
            sb.AppendDescriptionParagraph(description);
            sb.AppendTable(
                Model,
                Model.Records,
                "ProcessedModelledFoodConsumptionInputDataTable",
                ViewBag,
                header: true,
                caption: "Processed modelled food and processing type consumption statistics.",
                saveCsv: true
            );
        }
    }
}
