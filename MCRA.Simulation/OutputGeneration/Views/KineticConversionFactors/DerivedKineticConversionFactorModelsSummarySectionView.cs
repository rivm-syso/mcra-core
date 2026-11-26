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
                    hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.SubstanceNameTo));
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
                if (Model.Records.All(r => string.IsNullOrEmpty(r.KineticModelType))) {
                    hiddenProperties.Add(nameof(KineticConversionFactorModelSummaryRecord.KineticModelType));
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
                var tabSb = new StringBuilder();
                tabSb.Append("<div class=\"figure-container\">");
                foreach (var record in Model.Records) {
                    var chartCreator = new DerivedKineticConversionFactorModelScatterChartCreator(record, Model.SectionId);
                    tabSb.AppendChart(
                        $"SubstanceTargetExposureCorrelationScatterChart_{record.GetKey()}",
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        section: Model,
                        viewBag: ViewBag,
                        saveChartFile: false
                    );
                }
                tabSb.Append("</div>");
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
