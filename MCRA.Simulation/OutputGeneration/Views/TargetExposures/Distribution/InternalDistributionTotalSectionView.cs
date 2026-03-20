using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class InternalDistributionTotalSectionView : SectionView<InternalDistributionTotalSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            bool showUncertainty = !Model.Percentiles.All(p => double.IsNaN(p.MedianUncertainty));
            var panelBuilder = new HtmlTabPanelBuilder();

            if (Model.PercentageZeroIntake < 100) {
                {
                    var histChartCreator = new InternalChronicDistributionTotalChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    var histogramChart = ChartHelpers
                        .Chart(
                            name: "AggregateTotalIntakeDistributionChart",
                            section: Model,
                            chartCreator: histChartCreator,
                            fileType: ChartFileType.Svg,
                            viewBag: ViewBag,
                            saveChartFile: true,
                            caption: histChartCreator.Title
                        ).ToString();

                    var boxplotChartCreator = new InternalExposureDistributionBoxPlotChartCreator(
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

                    var boxplotChart = ChartHelpers
                        .Chart(
                            name: "BoxPlotChart",
                            section: Model,
                            chartCreator: boxplotChartCreator,
                            fileType: ChartFileType.Svg,
                            viewBag: ViewBag,
                            saveChartFile: true,
                            caption: boxplotChartCreator.Title,
                            chartData: percentileDataSection
                        ).ToString();

                    var lineChartCreator = new InternalCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    var lineChart = ChartHelpers
                        .Chart(
                            name: "AggregateTotalIntakeCumulativeDistributionChart",
                            section: Model,
                            chartCreator: lineChartCreator,
                            fileType: ChartFileType.Svg,
                            viewBag: ViewBag,
                            saveChartFile: true,
                            caption: lineChartCreator.Title
                        ).ToString();

                    panelBuilder.AddPanel(
                        id: "Panel_unstratified",
                        title: "Unstratified",
                        hoverText: "Unstratified",
                        content: new HtmlString(
                            "<div class=\"figure-container\">"
                            + histogramChart + lineChart
                            + "</div>"
                            + boxplotChart
                        )
                    );
                }

                if ((Model.StratifiedIntakeDistributionBins?.Count ?? 0) > 1) {
                    var histChartCreator = new StratifiedStackedHistogramChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    var histogramChart = ChartHelpers
                        .Chart(
                            name: "StratifiedStackedHistogramChart",
                            section: Model,
                            chartCreator: histChartCreator,
                            fileType: ChartFileType.Svg,
                            viewBag: ViewBag,
                            caption: histChartCreator.Title,
                            saveChartFile: true
                        ).ToString();

                    var boxplotChartCreator = new InternalExposureDistributionBoxPlotChartCreator(
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

                    var boxplotChart = ChartHelpers
                        .Chart(
                            name: "StratifiedBoxPlotChart",
                            section: Model,
                            chartCreator: boxplotChartCreator,
                            fileType: ChartFileType.Svg,
                            viewBag: ViewBag,
                            caption: boxplotChartCreator.Title,
                            saveChartFile: true,
                            chartData: percentileDataSection
                        ).ToString();

                    var lineChartCreator = new InternalStratifiedCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    var lineChart = ChartHelpers
                        .Chart(
                            name: "StratifiedCumulativeDistributionChart",
                            section: Model,
                            chartCreator: lineChartCreator,
                            fileType: ChartFileType.Svg,
                            viewBag: ViewBag,
                            saveChartFile: true,
                            caption: lineChartCreator.Title
                        ).ToString();

                    panelBuilder.AddPanel(
                        id: "Panel_stratified",
                        title: "Stratified",
                        hoverText: "Stratified",
                        content: new HtmlString(
                            "<div class=\"figure-container\">"
                            + histogramChart + lineChart
                            + "</div>"
                            + boxplotChart
                        )
                    );
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
            } else {
                sb.AppendNotification("No positive exposures.");
            }
        }
    }
}
