using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class FoodAsEatenConsumptionDataSectionView : SectionView<FoodAsEatenConsumptionDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var tableClasses = new List<string>() { "sortable hierarchical" };

            var description = $"Number of food as eaten: {Model.Records.Count}.";
            if (Model.UseSamplingWeights) {
                description += " Means and percentiles are weighted with individual sampling weights.";
            }
            sb.AppendDescriptionParagraph(description);

            var hiddenProperties = new List<string>();

            if (!Model.Records?.Any(r => !string.IsNullOrEmpty(r.BaseFoodCode)) ?? true) {
                hiddenProperties.Add("BaseFoodCode");
                hiddenProperties.Add("BaseFoodName");
            }
            if (!Model.Records?.Any(r => !string.IsNullOrEmpty(r.TreatmentCodes)) ?? true) {
                hiddenProperties.Add("TreatmentCodes");
                hiddenProperties.Add("TreatmentNames");
            }

            if (Model.HasHierarchicalData) {
                sb.AppendTable(
                    Model,
                    Model.HierarchySummaryList,
                    "FoodAsEatenConsumptionDataTable",
                    ViewBag,
                    "__Id",
                    "__IdParent",
                    "__IsSummaryRecord",
                    caption: "Consumption statistics by food as eaten food.",
                    saveCsv: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties,
                    tableClasses: tableClasses,
                    isHierarchical: true
                 );

            } else {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "FoodAsEatenConsumptionDataTable",
                    ViewBag,
                    saveCsv: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties,
                    caption: "Food consumption statistics."
                );
            }
        }
    }
}
