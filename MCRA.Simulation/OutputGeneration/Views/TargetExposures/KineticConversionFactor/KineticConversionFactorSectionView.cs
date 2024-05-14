using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticConversionFactorSectionView : SectionView<KineticConversionFactorSection> {
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
            //Render HTML

            sb.AppendTable(
                Model, 
                Model.Records, 
                "KineticConversionFactorTable", 
                ViewBag, 
                caption: "Kinetic conversion factors by substance and route.",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
