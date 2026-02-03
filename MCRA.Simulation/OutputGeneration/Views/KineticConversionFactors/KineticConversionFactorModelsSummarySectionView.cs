using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticConversionFactorModelsSummarySectionView : SectionView<KineticConversionFactorModelsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRouteFrom))) {
                hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.ExposureRouteFrom));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrixFrom))) {
                hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.BiologicalMatrixFrom));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExpressionTypeFrom))) {
                hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.ExpressionTypeFrom));
            }
            if (Model.Records.All(r => r.SubstanceCodeTo == r.SubstanceCodeFrom)) {
                hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.SubstanceCodeTo));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRouteTo))) {
                hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.ExposureRouteTo));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrixTo))) {
                hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.BiologicalMatrixTo));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExpressionTypeTo))) {
                hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.ExpressionTypeTo));
            }
            if (Model.Records.All(r => !r.HasCovariateAge)) {
                hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.HasCovariateAge));
            }
            if (Model.Records.All(r => !r.HasCovariateSex)) {
                hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.HasCovariateSex));
            }
            if (Model.Records.All(r => r.UncertaintyValues == null || r.UncertaintyValues.Distinct().Count() <= 1)) {
                hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.UncertaintyMedian));
                hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.UncertaintyLowerBoundPercentile));
                hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.UncertaintyUpperBoundPercentile));
            }

            sb.AppendDescriptionParagraph($"Number of kinetic conversion factor records: {Model.Records.Count}");
            if (Model.Records?.Count > 0) {
                sb.AppendTable(
                   Model,
                   Model.Records,
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
