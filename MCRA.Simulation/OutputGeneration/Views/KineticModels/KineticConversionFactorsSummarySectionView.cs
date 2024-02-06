using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticConversionFactorsSummarySectionView : SectionView<KineticConversionFactorsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.KineticConversionFactorRecords.All(r => string.IsNullOrEmpty(r.ExposureRouteFrom))) {
                hiddenProperties.Add("ExposureRouteFrom");
            }
            if (Model.KineticConversionFactorRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrixFrom))) {
                hiddenProperties.Add("BiologicalMatrixFrom");
            }
            if (Model.KineticConversionFactorRecords.All(r => string.IsNullOrEmpty(r.ExpressionTypeFrom))) {
                hiddenProperties.Add("ExpressionTypeFrom");
            }
            if (Model.KineticConversionFactorRecords.All(r => string.IsNullOrEmpty(r.ExposureRouteTo))) {
                hiddenProperties.Add("ExposureRouteTo");
            }
            if (Model.KineticConversionFactorRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrixTo))) {
                hiddenProperties.Add("BiologicalMatrixTo");
            }
            if (Model.KineticConversionFactorRecords.All(r => string.IsNullOrEmpty(r.ExpressionTypeTo))) {
                hiddenProperties.Add("ExpressionTypeTo");
            }
            if (Model.KineticConversionFactorRecords.All(r => !r.IsAgeLower)) {
                hiddenProperties.Add("IsAgeLower");
            }
            if (Model.KineticConversionFactorRecords.All(r => !r.IsGender)) {
                hiddenProperties.Add("IsGender");
            }
            if (Model.KineticConversionFactorRecords.All(r => !r.Both)) {
                hiddenProperties.Add("Both");
            }
            sb.AppendDescriptionParagraph($"Number of kinetic conversion records: {Model.KineticConversionFactorRecords.Count}");
            if (Model.KineticConversionFactorRecords?.Any() ?? false) {
                sb.AppendTable(
                   Model,
                   Model.KineticConversionFactorRecords,
                   "KineticConversionFactorsTable",
                   ViewBag,
                   caption: "Kinetic conversion factors.",
                   hiddenProperties: hiddenProperties,
                   saveCsv: true
                );
            }
        }
    }
}
