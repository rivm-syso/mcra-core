using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AcutePerFoodDrilldownSectionView : SectionView<AcutePerFoodDrilldownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.DrilldownRecord.DietaryExposure <= 0) {
                sb.AppendParagraph($"For individual: {Model.DrilldownRecord.IndividualId}, day {Model.DrilldownRecord.Day} no exposures available");
                return;
            }

            var hiddenPropertiesFoods = new List<string> { "Day" };
            var equivalents = Model.IsCumulative ? " equivalents" : "";
            var descriptionFood = $"Intake = consumption {(Model.IsModelledFood ? "modelled food" : "food as eaten")}" +
                $" * {Model.ReferenceCompoundName}{equivalents} / {Model.DrilldownRecord.BodyWeight} (= body weight)";

            if (Model.IndividualFoodDrillDownRecords != null) {
                var foodType = Model.IsModelledFood ? "ModelledFood" : "FoodAsEaten";
                if (Model.IndividualFoodDrillDownRecords.Count(c => c.Exposure > 0) > 1) {
                    var chartCreator = new DietaryAcuteFoodAsMeasuredPieChartCreator(Model.IndividualFoodDrillDownRecords, Model.DrilldownIndex);
                    sb.AppendChart(
                        $"DietaryAcute{foodType}FoodPieChart-{Model.DrilldownIndex}",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        "Total exposure per body weight/day for " + (Model.IsModelledFood ? "modelled foods" : "foods as eaten"),
                        true
                    );
                }
                sb.AppendTable(
                    Model,
                    Model.IndividualFoodDrillDownRecords,
                    $"DietaryAcute{foodType}FoodSectionTable-{Model.DrilldownIndex}",
                    ViewBag,
                    caption: descriptionFood,
                    saveCsv: true,
                    header: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenPropertiesFoods
                );
            }
        }
    }
}
