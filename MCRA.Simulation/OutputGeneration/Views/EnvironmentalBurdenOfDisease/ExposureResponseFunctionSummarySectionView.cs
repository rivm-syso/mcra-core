using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureResponseFunctionSummarySectionView : SectionView<ExposureResponseFunctionSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var panelBuilder = new HtmlTabPanelBuilder();

            foreach (var record in Model.ErfSummaryRecords) {

                var chartCreator = new ExposureResponseFunctionChartCreator(
                    record,
                    Model.SectionId,
                    Model.UncertaintyLowerLimit,
                    Model.UncertaintyUpperLimit
                );

                var key = $"{record.ErfCode}";
                panelBuilder.AddPanel(
                    id: $"Panel_{key}",
                    title: $"{key}",
                    hoverText: key,
                    content: ChartHelpers.Chart(
                        name: $"ExposureResponseFunctionChart{key}",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true
                    )
                );
            }
            panelBuilder.RenderPanel(sb);

            var hiddenProperties = new List<string>();
            if (Model.ErfSummaryRecords.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add("ExposureRoute");
            }
            if (Model.ErfSummaryRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            if (Model.ErfSummaryRecords.All(r => string.IsNullOrEmpty(r.ExpressionType))) {
                hiddenProperties.Add("ExpressionType");
            }
            if (Model.ErfSummaryRecords.All(r => string.IsNullOrEmpty(r.ExposureResponseSpecificationLower))) {
                hiddenProperties.Add("ExposureResponseSpecificationLower");
            }
            if (Model.ErfSummaryRecords.All(r => string.IsNullOrEmpty(r.ExposureResponseSpecificationUpper))) {
                hiddenProperties.Add("ExposureResponseSpecificationUpper");
            }

            sb.AppendTable(
                Model,
                Model.ErfSummaryRecords,
                "ExposureResponseFunctionTable",
                ViewBag,
                caption: "Exposure response function summary table.",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
