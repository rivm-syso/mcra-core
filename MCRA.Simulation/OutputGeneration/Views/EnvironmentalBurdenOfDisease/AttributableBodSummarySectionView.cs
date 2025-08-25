using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AttributableBodSummarySectionView : SectionView<AttributableBodSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>() {
                "SourceIndicators",
                "ErfCode",
                "EffectCode",
                "EffectName",
                "SubstanceCode",
                "SubstanceName",
                "Unit",
                "PopulationCode",
                "PopulationName",
                // Hide all columns with standardized EXPOSED attributable BoD
                "UpperCumulativeStandardisedExposedAttributableBod",
                "LowerCumulativeStandardisedExposedAttributableBod",
                "MedianCumulativeStandardisedExposedAttributableBod",
                "CumulativeStandardisedExposedAttributableBod",
                "UpperStandardisedExposedAttributableBod",
                "LowerStandardisedExposedAttributableBod",
                "MedianStandardisedExposedAttributableBod",
                "StandardisedExposedAttributableBod"
            };
            var isUncertainty = Model.Records.FirstOrDefault()?.AttributableBods.Any() ?? false;

            if (isUncertainty) {
                hiddenProperties.Add("AttributableBod");
                hiddenProperties.Add("StandardisedAttributableBod");
                hiddenProperties.Add("CumulativeAttributableBod");
                hiddenProperties.Add("BinPercentage");
                hiddenProperties.Add("Exposure");
                hiddenProperties.Add("ResponseValue");
                hiddenProperties.Add("AttributableFraction");
                hiddenProperties.Add("TotalBod");
            } else {
                hiddenProperties.Add("LowerAttributableBod");
                hiddenProperties.Add("UpperAttributableBod");
                hiddenProperties.Add("MedianAttributableBod");
                hiddenProperties.Add("LowerStandardisedAttributableBod");
                hiddenProperties.Add("UpperStandardisedAttributableBod");
                hiddenProperties.Add("MedianStandardisedAttributableBod");
                hiddenProperties.Add("LowerCumulativeAttributableBod");
                hiddenProperties.Add("UpperCumulativeAttributableBod");
                hiddenProperties.Add("MedianCumulativeAttributableBod");
                hiddenProperties.Add("LowerBoundExposure");
                hiddenProperties.Add("UpperBoundExposure");
                hiddenProperties.Add("MedianExposure");
                hiddenProperties.Add("MedianBinPercentage");
                hiddenProperties.Add("MedianResponseValue");
                hiddenProperties.Add("MedianAttributableFraction");
                hiddenProperties.Add("MedianTotalBod");
            }
            // Remove all standardised records when the population size is not specified
            // and in that case there is room for an extra column, otherwise surpress
            var missingPopulationSize = Model.Records.All(c => double.IsNaN(c.PopulationSize));
            if (missingPopulationSize) {
                hiddenProperties.Add("LowerStandardisedAttributableBod");
                hiddenProperties.Add("UpperStandardisedAttributableBod");
                hiddenProperties.Add("MedianStandardisedAttributableBod");
                hiddenProperties.Add("LowerStandardisedExposedAttributableBod");
                hiddenProperties.Add("UpperStandardisedExposedAttributableBod");
                hiddenProperties.Add("MedianStandardisedExposedAttributableBod");
                hiddenProperties.Add("LowerCumulativeStandardisedExposedAttributableBod");
                hiddenProperties.Add("UpperCumulativeStandardisedExposedAttributableBod");
                hiddenProperties.Add("MedianCumulativeStandardisedExposedAttributableBod");
                hiddenProperties.Add("StandardisedAttributableBod");
                hiddenProperties.Add("StandardisedExposedAttributableBod");
                hiddenProperties.Add("CumulativeStandardisedExposedAttributableBod");
            } else {
                hiddenProperties.Add("ExposurePercentileBin");
            }
            var isBottomUp = Model.Records.All(r => r.AttributableFraction == 0D);
            if (isBottomUp) {
                hiddenProperties.Add("AttributableFraction");
                hiddenProperties.Add("MedianAttributableFraction");
                hiddenProperties.Add("TotalBod");
                hiddenProperties.Add("MedianTotalBod");
            }

            var panelBuilder = new HtmlTabPanelBuilder();

            var panelGroup = Model.Records
                .Where(r => (!isUncertainty && !(r.TotalBod == 0D && r.CumulativeAttributableBod == 100D)) ||
                    (isUncertainty && !(r.UpperAttributableBod == 0D && r.MedianCumulativeAttributableBod == 100D))
                )
                .GroupBy(r => r.GetGroupKey());

            foreach (var group in panelGroup) {
                var key = group.Key;
                var name = group.First().GetGroupDisplayName();

                var panelSb = new StringBuilder();
                if (!group.All(g => g.AttributableBod == 0D)) {
                    
                    var populationSize = group.FirstOrDefault().PopulationSize;
                    if (!double.IsNaN(populationSize)) {
                        panelSb.AppendDescriptionParagraph($"Population size: {string.Format("{0:N0}", populationSize)}");
                    }

                    // Create copy of viewbag and fill with (local) BodIndicator
                    var viewBag = ViewBag.Clone();
                    viewBag.UnitsDictionary.Add("BodIndicator", group.First().BodIndicator ?? string.Empty);

                    panelSb.AppendTable(
                        Model,
                        [.. group],
                        $"AttributableBodTable_{key}",
                        viewBag,
                        header: true,
                        caption: $"Attributable burden of disease {name}.",
                        saveCsv: true,
                        sortable: true,
                        hiddenProperties: hiddenProperties
                    );
                    var chartCreator = new AttributableBodChartCreator(
                        [.. group],
                        Model.SectionId
                    );

                    var chart = ChartHelpers.Chart(
                        name: $"AttributableBodChart_{key}",
                        section: Model,
                        viewBag: ViewBag,
                        caption: chartCreator.Title,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true
                    );

                    var chartStandardised = string.Empty;
                    var chartExposed = string.Empty;
                    if (!missingPopulationSize) {
                        var chartCreatorStandardised = new StandardisedAttributableBodChartCreator(
                            [.. group],
                            Model.SectionId
                        );
                        var chartCreatorExposed = new StandardisedExposedAttributableBodChartCreator(
                            [.. group],
                            Model.SectionId
                        );

                        chartStandardised = ChartHelpers
                            .Chart(
                               name: $"StandardisedAttributableBodChart_{key}",
                               section: Model,
                               viewBag: ViewBag,
                               caption: chartCreatorStandardised.Title,
                               chartCreator: chartCreatorStandardised,
                               fileType: ChartFileType.Svg,
                               saveChartFile: true
                           ).ToString();

                        // Hide charts with standardized EXPOSED attributable BoD
                        //chartExposed = ChartHelpers.Chart(
                        //       name: $"ExposedAttributableBodChart{key}{id}",
                        //       section: Model,
                        //       viewBag: ViewBag,
                        //       caption: chartCreatorExposed.Title,
                        //       chartCreator: chartCreatorExposed,
                        //       fileType: ChartFileType.Svg,
                        //       saveChartFile: true
                        //   ).ToString();
                    }

                    var contentPanel = new HtmlString(
                        "<div class=\"figure-container\">"
                        + chart + chartStandardised + chartExposed
                        + "</div>"
                        + panelSb.ToString()
                    );

                    panelBuilder.AddPanel(
                        id: $"Panel_{key}",
                        title: name,
                        hoverText: key,
                        content: contentPanel
                    );
                } else {
                    panelSb.AppendNotification($"No attributable burden of disease above the threshold for {key}.");
                    panelBuilder.AddPanel(
                        id: $"Panel_{key}",
                        title: name,
                        hoverText: key,
                        content: new HtmlString(panelSb.ToString())
                    );
                }
            }
            panelBuilder.RenderPanel(sb);
        }
    }
}
