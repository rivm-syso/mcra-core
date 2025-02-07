using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ChronicPerFoodAsEatenDrilldownSectionView : SectionView<ChronicPerFoodAsEatenDrilldownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var equivalents = Model.IsCumulative ? " equivalents" : "";
            var descriptionFoods = $"Exposure per day = consumption food as eaten * {Model.ReferenceCompoundName}{equivalents} / {Model.BodyWeight} (= body weight).";
            var uniqueFoodNameCount = Model.IndividualFoodAsEatenDrillDownRecords.Select(dd => dd.FoodName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            if (uniqueFoodNameCount > 1) {
                var chartCreator = new DietaryChronicFoodAsEatenPieChartCreator(Model.IndividualFoodAsEatenDrillDownRecords, Model.DrilldownIndex);
                sb.AppendChart(
                    $"DietaryChronicFoodAsEatenPieChart-{Model.DrilldownIndex}",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            }
            sb.AppendTable(
                Model,
                Model.IndividualFoodAsEatenDrillDownRecords,
                $"DietaryChronicFoodAsEatenTable-{Model.DrilldownIndex}",
                ViewBag,
                caption: descriptionFoods,
                saveCsv: true,
                header: true,
                displayLimit: 20,
                hiddenProperties: null
            );
        }
    }
}
