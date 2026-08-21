using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class RiskContributionsBySubstanceSectionView : SectionView<RiskContributionsBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var results = new List<RiskContributionBySubstanceRecord>();
            var isUncertainty = Model.ContributionRecords.FirstOrDefault()?.Contributions.Any() ?? false;
            if (isUncertainty) {
                hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.Contribution));
                results = [.. Model.ContributionRecords.Where(c => c.MeanContribution > 0)];
            } else {
                hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.MeanContribution));
                results = [.. Model.ContributionRecords.Where(c => c.Contribution > 0)];
            }
            var unstratifiedHiddenProperties = new List<string>(hiddenProperties) {
                "Stratification"
            };
            //Render HTML
            if (results.Count != 0) {
                var panelBuilder = new HtmlTabPanelBuilder();
                var unstratifiedRecords = Model.ContributionRecords
                    .Where(r => string.IsNullOrEmpty(r.Stratification))
                    .OrderByDescending(r => r.MeanContribution)
                    .OrderByDescending(r => r.Contribution)
                    .ToList();
                if (unstratifiedRecords.Count > 0) {
                    var panelSb = new StringBuilder();
                    var chartCreator = new RiskContributionsBySubstancePieChartCreator(Model.SectionId, unstratifiedRecords, isUncertainty);
                    panelSb.AppendChart(
                        "RiskContributionsBySubstanceTotalChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );

                    panelSb.AppendDescriptionParagraph($"Total distribution {unstratifiedRecords.Count} substances.");
                    panelSb.AppendTable(
                        Model,
                        unstratifiedRecords,
                        "RiskContributionsBySubstanceTotalTable",
                        ViewBag,
                        caption: $"Risk contributions by substance (total distribution).",
                        saveCsv: true,
                        displayLimit: 10,
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
                    var barChartCreator = new RiskContributionBySubstanceTotalStackedBarChartCreator(Model.SectionId, stratifiedRecords, isUncertainty);

                    panelSb.AppendChart(
                        name: $"RiskContributionsBySubstanceTotalStratifiedChart",
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
                        $"RiskContributionsBySubstanceTotalStratifiedTable",
                        ViewBag,
                        caption: $"Risk contributions by substance (total distribution).",
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
            } else {
                sb.AppendNotification("No positive exposures.");
            }
        }
    }
}
