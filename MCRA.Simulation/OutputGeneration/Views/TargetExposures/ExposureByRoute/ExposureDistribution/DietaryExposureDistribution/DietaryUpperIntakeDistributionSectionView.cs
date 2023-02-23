using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryUpperIntakeDistributionSectionView : SectionView<DietaryUpperIntakeDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            bool showUncertainty = !Model.Percentiles.All(p => double.IsNaN(p.MedianUncertainty));

            //Render HTML
            if (Model is DietaryUpperIntakeCoExposureDistributionSection) {
                renderSectionView(sb, "DietaryUpperIntakeCoExposureDistributionSection", Model);
            } else {
                sb.AppendDescriptionParagraph($"Upper percentage {Model.UpperPercentage:F2} % ({Model.NRecords} records), " +
                    $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                    $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");

                var chartCreator = new DietaryUpperIntakeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "DietaryUpperIntakeDistributionChartCreatorChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
            }
        }
    }
}
