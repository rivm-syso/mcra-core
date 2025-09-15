using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmSingleValueExposuresSummarySectionView : SectionView<HbmSingleValueExposuresSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            if (string.IsNullOrEmpty(Model.Record.Description)) {
                hiddenProperties.Add("Description");
            }

            //Render HTML
            sb.AppendTable(
                Model,
                [Model.Record],
                "SurveyPointEstimatesTable",
                ViewBag,
                header: true,
                caption: "Survey for point estimates",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties,
                rotate: true
            );

            foreach (var model in Model.Percentiles) {
                var test = model.PercentileRecords;
                sb.AppendTable(
                    Model,
                    test,
                    $"Percentiles{model.SubstanceCode}Table",
                    ViewBag,
                    header: true,
                    caption: $"{model.SubstanceName} in {model.BiologicalMatrix} ({model.DoseUnit})",
                    saveCsv: true,
                    sortable: true,
                    rotate: true
                );
            }
        }
    }
}
