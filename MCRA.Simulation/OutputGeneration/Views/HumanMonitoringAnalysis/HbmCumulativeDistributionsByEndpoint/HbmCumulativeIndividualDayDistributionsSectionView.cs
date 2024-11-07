using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmCumulativeIndividualDayDistributionsSectionView : SectionView<HbmCumulativeIndividualDayDistributionsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.RestrictedUpperPercentile.HasValue) {
                var upper = Model.RestrictedUpperPercentile.Value;
                sb.AppendWarning("This section cannot be rendered because the sample size is insufficient for reporting the selected percentiles in accordance with the privacy guidelines." +
                    $" For the given sample size, only percentile values below p{upper:#0.##} can be reported.");
            } else {
                if (Model.Records.Any()) {
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
                        "CumulativeDayConcentrationsTable",
                        ViewBag,
                        caption: "Human biomonitoring individual day measurement distribution endpoint cumulative substance.",
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
}
