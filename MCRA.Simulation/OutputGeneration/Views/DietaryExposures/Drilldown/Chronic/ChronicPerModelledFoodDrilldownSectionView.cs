using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ChronicPerModelledFoodDrilldownSectionView : SectionView<ChronicPerModelledFoodDrilldownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var equivalents = Model.IsCumulative ? " equivalents" : "";
            var descriptionModelledFoods = $"Exposure per day = consumption modelled food * {Model.ReferenceCompoundName}{equivalents} / {Model.BodyWeight} (= body weight).";
            var uniqueModelledFoodNameCount = Model.IndividualModelledFoodDrillDownRecords.Select(dd => dd.FoodName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            if (uniqueModelledFoodNameCount > 1) {
                var chartCreator = new DietaryChronicModelledFoodPieChartCreator(Model.IndividualModelledFoodDrillDownRecords, Model.DrilldownIndex);
                sb.AppendChart(
                    $"DietaryChronicModelledFoodPieChart-{Model.DrilldownIndex}",
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
                Model.IndividualModelledFoodDrillDownRecords,
                $"DietaryChronicModelledFoodTable-{Model.DrilldownIndex}",
                ViewBag,
                caption: descriptionModelledFoods,
                saveCsv: true,
                header: true,
                displayLimit: 20,
                hiddenProperties: null
            );
        }
    }
}
