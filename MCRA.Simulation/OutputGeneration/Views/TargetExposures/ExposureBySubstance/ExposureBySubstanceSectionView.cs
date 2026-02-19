using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureBySubstanceSectionView : SectionView<ExposureBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var panelBuilder = new HtmlTabPanelBuilder();
            {
                var chartCreator = new ExposureBySubstanceBoxPlotChartCreator(
                    Model,
                    Model.ExposureBoxPlotRecords,
                    Model.TargetUnit,
                    Model.ShowOutliers,
                    false
                );

                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    name: $"BoxPlotBySubstanceData",
                    section: Model,
                    items: Model.ExposureBoxPlotRecords,
                    viewBag: ViewBag
                );
                panelBuilder.AddPanel(
                    id: "Panel_unstratified",
                    title: "Unstratified",
                    hoverText: "Unstratified",
                    content: ChartHelpers.Chart(
                        name: "BoxPlotBySubstanceChart",
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
                    var chartCreator = new ExposureBySubstanceBoxPlotChartCreator(
                        Model,
                        Model.StratifiedExposureBoxPlotRecords,
                        Model.TargetUnit,
                        Model.ShowOutliers,
                        true
                    );
                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        name: $"StratifiedBoxPlotBySubstanceData",
                        section: Model,
                        items: Model.StratifiedExposureBoxPlotRecords,
                        viewBag: ViewBag
                    );
                    panelBuilder.AddPanel(
                        id: "Panel_stratified",
                        title: "Stratified",
                        hoverText: "Stratified",
                        content: ChartHelpers.Chart(
                            name: "StratifiedBoxPlotBySubstanceChart",
                            section: Model,
                            chartCreator: chartCreator,
                            fileType: ChartFileType.Svg,
                            viewBag: ViewBag,
                            saveChartFile: true,
                            caption: chartCreator.Title,
                            chartData: percentileDataSection
                    ));
                }
            }
            panelBuilder.RenderPanel(sb, collapseSingleTab: true);

            var hiddenProperties = new List<string>();
            if (Model.ExposureRecords.All(c => string.IsNullOrEmpty(c.Stratification))) {
                hiddenProperties.Add(nameof(ExposureBySubstanceRecord.Stratification));
            }
            sb.AppendTable(
                Model,
                Model.ExposureRecords,
                "ExposureBySubstanceTable",
                ViewBag,
                caption: "Exposure statistics by substance (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
