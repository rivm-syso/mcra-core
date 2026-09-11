using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class FoodsSummarySectionView : SectionView<FoodsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            if (!Model.Records?.Any(r => !string.IsNullOrEmpty(r.CodeParent)) ?? true) {
                hiddenProperties.Add(nameof(FoodsSummaryRecord.CodeParent));
            }
            if (!Model.Records?.Any(r => !string.IsNullOrEmpty(r.BaseFoodCode)) ?? true) {
                hiddenProperties.Add(nameof(FoodsSummaryRecord.BaseFoodCode));
                hiddenProperties.Add(nameof(FoodsSummaryRecord.BaseFoodName));
            }
            if (!Model.Records?.Any(r => !string.IsNullOrEmpty(r.TreatmentCodes)) ?? true) {
                hiddenProperties.Add(nameof(FoodsSummaryRecord.TreatmentCodes));
                hiddenProperties.Add(nameof(FoodsSummaryRecord.TreatmentNames));
            }
            if (!Model.Records?.Any(r => r.DefaultUnitWeightRacQualifiedValue != null) ?? true) {
                hiddenProperties.Add(nameof(FoodsSummaryRecord.DefaultUnitWeightRac));
            }
            if (!Model.Records?.Any(r => r.DefaultUnitWeightEpQualifiedValue != null) ?? true) {
                hiddenProperties.Add(nameof(FoodsSummaryRecord.DefaultUnitWeightEp));
            }
            if (!Model.Records?.Any(r => r.LocationUnitWeightsRacValues?.Count > 0) ?? true) {
                hiddenProperties.Add(nameof(FoodsSummaryRecord.LocationUnitWeightsRac));
            }
            if (!Model.Records?.Any(r => r.LocationUnitWeightsEpValues?.Count > 0) ?? true) {
                hiddenProperties.Add(nameof(FoodsSummaryRecord.LocationUnitWeightsEp));
            }

            //Render HTML
            sb.AppendDescriptionParagraph($"Number of records: { Model.Records?.Count ?? 0}");

            if (Model.Records?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "FoodsTable",
                    ViewBag,
                    header: true,
                    caption: "Food definitions",
                    saveCsv: true,
                    sortable: true,
                    hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
