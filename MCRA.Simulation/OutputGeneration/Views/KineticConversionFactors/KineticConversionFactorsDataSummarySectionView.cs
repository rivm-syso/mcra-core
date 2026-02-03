using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticConversionFactorsDataSummarySectionView : SectionView<KineticConversionFactorsDataSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRouteFrom))) {
                hiddenProperties.Add(nameof(KineticConversionFactorsDataSummaryRecord.ExposureRouteFrom));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrixFrom))) {
                hiddenProperties.Add(nameof(KineticConversionFactorsDataSummaryRecord.BiologicalMatrixFrom));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExpressionTypeFrom))) {
                hiddenProperties.Add(nameof(KineticConversionFactorsDataSummaryRecord.ExpressionTypeFrom));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRouteTo))) {
                hiddenProperties.Add(nameof(KineticConversionFactorsDataSummaryRecord.ExposureRouteTo));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrixTo))) {
                hiddenProperties.Add(nameof(KineticConversionFactorsDataSummaryRecord.BiologicalMatrixTo));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExpressionTypeTo))) {
                hiddenProperties.Add(nameof(KineticConversionFactorsDataSummaryRecord.ExpressionTypeTo));
            }
            if (Model.Records.All(r => !r.HasCovariateAge)) {
                hiddenProperties.Add(nameof(KineticConversionFactorsDataSummaryRecord.HasCovariateAge));
            }
            if (Model.Records.All(r => !r.HasCovariateSex)) {
                hiddenProperties.Add(nameof(KineticConversionFactorsDataSummaryRecord.HasCovariateSex));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.DistributionType))) {
                hiddenProperties.Add(nameof(KineticConversionFactorsDataSummaryRecord.DistributionType));
            }
            if (Model.Records.All(r => double.IsNaN(r.UncertaintyUpper))) {
                hiddenProperties.Add(nameof(KineticConversionFactorsDataSummaryRecord.UncertaintyUpper));
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
