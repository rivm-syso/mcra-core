using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DoseResponseExperimentSectionView : SectionView<DoseResponseExperimentSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            sb.AppendDescriptionTable(
                Model.SectionId + "Summary",
                Model.SectionId,
                Model,
                ViewBag,
                header: false,
                hiddenProperties: null,
                attributes: null);

            var width = Model.DoseResponseSets.Count > 1 ? 600 : 500;
            var chartCreator = new DoseResponseDataChartCreator(Model, width, 350);
            sb.AppendChart(
                $"DoseResponseData-{Model.ExperimentCode}-{Model.ResponseCode}",
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
