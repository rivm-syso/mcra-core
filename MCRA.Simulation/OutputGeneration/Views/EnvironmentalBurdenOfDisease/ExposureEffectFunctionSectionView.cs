using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureEffectFunctionSummarySectionView : SectionView<ExposureEffectFunctionSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (string.IsNullOrEmpty(Model.EefRecord.ExposureRoute)) {
                hiddenProperties.Add("ExposureRoute");
            }
            if (string.IsNullOrEmpty(Model.EefRecord.BiologicalMatrix)) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            if (string.IsNullOrEmpty(Model.EefRecord.ExpressionType)) {
                hiddenProperties.Add("ExpressionType");
            }

            sb.AppendTable(
                Model,
                [Model.EefRecord],
                "ExposureEffectFunctionTable",
                ViewBag,
                caption: "Exposure effect function summary table.",
                saveCsv: true,
                sortable: false,
                rotate: true,
                hiddenProperties: hiddenProperties
            );

            var chartCreator = new ExposureEffectFunctionChartCreator(Model);
            sb.AppendChart(
                "ExposureEffectFunctionChart",
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
