using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class FrequencyAmountSummarySectionView : SectionView<FrequencyAmountSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var description = "The number of survey days versus amounts relation. The hinges indicate the lower and upper quartile " + 
                "(25 and 75%), the notches rougly indicate the 95% interval, the median is indicated by the black line.";
            sb.AppendDescriptionParagraph(description);

            var chartCreator = new FrequencyAmountChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "FrequenciesAmountsChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator.Title,
                true
            );
            sb.AppendTable(
                Model,
                Model.DayFrequencyRecords,
                "DayFrequencyTable",
                ViewBag,
                caption: "Number and percentage of days with positive (daily) exposures.",
                saveCsv: true
            );
            sb.AppendTable(
                Model,
                Model.ExposureFrequencyRecords,
                "ExposureFrequencyTable",
                ViewBag,
                caption: "Number and percentage of population with positive (daily) exposures.",
                saveCsv: true
            );
            sb.AppendTable(
                Model,
                Model.ExposureSummaryRecords,
                "ExposureStatisticsTable",
                ViewBag,
                caption: "Statistics of all exposures (including zeros) and positive exposures only.",
                saveCsv: true
            );
        }
    }
}
