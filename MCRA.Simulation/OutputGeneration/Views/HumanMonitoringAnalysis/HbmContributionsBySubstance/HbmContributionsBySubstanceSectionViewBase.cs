using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public abstract class HbmContributionsBySubstanceSectionViewBase 
        : SectionView<HbmContributionsSectionBase<SubstanceContributorKey, HbmContributionBySubstanceRecord>> {

        public override void RenderSectionHtml(StringBuilder sb) {
            var results = new List<HbmContributionBySubstanceRecord>();
            var isUncertainty = Model.Records.Any(c => c.Contributions.Count > 0);

            var hiddenProperties = new List<string>();
            if (isUncertainty) {
                hiddenProperties.Add("Contribution");
                results = [.. Model.Records.Where(c => c.MeanContribution > 0)];
            } else {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
                results = [.. Model.Records.Where(c => c.Contribution > 0)];
            }
            var unstratifiedHiddenProperties = new List<string>(hiddenProperties) {
                "Stratification"
            };

            if (results.Count != 0) {
                var panelBuilder = new HtmlTabPanelBuilder();
                var unstratifiedRecords = Model.Records
                    .Where(r => string.IsNullOrEmpty(r.Stratification))
                    .OrderByDescending(r => r.MeanContribution)
                    .OrderByDescending(r => r.Contribution)
                    .ToList();

                // Declare for total and upper sections: piechart and distribution strings, description, and chart creator 
                var pieChartString = string.Empty;
                var distributionString = string.Empty;
                var description = string.Empty;
                HbmRiskDriverContributionsPieChartCreatorBase<SubstanceContributorKey, HbmContributionBySubstanceRecord> chartCreator = null;
                if (Model is HbmContributionsBySubstanceUpperSection) {
                    var upperSection = Model as HbmContributionsBySubstanceUpperSection;
                    pieChartString = "Upper";
                    distributionString = "upper distribution";
                    description = $"Exposure: upper tail {upperSection.CalculatedUpperPercentage:F1}% ({upperSection.NumberOfIntakes} records), " +
                        $"minimum value {upperSection.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                        $"maximum value {upperSection.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}";
                    chartCreator = new HbmContributionsBySubstanceUpperPieChartCreator(Model.SectionId, unstratifiedRecords, upperSection.UpperPercentage, isUncertainty);
                } else {
                    pieChartString = "Total";
                    distributionString = "total distribution";
                    chartCreator = new HbmContributionsBySubstanceTotalPieChartCreator(Model.SectionId, unstratifiedRecords, isUncertainty);
                }

                if (unstratifiedRecords.Count > 0) {
                    var panelSb = new StringBuilder();
                    panelSb.Append(description);
                    panelSb.AppendChart(
                        name: $"Hbm{pieChartString}DistributionRiskDriversChart",
                        section: Model,
                        viewBag: ViewBag,
                        caption: chartCreator.Title,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true
                    );

                    panelSb.AppendDescriptionParagraph($"Number of substances: {unstratifiedRecords.Count}");

                    panelSb.AppendTable(
                        Model,
                        unstratifiedRecords,
                        $"Hbm{pieChartString}DistributionRiskDriversTable",
                        ViewBag,
                        caption: $"Contributions of substances ({distributionString}).",
                        saveCsv: true,
                        hiddenProperties: unstratifiedHiddenProperties
                    );

                    panelBuilder.AddPanel(
                        id: "HbmRiskDriverContributionsPanel",
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
                    .Cast<HbmContributionBySubstanceRecord>()
                    .ToList();
                if (stratifiedRecords.Count > 0) {
                    // Render stratified records
                    var panelSb = new StringBuilder();
                    HbmStackedBarChartCreatorBase<SubstanceContributorKey, HbmContributionBySubstanceRecord> barChartCreator = new HbmContributionsBySubstanceTotalStackedBarChartCreator(Model.SectionId, stratifiedRecords, isUncertainty);

                    panelSb.AppendChart(
                        name: $"Hbm{pieChartString}DistributionStratifiedRiskDriversChart",
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
                        $"Hbm{pieChartString}DistributionStratifiedRiskDriversTable",
                        ViewBag,
                        caption: $"Contributions of substances ({distributionString}).",
                        saveCsv: true,
                        hiddenProperties: hiddenProperties
                    );

                    panelBuilder.AddPanel(
                        id: "HbmRiskDriverContributionsStratifiedPanel",
                        title: "Stratified contributions",
                        hoverText: "Stratified contributions",
                        content: new HtmlString(panelSb.ToString())
                    );
                }

                panelBuilder.RenderPanel(sb, collapseSingleTab: true);
            } else {
                sb.AppendDescriptionParagraph("No positive concentrations found");
            }
        }
    }
}
