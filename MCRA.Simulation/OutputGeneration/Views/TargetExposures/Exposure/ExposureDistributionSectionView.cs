using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureDistributionSectionView : SectionView<ExposureDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var panelBuilder = new HtmlTabPanelBuilder();
            {
                var chartCreator = new BoxPlotChartCreator(
                    Model,
                    Model.BoxPlotRecords,
                    Model.TargetUnit,
                    Model.ShowOutliers,
                    false
                );

                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    name: $"BoxPlotData",
                    section: Model,
                    items: Model.BoxPlotRecords,
                    viewBag: ViewBag
                );
                panelBuilder.AddPanel(
                    id: "Panel_unstratified",
                    title: "Unstratified",
                    hoverText: "Unstratified",
                    content: ChartHelpers.Chart(
                        name: "BoxPlotChart",
                        section: Model,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        viewBag: ViewBag,
                        saveChartFile: true,
                        caption: chartCreator.Title,
                        chartData: percentileDataSection
                ));
            }
            {
                if (Model.StratifiedExposureBoxPlotRecords?.Count > 0) {
                    var chartCreator = new BoxPlotChartCreator(
                        Model,
                        Model.StratifiedExposureBoxPlotRecords,
                        Model.TargetUnit,
                        Model.ShowOutliers,
                        true
                    );

                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        name: $"StratifiedBoxPlotData",
                        section: Model,
                        items: Model.StratifiedExposureBoxPlotRecords,
                        viewBag: ViewBag
                    );
                    panelBuilder.AddPanel(
                        id: "Panel_stratified",
                        title: "Stratified",
                        hoverText: "Stratified",
                        content: ChartHelpers.Chart(
                            name: "StratifiedBoxPlotChart",
                            section: Model,
                            chartCreator: chartCreator,
                            fileType: ChartFileType.Svg,
                            viewBag: ViewBag,
                            caption: chartCreator.Title,
                            saveChartFile: true,
                            chartData: percentileDataSection
                    ));
                }
            }
            panelBuilder.RenderPanel(sb, collapseSingleTab: true);

            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Stratification))) {
                hiddenProperties.Add(nameof(ExposureDistributionRecord.Stratification));
            }
            sb.AppendTable(
                Model,
                Model.Records,
                "ExposureDistributionTable",
                ViewBag,
                caption: "Exposure statistics (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
