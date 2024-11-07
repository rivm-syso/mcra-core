using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmSamplesSummarySectionView : SectionView<HbmSamplesSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add("ExposureRoute");
            }

            sb.AppendTable(
                Model,
                Model.Records,
                "HumanMonitoringSamplesSummaryTable",
                ViewBag,
                caption: "Human biomonitoring samples summary.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
