using System.Text;
using DocumentFormat.OpenXml.Drawing.Charts;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureDistributionPercentilesSectionView : SectionView<ExposureDistributionPercentilesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            sb.AppendDescriptionParagraph($"Number of records: {Model.Records?.Count ?? 0}");

            var hiddenProperties = new List<string> {
                nameof(TargetExposurePercentileRecord.SubstanceCode),
                nameof(TargetExposurePercentileRecord.SubstanceName),
                nameof(TargetExposurePercentileRecord.Route),
                nameof(TargetExposurePercentileRecord.Source)
            };
            if (Model.Records.All(c => string.IsNullOrEmpty(c.Stratification))) {
                hiddenProperties.Add(nameof(TargetExposurePercentileRecord.Stratification));
            }
            if (Model.Records.All(c => c.Values.Count == 0)) {
                hiddenProperties.Add(nameof(TargetExposurePercentileRecord.Median));
                hiddenProperties.Add(nameof(TargetExposurePercentileRecord.LowerBound));
                hiddenProperties.Add(nameof(TargetExposurePercentileRecord.UpperBound));
            }
            var panelBuilder = new HtmlTabPanelBuilder();

            bool showUncertainty = Model.Records.Any(r => !double.IsNaN(r.LowerBound) && r.LowerBound > 0);
            if (showUncertainty) {
                var lowerBound = Model.UncertaintyLowerLimit;
                var upperBound = Model.UncertaintyUpperLimit;
                if (upperBound < lowerBound) {
                    (lowerBound, upperBound) = (upperBound, lowerBound);
                    Model.UncertaintyLowerLimit = lowerBound;
                    Model.UncertaintyUpperLimit = upperBound;
                }
                var upperBoxDefault = 75D;
                var lowerBoxDefault = 25D;
                if (upperBound < upperBoxDefault) {
                    upperBoxDefault = upperBound;
                }
                if (lowerBound > lowerBoxDefault) {
                    lowerBoxDefault = lowerBound;
                }
                //create chart data section
                var bootstrapResultsDataSection = DataSectionHelper.CreateCsvDataSection(
                    "ExposurePercentilesBootstrapTable", Model, Model.GetPercentileBootstrapRecords(false),
                    ViewBag, true, hiddenProperties
                );
                var values = Model.Records.SelectMany(c => c.Values).ToList();
                var maximum = values.Max() * 1.1;
                var minimum = values.Min() * 0.9;
                var unstratifiedChartCreator = new ExposureDistributionPercentileChartCreator(
                    Model,
                    ViewBag.GetUnit("IntakeUnit"),
                    0,
                    minimum,
                    maximum
                );
                panelBuilder.AddPanel(
                    id: "Panel_unstratified",
                    title: "Unstratified",
                    hoverText: "Unstratified",
                    content: ChartHelpers
                        .Chart(
                            name: "UnstratifiedIntakePercentileChart",
                            section: Model,
                            chartCreator: unstratifiedChartCreator,
                            fileType: ChartFileType.Svg,
                            viewBag: ViewBag,
                            saveChartFile: true,
                            caption: unstratifiedChartCreator.Title,
                            chartData: bootstrapResultsDataSection
                        )
                );
                var categories = Model.Records
                    .Where(r => !string.IsNullOrEmpty(r.Stratification))
                    .Select(c => c.Stratification)
                    .OrderBy(c => c)
                    .ToHashSet();
                if (Model.Stratify) {
                    var htmlString = string.Empty;
                    var paletteColor = 0;
                    foreach (var category in categories) {
                        var stratifiedChartCreator = new ExposureDistributionPercentileChartCreator(
                            Model,
                            ViewBag.GetUnit("IntakeUnit"),
                            paletteColor,
                            minimum,
                            maximum,
                            categories.Count,
                            category
                        );
                        var chart = ChartHelpers
                            .Chart(
                                name: "StratifiedIntakePercentileChart",
                                section: Model,
                                chartCreator: stratifiedChartCreator,
                                fileType: ChartFileType.Svg,
                                viewBag: ViewBag,
                                saveChartFile: true,
                                caption: stratifiedChartCreator.Title,
                                chartData: bootstrapResultsDataSection
                            ).ToString();
                        htmlString += chart;
                        paletteColor++;
                    }
                    panelBuilder.AddPanel(
                        id: "Panel_stratified",
                        title: "Stratified",
                        hoverText: "Stratified",
                        content: new HtmlString(
                            "<div class=\"figure-container\">"
                            + htmlString
                            + "</div>"
                        )
                    );
                }
                sb.AppendDescriptionParagraph($"The boxplots for uncertainty show the p{lowerBoxDefault} and p{upperBoxDefault} as edges of the box, " +
                    $"and p{lowerBound} and p{upperBound} as edges of the whiskers. The reference value is indicated with the dashed black line, the median " +
                    $"with the solid black line within the box. Outliers are displayed as dots outside the wiskers.");
                panelBuilder.RenderPanel(sb, collapseSingleTab: true);
            }


            if (Model.Records?.Count > 0) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "IntakePercentileTable",
                    ViewBag,
                    header: true,
                    caption: "Percentiles",
                    saveCsv: true,
                    sortable: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
