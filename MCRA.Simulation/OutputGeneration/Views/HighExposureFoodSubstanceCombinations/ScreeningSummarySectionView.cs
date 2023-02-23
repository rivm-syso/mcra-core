using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ScreeningSummarySectionView : SectionView<ScreeningSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            hiddenProperties.Add("CompoundCode");
            hiddenProperties.Add("FoodAsEatenCode");
            hiddenProperties.Add("FoodAsMeasuredCode");
            var countRiskDrivers = Model.GroupedScreeningSummaryRecords.Count - 1;
            var countComponents = Model.GroupedScreeningSummaryRecords.Take(countRiskDrivers).Sum(c => c.NumberOfFoods);
            var count = Model.ScreeningSummaryRecords.Count - 1;
            var intakeUnit = ViewBag.GetUnit("IntakeUnit").ToHtml();

            //Render HTML
            sb.Append($@"<p>Risk driver component with the highest exposure percentile:
            <ul>
                <li>Compound: {Model.RiskDriver.CompoundName}</li>
                <li>Food-as-measured: {Model.RiskDriver.FoodAsMeasuredName}</li>
                <li>Food-as-eaten: {Model.RiskDriver.FoodAsEatenName}</li>
            </ul>
            p{Model.CriticalExposurePercentage} of exposure: {Model.RiskDriver.Exposure:G4} {intakeUnit} (limit)</p>
            <p>Cumulative importance selection percentage = {Model.CumulativeSelectionPercentage} %</p>
            <p>Fraction of LOR used for censored value imputation = {Model.ImportanceFactorLor:F2}</p>");

            if (Model.GroupedScreeningSummaryRecords.Any()) {
                sb.Append("<div>");
                sb.Append($"<p><b>Selected risk drivers (n = {countRiskDrivers}, containing {countComponents} " +
                    $"out of {Model.GroupedScreeningSummaryRecords.Sum(c => c.NumberOfFoods)} components)</b></p>");

                var chartCreator = new GroupedScreeningPieChartCreator(Model);
                sb.AppendChart(
                    "GroupedScreeningPieChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );

                sb.AppendTable(
                   Model,
                   Model.GroupedScreeningSummaryRecords,
                   "ExpandedRestrictedDataTable",
                   ViewBag,
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                ); 
                sb.Append("</div>");
            } else {
                sb.AppendParagraph("No info for expanded restricted set available");
            }

            if (Model.GroupedScreeningSummaryRecords.Any()) {
                sb.Append("<div>");
                sb.Append($"<p><b>Relevant components of selected risk drivers (n = {count} ) </b></p>");

                var chartCreator = new ScreeningPieChartCreator(Model);
                sb.AppendChart(
                    "ScreeningPieChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );

                if (Model.ScreeningSummaryRecords.All(c => double.IsNaN(c.WeightDetect))) {
                    hiddenProperties.Add("WeightDetect");
                }
                if (Model.ScreeningSummaryRecords.All(c => double.IsNaN(c.WeightCensoredValues))) {
                    hiddenProperties.Add("WeightCensoredValues");
                }
                if (Model.ScreeningSummaryRecords.All(c => double.IsNaN(c.MuCensoredValues))) {
                    hiddenProperties.Add("MuCensoredValues");
                }
                if (Model.ScreeningSummaryRecords.All(c => double.IsNaN(c.SigmaCensoredValues))) {
                    hiddenProperties.Add("SigmaCensoredValues");
                }
                if (Model.ScreeningSummaryRecords.All(c => c.FractionCensoredValues == 0)) {
                    hiddenProperties.Add("FractionCensoredValues");
                }

                sb.AppendTable(
                   Model,
                   Model.ScreeningSummaryRecords,
                   "TopScreeningDataTable",
                   ViewBag,
                   caption: "Screening.",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                ); 
                sb.Append("</div>");
            } else {
                sb.AppendParagraph("No info for screening available");
            }
        }
    }
}
