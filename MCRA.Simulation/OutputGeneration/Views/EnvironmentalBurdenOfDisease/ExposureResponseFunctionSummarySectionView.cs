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
                hiddenProperties.Add(nameof(ErfSummaryRecord.ExposureRoute));
            }
            if (Model.ErfSummaryRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add(nameof(ErfSummaryRecord.BiologicalMatrix));
            }
            if (Model.ErfSummaryRecords.All(r => string.IsNullOrEmpty(r.ExpressionType))) {
                hiddenProperties.Add(nameof(ErfSummaryRecord.ExpressionType));
            }
            if (Model.ErfSummaryRecords.All(r => string.IsNullOrEmpty(r.ErSpecificationUncertaintyType))) {
                hiddenProperties.Add(nameof(ErfSummaryRecord.ErSpecificationUncertaintyType));
                hiddenProperties.Add(nameof(ErfSummaryRecord.ErSpecificationUncLower));
                hiddenProperties.Add(nameof(ErfSummaryRecord.ErSpecificationUncUpper));
            } else if (Model.ErfSummaryRecords.All(r => string.IsNullOrEmpty(r.ErSpecificationUncLower))) {
                hiddenProperties.Add(nameof(ErfSummaryRecord.ErSpecificationUncLower));
            } else if (Model.ErfSummaryRecords.All(r => string.IsNullOrEmpty(r.ErSpecificationUncUpper))) {
                hiddenProperties.Add(nameof(ErfSummaryRecord.ErSpecificationUncUpper));
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
