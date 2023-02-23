using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryInputDataSectionView : SectionView<NonDietaryInputDataSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (!Model.NonDietaryInputDataRecords.Any()) {
                sb.AppendParagraph("No non-dietary exposures found/available for the specified scope.", "warning");
            } else {
                sb.Append("<div>");
                sb.Append("Summary statistics per survey and substance");
                sb.Append("</div>");
                sb.AppendTable(
                   Model,
                   Model.NonDietaryInputDataRecords,
                   "NonDietaryInputDataTable",
                   ViewBag,
                   caption: "NonDietary input data.",
                   saveCsv: true,
                   header: true
                );
                if (Model.NonDietarySurveyPropertyRecords.Any()) {
                    sb.Append("<div>");
                    sb.Append("Non-dietary survey properties");
                    sb.Append("</div>");
                    sb.AppendTable(
                       Model,
                       Model.NonDietarySurveyPropertyRecords,
                       "NonDietaryInputDataCovariatesTable",
                       ViewBag,
                       caption: "NonDietary input data covariates.",
                       saveCsv: true,
                       header: true
                    );
                }
                sb.Append("<div>");
                sb.Append("Summary statistics per survey");
                sb.Append("</div>");
                sb.AppendTable(
                   Model,
                   Model.NonDietarySurveyProbabilityRecords,
                   "NonDietaryInputDataProbabilitiesTable",
                   ViewBag,
                   caption: "NonDietary input data probabilities.",
                   saveCsv: true,
                   header: true
                );
            }
        }
    }
}
