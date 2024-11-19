using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticConversionFactorModelsSummarySectionView : SectionView<KineticConversionFactorModelsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRouteFrom))) {
                hiddenProperties.Add("ExposureRouteFrom");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrixFrom))) {
                hiddenProperties.Add("BiologicalMatrixFrom");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExpressionTypeFrom))) {
                hiddenProperties.Add("ExpressionTypeFrom");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRouteTo))) {
                hiddenProperties.Add("ExposureRouteTo");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrixTo))) {
                hiddenProperties.Add("BiologicalMatrixTo");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExpressionTypeTo))) {
                hiddenProperties.Add("ExpressionTypeTo");
            }
            if (Model.Records.All(r => !r.IsAgeLower)) {
                hiddenProperties.Add("IsAgeLower");
            }
            if (Model.Records.All(r => !r.IsGender)) {
                hiddenProperties.Add("IsGender");
            }
            if (Model.Records.All(r => !r.Both)) {
                hiddenProperties.Add("Both");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.DistributionType))) {
                hiddenProperties.Add("DistributionType");
            }
            if (Model.Records.All(r => double.IsNaN(r.UncertaintyUpper))) {
                hiddenProperties.Add("UncertaintyUpper");
            }

            sb.AppendDescriptionParagraph($"Number of kinetic conversion factor models: {Model.Records.Count}");
            if (Model.Records?.Count > 0) {
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "KineticConversionFactorModelsTable",
                   ViewBag,
                   caption: "Kinetic conversion factor models.",
                   hiddenProperties: hiddenProperties,
                   saveCsv: true
                );
            }
        }
    }
}
