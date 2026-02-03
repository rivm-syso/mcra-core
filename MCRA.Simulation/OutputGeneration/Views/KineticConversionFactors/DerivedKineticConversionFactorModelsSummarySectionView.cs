using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DerivedKineticConversionFactorModelsSummarySectionView : SectionView<DerivedKineticConversionFactorModelsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var targetPanelBuilder = new HtmlTabPanelBuilder();
            {
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

                var tabSb = new StringBuilder();
                tabSb.AppendDescriptionParagraph($"Number of kinetic conversion factor records: {Model.Records.Count}");
                if (Model.Records?.Count > 0) {
                    tabSb.AppendTable(
                       Model,
                       Model.Records,
                       "KineticConversionFactorsTable",
                       ViewBag,
                       caption: "Kinetic conversion factors derived from PBK model simulations.",
                       hiddenProperties: hiddenProperties,
                       saveCsv: true
                    );
                }
                targetPanelBuilder.AddPanel(
                    id: "KineticConversionFactorsTableTab",
                    title: "Table",
                    hoverText: "Kinetic conversion factor models summary table",
                    content: new HtmlString(tabSb.ToString())
                );
            }
            {
                int ncols = 3;
                int nrows = (int)Math.Ceiling(1.0 * Model.Records.Count / ncols);
                var ix = 0;

                var tabSb = new StringBuilder();
                tabSb.Append("<table><tbody>");
                for (int i = 0; i < nrows; i++) {
                    tabSb.Append("<tr>");
                    for (int j = 0; j < ncols; j++) {
                        tabSb.Append("<td>");
                        if (ix < Model.Records.Count) {
                            var record = Model.Records[ix++];
                            var chartCreator = new DerivedKineticConversionFactorModelScatterChartCreator(Model, record);
                            tabSb.AppendChart(
                                $"SubstanceTargetExposureCorrelationScatterChart_{record.GetKey()}",
                                chartCreator: chartCreator,
                                fileType: ChartFileType.Png,
                                section: Model,
                                viewBag: ViewBag,
                                saveChartFile: false
                            );
                        }
                        tabSb.Append("</td>");
                    }
                    tabSb.Append("</tr>");
                }
                tabSb.Append("</tbody></table>");
                targetPanelBuilder.AddPanel(
                    id: "KineticConversionFactorsChartsTab",
                    title: "Charts",
                    hoverText: "Kinetic conversion factor models charts",
                    content: new HtmlString(tabSb.ToString())
                );
            }
            targetPanelBuilder.RenderPanel(sb);
        }
    }
}
