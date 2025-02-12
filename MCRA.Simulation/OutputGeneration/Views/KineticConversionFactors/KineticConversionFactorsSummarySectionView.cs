﻿using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticConversionFactorsSummarySectionView : SectionView<KineticConversionFactorsSummarySection> {
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
            if (Model.Records.All(r => !r.AgeLower.HasValue || !double.IsNaN(r.AgeLower.Value))) {
                hiddenProperties.Add("AgeLower");
            }
            if (Model.Records.All(r => !string.IsNullOrEmpty(r.Sex))) {
                hiddenProperties.Add("Sex");
            }
            if (Model.Records.All(r => r.UncertaintyValues == null || r.UncertaintyValues.Distinct().Count() <= 1)) {
                hiddenProperties.Add("UncertaintyMedian");
                hiddenProperties.Add("UncertaintyLowerBoundPercentile");
                hiddenProperties.Add("UncertaintyUpperBoundPercentile");
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
