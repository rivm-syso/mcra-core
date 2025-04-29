using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AttributableBodSummarySectionView : SectionView<AttributableBodSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>() {
                "BodIndicator",
                "ExposureResponseFunctionCode",
                "Unit",
                "PopulationCode",
                "PopulationName"
            };
            var isUncertainty = Model.Records.FirstOrDefault()?.AttributableBods.Any() ?? false;

            if (isUncertainty) {
                hiddenProperties.Add("AttributableBod");
                hiddenProperties.Add("StandardizedAttributableBod");
                hiddenProperties.Add("StandardizedExposedAttributableBod");
                hiddenProperties.Add("CumulativeAttributableBod");
                hiddenProperties.Add("CumulativeStandardizedExposedAttributableBod");
                hiddenProperties.Add("Exposure");
                hiddenProperties.Add("BinPercentage");
                hiddenProperties.Add("TotalBod");
            } else {
                hiddenProperties.Add("LowerAttributableBod");
                hiddenProperties.Add("UpperAttributableBod");
                hiddenProperties.Add("MedianAttributableBod");
                hiddenProperties.Add("LowerStandardizedAttributableBod");
                hiddenProperties.Add("UpperStandardizedAttributableBod");
                hiddenProperties.Add("MedianStandardizedAttributableBod");
                hiddenProperties.Add("LowerStandardizedExposedAttributableBod");
                hiddenProperties.Add("UpperStandardizedExposedAttributableBod");
                hiddenProperties.Add("MedianStandardizedExposedAttributableBod");
                hiddenProperties.Add("LowerCumulativeAttributableBod");
                hiddenProperties.Add("UpperCumulativeAttributableBod");
                hiddenProperties.Add("MedianCumulativeAttributableBod");
                hiddenProperties.Add("LowerCumulativeStandardizedExposedAttributableBod");
                hiddenProperties.Add("UpperCumulativeStandardizedExposedAttributableBod");
                hiddenProperties.Add("MedianCumulativeStandardizedExposedAttributableBod");
                hiddenProperties.Add("LowerBoundExposure");
                hiddenProperties.Add("UpperBoundExposure");
                hiddenProperties.Add("MedianExposure");
                hiddenProperties.Add("MedianBinPercentage");
                hiddenProperties.Add("MedianTotalBod");
            }
            // Remove all standardized records when the population size is not specified
            // and in that case there is room for an extra column, otherwise surpress
            var missingPopulationSize = Model.Records.All(c => double.IsNaN(c.PopulationSize));
            if (missingPopulationSize) {
                hiddenProperties.Add("LowerStandardizedAttributableBod");
                hiddenProperties.Add("UpperStandardizedAttributableBod");
                hiddenProperties.Add("MedianStandardizedAttributableBod");
                hiddenProperties.Add("LowerStandardizedExposedAttributableBod");
                hiddenProperties.Add("UpperStandardizedExposedAttributableBod");
                hiddenProperties.Add("MedianStandardizedExposedAttributableBod");
                hiddenProperties.Add("LowerCumulativeStandardizedExposedAttributableBod");
                hiddenProperties.Add("UpperCumulativeStandardizedExposedAttributableBod");
                hiddenProperties.Add("MedianCumulativeStandardizedExposedAttributableBod");
                hiddenProperties.Add("StandardizedAttributableBod");
                hiddenProperties.Add("StandardizedExposedAttributableBod");
                hiddenProperties.Add("CumulativeStandardizedExposedAttributableBod");
            } else {
                hiddenProperties.Add("ExposurePercentileBin");
            }
            if (Model.Records.All(r => r.AttributableFraction == 0D)) {
                hiddenProperties.Add("AttributableFraction");
            }

            var panelBuilder = new HtmlTabPanelBuilder();

            var panelGroup = Model.Records
                .Where(r => !isUncertainty && !(r.TotalBod == 0D && r.CumulativeAttributableBod == 100D) ||
                    isUncertainty && !(r.UpperAttributableBod == 0D && r.MedianCumulativeAttributableBod == 100D)
                )
                .GroupBy(r => (r.PopulationName, r.BodIndicator, r.ExposureResponseFunctionCode));

            foreach (var group in panelGroup) {
                var panelSb = new StringBuilder();
                var key = $"{group.Key.PopulationName}-{group.Key.BodIndicator}-{group.Key.ExposureResponseFunctionCode}";
                panelSb.AppendTable(
                    Model,
                    [.. group],
                    $"AttributableBodTable_{key}",
                    ViewBag,
                    header: true,
                    caption: $"Attributable burden of disease {key}.",
                    saveCsv: true,
                    sortable: true,
                    hiddenProperties: hiddenProperties
                );
                var chartCreator = new AttributableBodChartCreator(
                    [.. group],
                    Model.SectionId
                );

                var chart = ChartHelpers.Chart(
                    name: $"AttributableBodChart{key}",
                    section: Model,
                    viewBag: ViewBag,
                    caption: chartCreator.Title,
                    chartCreator: chartCreator,
                    fileType: ChartFileType.Svg,
                    saveChartFile: true
                );

                var chartStandardized = string.Empty;
                var chartExposed = string.Empty;
                if (!missingPopulationSize) {
                    var chartCreatorStandardized = new StandardizedAttributableBodChartCreator(
                        [.. group],
                        Model.SectionId
                    );
                    var chartCreatorExposed = new StandardizedExposedAttributableBodChartCreator(
                        [.. group],
                        Model.SectionId
                    );

                     chartStandardized = ChartHelpers.Chart(
                            name: $"StandardizedAttributableBodChart{key}",
                            section: Model,
                            viewBag: ViewBag,
                            caption: chartCreatorStandardized.Title,
                            chartCreator: chartCreatorStandardized,
                            fileType: ChartFileType.Svg,
                            saveChartFile: true
                        ).ToString();

                     chartExposed = ChartHelpers.Chart(
                            name: $"ExposedAttributableBodChart{key}",
                            section: Model,
                            viewBag: ViewBag,
                            caption: chartCreatorExposed.Title,
                            chartCreator: chartCreatorExposed,
                            fileType: ChartFileType.Svg,
                            saveChartFile: true
                        ).ToString();
                }

                var contentPanel = new HtmlString(
                    "<div class=\"figure-container\">"
                    + chart + chartStandardized + chartExposed
                    + "</div>"
                    + panelSb.ToString()
                );

                panelBuilder.AddPanel(
                    id: $"Panel_{key}",
                    title: $"{key}",
                    hoverText: key,
                    content: contentPanel
                );
            }
            panelBuilder.RenderPanel(sb);
        }
    }
}
