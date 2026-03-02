using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AttributableBodSummarySectionView : SectionView<AttributableBodSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>() {
                nameof(AttributableBodSummaryRecord.SourceIndicators),
                nameof(AttributableBodSummaryRecord.ErfCode),
                nameof(AttributableBodSummaryRecord.EffectCode),
                nameof(AttributableBodSummaryRecord.EffectName),
                nameof(AttributableBodSummaryRecord.SubstanceCode),
                nameof(AttributableBodSummaryRecord.SubstanceName),
                nameof(AttributableBodSummaryRecord.PopulationCode),
                nameof(AttributableBodSummaryRecord.PopulationName),
            };
            var isUncertainty = Model.Records.FirstOrDefault()?.AttributableBods.Any() ?? false;

            if (isUncertainty) {
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.AttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.StandardisedAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.CumulativeAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.BinPercentage));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.Exposure));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.ResponseValue));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.AttributableFraction));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.TotalBod));
            } else {
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.LowerAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.UpperAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.MedianAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.LowerStandardisedAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.UpperStandardisedAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.MedianStandardisedAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.LowerCumulativeAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.UpperCumulativeAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.MedianCumulativeAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.LowerBoundExposure));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.UpperBoundExposure));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.MedianExposure));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.MedianBinPercentage));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.MedianResponseValue));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.MedianAttributableFraction));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.MedianTotalBod));
            }
            // Remove all standardised records when the population size is not specified
            // and in that case there is room for an extra column, otherwise surpress
            var missingPopulationSize = Model.Records.All(c => double.IsNaN(c.PopulationSize));
            if (missingPopulationSize) {
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.LowerStandardisedAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.UpperStandardisedAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.MedianStandardisedAttributableBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.StandardisedAttributableBod));
            } else {
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.ExposurePercentileBin));
            }
            var isBottomUp = Model.Records.All(r => r.AttributableFraction == 0D);
            if (isBottomUp) {
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.AttributableFraction));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.MedianAttributableFraction));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.TotalBod));
                hiddenProperties.Add(nameof(AttributableBodSummaryRecord.MedianTotalBod));
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
                        Model.StandardisationMethod,
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
                            Model.StandardisationMethod,
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
