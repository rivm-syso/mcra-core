using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureEffectFunctionSummarySectionView : SectionView<ExposureEffectFunctionSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var panelBuilder = new HtmlTabPanelBuilder();

            var panelGroups = Model.Records
                .GroupBy(r => r.ExposureResponseFunctionCode);

            foreach (var panelGroup in panelGroups) {
                var key = $"{panelGroup.Key}";

                var exposureResponseFunction = Model.ExposureEffectFunctions
                    .Single(r => r.Code == panelGroup.Key);

                var attributableBodSummaryRecords = Model.Records
                    .Where(r => r.ExposureResponseFunctionCode == panelGroup.Key)
                    .ToList();

                var chartCreator = new ExposureEffectFunctionChartCreator(
                    attributableBodSummaryRecords,
                    exposureResponseFunction,
                    Model.SectionId
                );

                panelBuilder.AddPanel(
                    id: $"Panel_{key}",
                    title: $"{key}",
                    hoverText: key,
                    content: ChartHelpers.Chart(
                        name: $"ExposureEffectFunctionChart{key}",
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
            if (Model.EefRecords.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add("ExposureRoute");
            }
            if (Model.EefRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            if (Model.EefRecords.All(r => string.IsNullOrEmpty(r.ExpressionType))) {
                hiddenProperties.Add("ExpressionType");
            }

            sb.AppendTable(
                Model,
                Model.EefRecords,
                "ExposureEffectFunctionTable",
                ViewBag,
                caption: "Exposure effect function summary table.",
                saveCsv: true,
                sortable: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
