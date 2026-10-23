using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualKineticConversionBySubstanceSectionView : SectionView<HbmIndividualKineticConversionBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> {
                nameof(KineticConversionFactorModelSummaryRecord.BiologicalMatrixTo),
                nameof(KineticConversionFactorModelSummaryRecord.ExposureRouteFrom),
                nameof(KineticConversionFactorModelSummaryRecord.ExpressionTypeFrom),
                nameof(KineticConversionFactorModelSummaryRecord.ExpressionTypeTo),
                nameof(KineticConversionFactorModelSummaryRecord.HasCovariateAge),
                nameof(KineticConversionFactorModelSummaryRecord.HasCovariateSex),
                nameof(KineticConversionFactorModelSummaryRecord.UncertaintyMedian),
                nameof(KineticConversionFactorModelSummaryRecord.UncertaintyLowerBoundPercentile),
                nameof(KineticConversionFactorModelSummaryRecord.UncertaintyUpperBoundPercentile)
            };

            var panelBuilder = new HtmlTabPanelBuilder();

            var groups = Model.Records.GroupBy(r => r.SubstanceCodeFrom);
            foreach (var group in groups) {
                var panelSb = new StringBuilder();
                var groupModels = group.OrderBy(r => r.BiologicalMatrixFrom).ToList();
                panelSb.Append("<div class=\"figure-container\">");
                foreach (var model in groupModels) {
                    var chartCreator = new DerivedKineticConversionFactorModelScatterChartCreator(model, Model.SectionId);
                    panelSb.AppendChart(
                        $"HbmKineticConversionBySubstanceChart_{model.SubstanceCodeFrom}_{model.GetTargetNameFrom()}_{model.GetTargetNameTo()}",
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        section: Model,
                        viewBag: ViewBag,
                        saveChartFile: false
                    );
                }
                panelSb.Append("</div>");

                panelBuilder.AddPanel(
                    id: $"HbmKineticConversionFactorsChartsTab_{group.First().SubstanceCodeFrom}",
                    title: group.First().SubstanceNameFrom,
                    hoverText: $"Kinetic conversions {group.First().SubstanceNameFrom}",
                    content: new HtmlString(panelSb.ToString())
                );
            }
            panelBuilder.RenderPanel(sb, collapseSingleTab: true);

            if (Model.Records.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "HbmKineticConversionBySubstanceTable",
                    ViewBag,
                    caption: $"Converted human biomonitoring concentrations and substance.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph($"No kinetic conversion available");
            }
        }
    }
}
