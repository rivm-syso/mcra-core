using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmCumulativeIndividualDayDistributionsSectionView : SectionView<HbmCumulativeIndividualDayDistributionsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.Any()) {
                hiddenProperties.Add("SubstanceCode");
                if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                    hiddenProperties.Add("BiologicalMatrix");
                }

                var percentileDataSection = DataSectionHelper
                    .CreateCsvDataSection("CumulativeDayConcentrationsPercentiles", Model, Model.HbmBoxPlotRecords, ViewBag);
                var chartCreator = new HbmCumulativeIndividualDayDistributionsBoxPlotChartCreator(Model);

                sb.AppendChart(
                    "CumulativeDayConcentrationsBoxPlotChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    caption: chartCreator.Title,
                    saveChartFile: true,
                    chartData: percentileDataSection
                );

                sb.AppendTable(
                    Model,
                    Model.Records,
                    "CumulativeDayConcentrationsTable",
                    ViewBag,
                    caption: "Human monitoring individual day measurement distribution endpoint cumulative substance.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph($"After removing individual days with missing values, no data are left.");
            }
        }
    }
}
