using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticConversionSummarySectionView : SectionView<KineticConversionSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Records.First().KineticConversionFactors.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerKineticConversionFactor");
                hiddenProperties.Add("UpperKineticConversionFactor");
                hiddenProperties.Add("MeanKineticConversionFactor");
            } else {
                hiddenProperties.Add("KineticConversionFactor");
            }

            sb.AppendTable(
                Model,
                Model.Records,
                "KineticConversionSummaryTable",
                ViewBag,
                caption: "Kinetic conversion models by substance and route.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
