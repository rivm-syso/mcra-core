using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmSurveySummarySectionView : SectionView<HbmSurveySummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            {
                var hiddenProperties = new List<string>();
                if (string.IsNullOrEmpty(Model.Record.Description)) {
                    hiddenProperties.Add("Description");
                }
                sb.AppendTable(
                    Model,
                    new List<HbmSurveySummaryRecord>() { Model.Record },
                    "HumanMonitoringSurveySummaryTable",
                    ViewBag,
                    hiddenProperties: hiddenProperties,
                    caption: "HBM study summary.",
                    saveCsv: true,
                    header: true
                );
            }
        }
    }
}
