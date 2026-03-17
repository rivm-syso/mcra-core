using System.Text;
using DocumentFormat.OpenXml.Drawing.Charts;
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
                    var chartCreator1 = new InternalChronicDistributionTotalChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    var histogramChart = ChartHelpers.Chart(
                        name: "AggregateTotalIntakeDistributionChart",
                        section: Model,
                        chartCreator: chartCreator1,
                        fileType: ChartFileType.Svg,
                        viewBag: ViewBag,
                        saveChartFile: true,
                        caption: chartCreator1.Title
                    ).ToString();

                    var chartCreator2 = new InternalCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    var lineChart = ChartHelpers.Chart(
                        name: "AggregateTotalIntakeCumulativeDistributionChart",
                        section: Model,
                        chartCreator: chartCreator2,
                        fileType: ChartFileType.Svg,
                        viewBag: ViewBag,
                        saveChartFile: true,
                        caption: chartCreator2.Title
                    ).ToString();

                    panelBuilder.AddPanel(
                        id: "Panel_unstratified",
                        title: "Unstratified",
                        hoverText: "Unstratified",
                        content: new HtmlString(
                            "<div class=\"figure-container\">"
                            + histogramChart + lineChart
                            + "</div>"
                        )
                    );
                }
                if ((Model.StratifiedIntakeDistributionBins?.Count ?? 0) > 1) {
                    var chartCreator1 = new StratifiedStackedHistogramChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    var histogramChart = ChartHelpers.Chart(
                        name: "StratifiedStackedHistogramChart",
                        section: Model,
                        chartCreator: chartCreator1,
                        fileType: ChartFileType.Svg,
                        viewBag: ViewBag,
                        caption: chartCreator1.Title,
                        saveChartFile: true
                    ).ToString();

                    var chartCreator2 = new InternalStratifiedCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                    var lineChart = ChartHelpers.Chart(
                        name: "StratifiedCumulativeDistributionChart",
                        section: Model,
                        chartCreator: chartCreator2,
                        fileType: ChartFileType.Svg,
                        viewBag: ViewBag,
                        saveChartFile: true,
                        caption: chartCreator2.Title
                    ).ToString();

                    panelBuilder.AddPanel(
                        id: "Panel_stratified",
                        title: "Stratified",
                        hoverText: "Stratified",
                        content: new HtmlString(
                            "<div class=\"figure-container\">"
                            + histogramChart + lineChart
                            + "</div>"
                        )
                    );
                }
                panelBuilder.RenderPanel(sb, collapseSingleTab: true);

            } else {
                sb.AppendNotification("No positive exposures.");
            }

            //TODO move this to exposure by route section, discuss with Jasper
            //if ((Model.CategorizedHistogramBins?.Count ?? 0) > 1) {
            //    var chartCreator = new InternalChronicStackedHistogramChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            //    sb.AppendChart(
            //        "AcuteTotalStackedHistogramChart",
            //        chartCreator,
            //        ChartFileType.Svg,
            //        Model,
            //        ViewBag,
            //        chartCreator.Title,
            //        true
            //    );
            //}
        }
    }
}
