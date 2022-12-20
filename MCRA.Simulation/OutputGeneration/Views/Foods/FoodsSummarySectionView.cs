using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class FoodsSummarySectionView : SectionView<FoodsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            if (!Model.Records?.Any(r => !string.IsNullOrEmpty(r.CodeParent)) ?? true) {
                hiddenProperties.Add("CodeParent");
            }
            if (!Model.Records?.Any(r => !string.IsNullOrEmpty(r.BaseFoodCode)) ?? true) {
                hiddenProperties.Add("BaseFoodCode");
                hiddenProperties.Add("BaseFoodName");
            }
            if (!Model.Records?.Any(r => !string.IsNullOrEmpty(r.TreatmentCodes)) ?? true) {
                hiddenProperties.Add("TreatmentCodes");
                hiddenProperties.Add("TreatmentNames");
            }
            if (!Model.Records?.Any(r => r.DefaultUnitWeightRacQualifiedValue != null) ?? true) {
                hiddenProperties.Add("DefaultUnitWeightRac");
            }
            if (!Model.Records?.Any(r => r.DefaultUnitWeightEpQualifiedValue != null) ?? true) {
                hiddenProperties.Add("DefaultUnitWeightEp");
            }
            if (!Model.Records?.Any(r => r.LocationUnitWeightsRacValues?.Any() ?? false) ?? true) {
                hiddenProperties.Add("LocationUnitWeightsRac");
            }
            if (!Model.Records?.Any(r => r.LocationUnitWeightsEpValues?.Any() ?? false) ?? true) {
                hiddenProperties.Add("LocationUnitWeightsEp");
            }

            //Render HTML
            sb.AppendDescriptionParagraph($"Number of records: { Model.Records?.Count ?? 0}");

            if (Model.Records?.Any() ?? false) {
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
