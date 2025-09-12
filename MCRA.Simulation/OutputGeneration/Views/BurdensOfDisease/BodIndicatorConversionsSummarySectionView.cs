using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class BodIndicatorConversionsSummarySectionView : SectionView<BodIndicatorConversionsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => c.FromUnit == string.Empty)) {
                hiddenProperties.Add("FromUnit");
            }
            if (Model.Records.All(c => c.ToUnit == string.Empty)) {
                hiddenProperties.Add("ToUnit");
            }
            sb.AppendTable(
                Model,
                Model.Records,
                "BodIndicatorConversionsSummaryTable",
                ViewBag,
                header: true,
                caption: "Bod indicator conversions.",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
