using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductExposureAssessmentSummarySectionView : SectionView<ConsumerProductExposureAssessmentSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Description))) {
                hiddenProperties.Add(nameof(ConsumerProductExposureAssessmentRecord.Description));
            }
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Gender))) {
                hiddenProperties.Add(nameof(ConsumerProductExposureAssessmentRecord.Gender));
            }
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Country))) {
                hiddenProperties.Add(nameof(ConsumerProductExposureAssessmentRecord.Country));
            }
            if (Model.Records.All(c => double.IsNaN(c.AgeMin))) {
                hiddenProperties.Add(nameof(ConsumerProductExposureAssessmentRecord.AgeMin));
            }
            if (Model.Records.All(c => double.IsNaN(c.AgeMax))) {
                hiddenProperties.Add(nameof(ConsumerProductExposureAssessmentRecord.AgeMax));
            }

            //Render HTML
            sb.AppendTable(
               Model,
               Model.Records,
               "ConsumerProductExposureAssessmentTable",
               ViewBag,
               caption: "Consumer product exposure assessment summary.",
               saveCsv: true,
               header: true,
               displayLimit: 20,
               hiddenProperties: hiddenProperties
            );
        }
    }
}
