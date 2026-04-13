using System.Text;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {

    public class ExposuresSectionView<S, T1, T2> : SectionView<InternalExposureDistributionSectionBase<S, T1, T2>>
        where S : IExposureContributorKey, new()
        where T1 : InternalExposureDistributionRecordBase<S>, new()
        where T2 : InternalExposureBoxPlotRecordBase<S>, new() {
        public override void RenderSectionHtml(StringBuilder sb) {
            var panelBuilder = new HtmlTabPanelBuilder();
            {
                var chartCreator = new InternalExposureBoxPlotChartCreator<S, T2>(
                    Model.DescriptorName,
                    Model.BoxPlotRecords,
                    Model.TargetUnit,
                    Model.ShowOutliers);

                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    name: $"BoxPlotBy{Model.DescriptorKey}Data",
                    section: Model,
                    items: Model.BoxPlotRecords,
                    viewBag: ViewBag
                );
                panelBuilder.AddPanel(
                    id: "Panel_unstratified",
                    title: "Unstratified",
                    hoverText: "Unstratified",
                    content: ChartHelpers.Chart(
                        name: $"BoxPlotBy{Model.DescriptorKey}Chart",
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
                if (Model.StratifiedBoxPlotRecords?.Count > 0) {
                    var chartCreator = new InternalExposureStratifiedBoxPlotChartCreator<S, T2>(
                        Model.DescriptorName,
                        Model.StratifiedBoxPlotRecords,
                        Model.TargetUnit,
                        Model.ShowOutliers,
                        true
                    );

                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        name: $"StratifiedBoxPlotBy{Model.DescriptorKey}Data",
                        section: Model,
                        items: Model.StratifiedBoxPlotRecords,
                        viewBag: ViewBag
                    );
                    panelBuilder.AddPanel(
                        id: "Panel_stratified",
                        title: "Stratified",
                        hoverText: "Stratified",
                        content: ChartHelpers.Chart(
                            name: $"StratifiedBoxPlotBy{Model.DescriptorKey}Chart",
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
                hiddenProperties.Add("Stratification");
            }
            sb.AppendTable(
                Model,
                Model.Records,
                $"ExposureBy{Model.DescriptorKey}Table",
                ViewBag,
                caption: $"Exposure statistics by {Model.DescriptorName} (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}