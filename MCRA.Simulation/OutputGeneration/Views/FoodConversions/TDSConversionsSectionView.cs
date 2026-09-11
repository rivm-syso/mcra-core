using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TDSConversionsSectionView : SectionView<TDSConversionsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.FoodConversionSummaryRecords.All(c => c.Regionality == null)) {
                hiddenProperties.Add(nameof(TDSConversionSummaryRecord.Regionality));
            }
            if (Model.FoodConversionSummaryRecords.All(c => c.Seasonality == null)) {
                hiddenProperties.Add(nameof(TDSConversionSummaryRecord.Seasonality));
            }
            if (Model.FoodConversionSummaryRecords.All(c => c.Description == null)) {
                hiddenProperties.Add(nameof(TDSConversionSummaryRecord.Description));
            }

            //Render HTML
            sb.AppendDescriptionParagraph($"Number of TDS conversions: {Model.FoodConversionSummaryRecords.Count}");

            sb.AppendTable(
               Model,
               Model.FoodConversionSummaryRecords,
               "TDSConversionTable",
               ViewBag,
               caption: "Total diet study food conversions.",
               saveCsv: true,
               header: true,
               hiddenProperties: hiddenProperties
            );
        }
    }
}
