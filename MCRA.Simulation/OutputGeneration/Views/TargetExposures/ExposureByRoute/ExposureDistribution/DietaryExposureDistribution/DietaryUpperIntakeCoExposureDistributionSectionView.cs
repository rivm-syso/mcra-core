using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryUpperIntakeCoExposureDistributionSectionView : SectionView<DietaryUpperIntakeCoExposureDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            sb.AppendDescriptionParagraph($@"Upper percentage {Model.UpperPercentage:F2} % ({Model.NRecords} records),
                         minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")},
                         maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");
            var chartCreator = new DietaryUpperIntakeCoExposureDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                    "DietaryUpperIntakeCoExposureDistributionChart",
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
