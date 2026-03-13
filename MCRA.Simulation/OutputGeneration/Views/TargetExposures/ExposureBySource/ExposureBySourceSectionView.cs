using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureBySourceSectionView : SectionView<ExposureBySourceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var panelBuilder = new HtmlTabPanelBuilder();
            {
                var chartCreator = new BoxPlotBySourceChartCreator(
                    Model,
                    Model.BoxPlotRecords,
                    Model.TargetUnit,
                    Model.ShowOutliers,
                    false
                );

                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    name: $"BoxPlotBySourceData",
                    section: Model,
                    items: Model.BoxPlotRecords,
                    viewBag: ViewBag
                );
                panelBuilder.AddPanel(
                    id: "Panel_unstratified",
                    title: "Unstratified",
                    hoverText: "Unstratified",
                    content: ChartHelpers.Chart(
                        name: "BoxPlotBySourceChart",
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
                    var chartCreator = new BoxPlotBySourceChartCreator(
                        Model,
                        Model.StratifiedExposureBoxPlotRecords,
                        Model.TargetUnit,
                        Model.ShowOutliers,
                        true
                    );

                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        name: $"StratifiedBoxPlotBySourceData",
                        section: Model,
                        items: Model.StratifiedExposureBoxPlotRecords,
                        viewBag: ViewBag
                    );
                    panelBuilder.AddPanel(
                        id: "Panel_stratified",
                        title: "Stratified",
                        hoverText: "Stratified",
                        content: ChartHelpers.Chart(
                            name: "StratifiedBoxPlotBySourceChart",
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
                hiddenProperties.Add(nameof(ExposureBySourceRecord.Stratification));
            }
            sb.AppendTable(
                Model,
                Model.Records,
                "ExposureBySourceTable",
                ViewBag,
                caption: "Exposure statistics by source (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
