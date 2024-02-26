using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmCumulativeIndividualDistributionsSectionView : SectionView<HbmCumulativeIndividualDistributionsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Records.Any()) {
                var percentileDataSection = DataSectionHelper
                    .CreateCsvDataSection("CumulativeConcentrationsPercentiles", Model, Model.HbmBoxPlotRecords, ViewBag);
                var chartCreator = new HbmCumulativeIndividualDistributionsBoxPlotChartCreator(Model);

                sb.AppendChart(
                    "CumulativeConcentrationsBoxPlotChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    caption: chartCreator.Title,
                    saveChartFile: true,
                    chartData: percentileDataSection
                );

                var hiddenProperties = new List<string>();
                hiddenProperties.Add("SubstanceCode");
                if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                    hiddenProperties.Add("BiologicalMatrix");
                }
                if (Model.Records.All(r => !r.MedianAllUncertaintyValues?.Any() ?? true)) {
                    hiddenProperties.Add("MedianAllMedianPercentile");
                    hiddenProperties.Add("MedianAllLowerBoundPercentile");
                    hiddenProperties.Add("MedianAllUpperBoundPercentile");
                }

                sb.AppendTable(
                    Model,
                    Model.Records,
                    "CumulativeConcentrationsTable",
                    ViewBag,
                    caption: "Human monitoring individual measurement distribution endpoint cumulative substance.",
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
