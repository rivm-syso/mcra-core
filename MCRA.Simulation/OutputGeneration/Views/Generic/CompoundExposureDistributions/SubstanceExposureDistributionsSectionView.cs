using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SubstanceExposureDistributionsSectionView : SectionView<SubstanceExposureDistributionsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.SubstanceExposureDistributionRecords.Count == 0) {
                sb.AppendWarning("No substance exposures available");
                return;
            }

            sb.AppendNotification("Note: this section summarizes the substance exposure distributions of the individual days.");

            var targetPanelBuilder = new HtmlTabPanelBuilder();

            {
                // Substance distribution charts tab
                int take = 4;
                int loopCount = (int)Math.Ceiling(1.0 * Model.SubstanceExposureDistributionRecords.Count / take);
                var tableSb = new StringBuilder();
                tableSb.Append(@"<table><tbody>");
                for (int i = 0; i < loopCount; i++) {
                    tableSb.Append("<tr>");
                    foreach (var record in Model.SubstanceExposureDistributionRecords.Skip(i * take).Take(take)) {
                        tableSb.Append("<td>");
                        if (record.HistogramBins?.Count > 0) {
                            var chartCreator1 = new XYRescaledExposureDistributionPerCompoundChartCreator(record, 250, 175, Model.Upper, Model.Lower, Model.MaximumFrequency, ViewBag.GetUnit("IntakeUnit"));
                            tableSb.AppendChart(
                                "XYRescaledExposureDistributionPerCompoundChart",
                                chartCreator1,
                                ChartFileType.Svg,
                                Model,
                                ViewBag,
                                chartCreator1.Title,
                                true
                            );
                        } else {
                            if (record.IsImputed == true) {
                                tableSb.Append($"<div class='no_measurements'>Imputed exposure for {record.SubstanceName.ToHtml()}</div>");
                            } else {
                                tableSb.Append($"<div class='no_measurements'>No positive exposure for {record.SubstanceName.ToHtml()}</div>");
                            }
                        }
                        tableSb.Append("</td>");
                    }
                    tableSb.Append("</tr>");
                }
                tableSb.Append("</tbody></table>");
                targetPanelBuilder.AddPanel(
                    id: "SubstanceExposureDistributionChartsTab",
                    title: "Substance distribution charts",
                    hoverText: "Substance exposure distribution charts",
                    content: new HtmlString(tableSb.ToString())
                );
            }
            {
                // Substance distributions table tab
                var tableSb = new StringBuilder();
                tableSb.AppendTable(
                       Model,
                       Model.SubstanceExposureDistributionRecords,
                       "SubstanceExposureDistributionsTable",
                       ViewBag,
                       caption: "Fitted (log-normal) substance exposure distributions.",
                       saveCsv: true
                   );
                targetPanelBuilder.AddPanel(
                    id: "SubstanceExposureDistributionsTableTab",
                    title: "Substance distributions table",
                    hoverText: "Substance exposure distributions table",
                    content: new HtmlString(tableSb.ToString())
                );
            }

            if (Model.CombinedSubstanceExposureDistributionRecord?.HistogramBins?.Count > 0) {
                var tabSb = new StringBuilder();
                if (Model.EqualityOfMeans) {
                    tabSb.AppendNotification("Means are unequal (p < 0.05, F-test)");
                }
                if (Model.HomogeneityOfVariances) {
                    tabSb.AppendNotification("Variances differ between substances (p < 0.05, Bartlett's test)");
                }
                var chartCreator = new UnrescaledExposureDistributionPerCompoundChartCreator(Model.CombinedSubstanceExposureDistributionRecord, 350, 250, ViewBag.GetUnit("IntakeUnit"));
                tabSb.AppendChart(
                    "UnrescaledExposureDistributionPerSubstanceChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
                tabSb.AppendTable(
                    section: Model,
                    items: [Model.CombinedSubstanceExposureDistributionRecord],
                    name: "CombinedSubstanceExposureDistributionTable",
                    viewBag: ViewBag,
                    caption: "Fitted (log-normal) combined exposure distribution.",
                    hiddenProperties: [
                        nameof(SubstanceExposureDistributionRecord.SubstanceCode),
                        nameof(SubstanceExposureDistributionRecord.SubstanceName),
                        nameof(SubstanceExposureDistributionRecord.RelativePotencyFactor),
                        nameof(SubstanceExposureDistributionRecord.AssessmentGroupMembership)
                    ],
                    rotate: true
                );
                targetPanelBuilder.AddPanel(
                    id: "CombinedSubstanceExposureDistributionChartTab",
                    title: "Combined distribution",
                    hoverText: "Combined substance exposure distribution chart",
                    content: new HtmlString(tabSb.ToString())
                );
            }
            if (Model.SubstanceExposureDistributionRecords.Any(r => r.RelativePotencyFactor > 0)) {
                var chartCreator = new SubstancePotencyVersusExposureChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                targetPanelBuilder.AddPanel(
                    id: "SubstancePotencyVersusExposureChartTab",
                    title: "RPFs versus exposures",
                    hoverText: "RPFs versus exposures chart",
                    content: ChartHelpers.Chart(
                        name: "SubstancePotencyVersusExposureChart",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator,
                        ChartFileType.Svg,
                        saveChartFile: true,
                        caption: chartCreator.Title
                    )
                );
            }
            targetPanelBuilder.RenderPanel(sb);
        }
    }
}
