using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductExposureSetSummarySectionView : SectionView<ConsumerProductExposureSetSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Minimum))) {
                hiddenProperties.Add(nameof(ConsumerProductExposureSetRecord.Minimum));
                hiddenProperties.Add(nameof(ConsumerProductExposureSetRecord.Maximum));
            }
            var panelBuilder = new HtmlTabPanelBuilder();
            foreach (var record in Model.PercentileRecords) {
                var panelSb = new StringBuilder();
                var chartCreator = new ConsumerProductPercentilesDataChartCreator(Model.SectionId, record, record.Unit);
                panelSb.AppendChart(
                     $"{record.ExposureSetCode}DistributionChart",
                     chartCreator,
                     ChartFileType.Svg,
                     Model,
                     ViewBag,
                     $"{record.ExposureSetCode}: {record.SubstanceName}, {record.Route}",
                     true
                 );

                panelBuilder.AddPanel(
                     id: $"Panel_{record.ExposureSetCode}",
                     title: record.ExposureSetCode,
                     hoverText: record.ExposureSetCode,
                     content: new HtmlString(panelSb.ToString())
                 );
            }
            panelBuilder.RenderPanel(sb, collapseSingleTab: true);
        }
    }
}
