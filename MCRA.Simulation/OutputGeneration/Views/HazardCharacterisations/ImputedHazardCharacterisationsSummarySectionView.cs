using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ImputedHazardCharacterisationsSummarySectionView : SectionView<ImputedHazardCharacterisationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelCode))) {
                hiddenProperties.Add("ModelCode");
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBoundPercentile))) {
                hiddenProperties.Add("TargetDoseLowerBoundPercentile");
                hiddenProperties.Add("TargetDoseUpperBoundPercentile");
            }
            if (Model.Records.All(r => double.IsNaN(r.GeometricStandardDeviation))) {
                hiddenProperties.Add("GeometricStandardDeviation");
            }
            if (Model.Records.All(r => double.IsNaN(r.NominalIntraSpeciesConversionFactor) || r.NominalIntraSpeciesConversionFactor == 1D)) {
                hiddenProperties.Add("NominalIntraSpeciesConversionFactor");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.PotencyOrigin))) {
                hiddenProperties.Add("PotencyOrigin");
            }

            //Render HTML
            if (Model.Records.Any()) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ImputedTargetDosesTable",
                    ViewBag,
                    caption: "Imputed hazard characterisations.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
