using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ImputedHazardCharacterisationsSummarySectionView : SectionView<ImputedHazardCharacterisationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ModelCode))) {
                hiddenProperties.Add(nameof(ImputedHazardCharacterisationSummaryRecord.ModelCode));
            }
            if (Model.Records.All(r => double.IsNaN(r.TargetDoseLowerBoundPercentile))) {
                hiddenProperties.Add(nameof(ImputedHazardCharacterisationSummaryRecord.TargetDoseLowerBoundPercentile));
                hiddenProperties.Add(nameof(ImputedHazardCharacterisationSummaryRecord.TargetDoseUpperBoundPercentile));
                hiddenProperties.Add(nameof(ImputedHazardCharacterisationSummaryRecord.TargetDoseLowerBoundPercentileUnc));
                hiddenProperties.Add(nameof(ImputedHazardCharacterisationSummaryRecord.TargetDoseUpperBoundPercentileUnc));
            }
            if (Model.Records.All(r => double.IsNaN(r.GeometricStandardDeviation))) {
                hiddenProperties.Add(nameof(ImputedHazardCharacterisationSummaryRecord.GeometricStandardDeviation));
            }
            if (Model.Records.All(r => double.IsNaN(r.NominalIntraSpeciesConversionFactor) || r.NominalIntraSpeciesConversionFactor == 1D)) {
                hiddenProperties.Add(nameof(ImputedHazardCharacterisationSummaryRecord.NominalIntraSpeciesConversionFactor));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.PotencyOrigin))) {
                hiddenProperties.Add(nameof(ImputedHazardCharacterisationSummaryRecord.PotencyOrigin));
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
