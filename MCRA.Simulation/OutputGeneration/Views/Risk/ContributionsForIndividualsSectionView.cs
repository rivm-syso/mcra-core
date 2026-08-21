using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;


namespace MCRA.Simulation.OutputGeneration.Views {
    public class ContributionsForIndividualsSectionView : SectionView<ContributionsForIndividualsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            {
                var hiddenProperties = new List<string>() {
                    nameof(RiskContributionBySubstanceRecord.PositivesCount),
                    nameof(RiskContributionBySubstanceRecord.PercentagePositives),
                    nameof(RiskContributionBySubstanceRecord.MeanAll),
                };
                var results = new List<RiskContributionBySubstanceRecord>();
                var isUncertainty = Model.IndividualContributionRecords.FirstOrDefault()?.Contributions.Any() ?? false;

                if (isUncertainty) {
                    hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.Contribution));
                    results = [.. Model.IndividualContributionRecords.Where(c => c.MeanContribution > 0)];
                } else {
                    hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.LowerContributionPercentage));
                    hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.UpperContributionPercentage));
                    hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.MeanContribution));
                    results = [.. Model.IndividualContributionRecords.Where(c => c.Contribution > 0)];
                }
                if (results.Count != 0) {

                    var panelBuilder = new HtmlTabPanelBuilder();
                    var unstratifiedRecords = Model.IndividualContributionRecords
                        .Where(r => string.IsNullOrEmpty(r.Stratification))
                        .OrderByDescending(r => r.MeanContribution)
                        .OrderByDescending(r => r.Contribution)
                        .ToList();

                    if (unstratifiedRecords.Count > 0) {
                        var unstratifiedHiddenProperties = new List<string>(hiddenProperties) { "Stratification" };
                        var panelSb = new StringBuilder();
                        panelSb.Append(TableHelpers.CsvExportLink(
                            "IndividualContributionsBoxPlotTable", Model, Model.BoxPlotRecords, ViewBag, true, true)
                        );
                        panelSb.Append("<div class=\"figure-container\">");
                        {
                            var chartCreator = new IndividualContributionsBySubstanceBoxPlotChartCreator(
                                Model,
                                Model.ShowOutliers
                            );
                            panelSb.AppendChart(
                                "SubstanceContributionsIndividualRisksBoxPlot",
                                chartCreator,
                                ChartFileType.Svg,
                                Model,
                                ViewBag,
                                caption: chartCreator.Title,
                                saveChartFile: true
                            );
                        }
                        {
                            var chartCreator = new IndividualContributionsPieChartCreator(Model.SectionId, unstratifiedRecords, isUncertainty);
                            panelSb.AppendChart(
                                "SubstanceContributionsIndividualTotalChart",
                                chartCreator,
                                ChartFileType.Svg,
                                Model,
                                ViewBag,
                                caption: chartCreator.Title,
                                saveChartFile: true
                            );
                        }
                        panelSb.Append("</div>");
                        panelSb.AppendTable(
                            Model,
                            Model.IndividualContributionRecords,
                            "SubstanceContributionsIndividualRisksTable",
                            ViewBag,
                            displayLimit: 10,
                            caption: "Mean contributions to risk for individuals.",
                            saveCsv: true,
                            hiddenProperties: unstratifiedHiddenProperties
                        );
                        panelBuilder.AddPanel(
                            id: "RiskContributionsPanel",
                            title: "Contributions",
                            hoverText: "Contributions",
                            content: new HtmlString(panelSb.ToString())
                        );
                    }
                    var stratifiedRecords = results
                        .Where(r => !string.IsNullOrEmpty(r.Stratification))
                        .OrderBy(r => r.Stratification)
                        .OrderByDescending(r => r.MeanContribution)
                        .OrderByDescending(r => r.Contribution)
                        .Cast<RiskContributionBySubstanceRecord>()
                        .ToList();
                    if (stratifiedRecords.Count > 0) {
                        // Render stratified records
                        var panelSb = new StringBuilder();
                        var barChartCreator = new IndividualContributionsTotalStackedBarChartCreator(Model.SectionId, stratifiedRecords, isUncertainty);

                        panelSb.AppendChart(
                            name: $"SubstanceContributionsIndividualTotalStratifiedChart",
                            section: Model,
                            viewBag: ViewBag,
                            caption: barChartCreator.Title,
                            chartCreator: barChartCreator,
                            fileType: ChartFileType.Svg,
                            saveChartFile: true
                        );
                        panelSb.AppendTable(
                            Model,
                            stratifiedRecords,
                            $"SubstanceContributionsIndividualRisksTableStratifiedTable",
                            ViewBag,
                            caption: $"Mean contributions to risk for individuals.",
                            saveCsv: true,
                            hiddenProperties: hiddenProperties
                        );

                        panelBuilder.AddPanel(
                            id: "RiskContributionsStratifiedPanel",
                            title: "Stratified contributions",
                            hoverText: "Stratified contributions",
                            content: new HtmlString(panelSb.ToString())
                        );
                    }
                    panelBuilder.RenderPanel(sb, collapseSingleTab: true);
                }
            }
        }
    }
}

