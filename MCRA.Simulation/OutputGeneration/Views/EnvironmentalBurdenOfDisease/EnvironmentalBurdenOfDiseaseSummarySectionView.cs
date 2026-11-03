using System.Text;
using MCRA.Simulation.OutputGeneration.ActionSummaries.EnvironmentalBurdenOfDisease;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class EnvironmentalBurdenOfDiseaseSummarySectionView : SectionView<EnvironmentalBurdenOfDiseaseSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();
            if (Model.Records.All(c => double.IsInfinity(c.StandardisedTotalAttributableBod))) {
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.StandardisedTotalAttributableBod));
            }
            var isUncertainty = Model.Records.FirstOrDefault()?.TotalAttributableBods.Any() ?? false;
            if (isUncertainty) {
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.TotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.StandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.TotalPopulationAttributableFraction));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.TotalPopulationShift));
            } else {
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.MedianTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.LowerTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.UpperTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.MedianStandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.LowerStandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.UpperStandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.MedianTotalPopulationAttributableFraction));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.LowerTotalPopulationAttributableFraction));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.UpperTotalPopulationAttributableFraction));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.MedianTotalPopulationShift));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.LowerTotalPopulationShift));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.UpperTotalPopulationShift));
            }
            var hasBod = !Model.Records.All(c => c.TotalAttributableBod == 0D) &&
                !Model.Records.All(c => c.UpperTotalAttributableBod == 0D);
            var hasShift = !Model.Records.All(c => c.TotalPopulationShift == 0D) &&
                !Model.Records.All(c => c.UpperTotalPopulationShift == 0D);
            if (!hasBod) {
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.TotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.MedianTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.LowerTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.UpperTotalAttributableBod));
            }
            if (!hasShift) {
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.TotalPopulationShift));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.MedianTotalPopulationShift));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.LowerTotalPopulationShift));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.UpperTotalPopulationShift));
            }

            if (Model.Records.All(r => string.IsNullOrEmpty(r.SourceIndicators))) {
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.SourceIndicators));
            }
            var recordsGrouped = Model.Records
                .GroupBy(r => (r.PopulationCode, r.SubstanceCode, r.EffectCode, r.BodIndicator));
            if (recordsGrouped.All(r => r.Count() <= 1)) {
                // Don't display ERF code/name unless multiple ERFs are used for same population,
                // substance, effect, and BoD indicator. 
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.ErfCode));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.ErfName));
            }

            var missingPopulationSize = Model.Records.All(c => double.IsNaN(c.PopulationSize));
            if (missingPopulationSize) {
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.StandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.MedianStandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.LowerStandardisedTotalAttributableBod));
                hiddenProperties.Add(nameof(EnvironmentalBurdenOfDiseaseSummaryRecord.UpperStandardisedTotalAttributableBod));
            }

            var caption = Model.IsCumulative
                ? "Environmental burden of disease overview (cumulative exposure)."
                : "Environmental burden of disease overview.";
            sb.AppendTable(
                Model,
                Model.Records,
                "EnvironmentalBurdenOfDiseaseSummaryTable",
                ViewBag,
                caption: caption,
                saveCsv: true,
                sortable: false,
                hiddenProperties: hiddenProperties
            );

            if (Model.Records.GroupBy(r => (r.BodIndicator, r.EffectCode)).Any(g => g.Count() > 0)) {
                sb.AppendWarning($"Summary of total attributable burden omitted because of potential double-counting of the same effect.");
            } else {
                var panelBuilder = new HtmlTabPanelBuilder();
                var panelGroup = Model.Records
                    .Where(r => (!isUncertainty && r.TotalAttributableBod != 0) || (isUncertainty && r.MedianTotalAttributableBod != 0))
                    .GroupBy(r => r.BodIndicator);

                foreach (var group in panelGroup) {
                    var key = group.Key;
                    var name = group.Key;

                    var panelSb = new StringBuilder();
                    panelSb.Append("<div class=\"figure-container\">");

                    var chartSubstanceByEffectCreator = new EbdStackedBarSubstanceByEffectChartCreator(
                        Model.SectionId,
                        Model.Records,
                        isUncertainty
                    );
                    panelSb.AppendChart(
                            name: $"EbdSubstanceByEffectChart_{key}",
                            section: Model,
                            chartCreator: chartSubstanceByEffectCreator,
                            fileType: ChartFileType.Svg,
                            viewBag: ViewBag,
                            saveChartFile: true,
                            caption: chartSubstanceByEffectCreator.Title
                        );

                    var chartEffectBySubstanceCreator = new EbdStackedBarEffectBySubstanceChartCreator(
                        Model.SectionId,
                        Model.Records,
                        isUncertainty
                    );
                    panelSb.AppendChart(
                        name: $"EbdEffectBySubstamceChart_{key}",
                        section: Model,
                        chartCreator: chartEffectBySubstanceCreator,
                        fileType: ChartFileType.Svg,
                        viewBag: ViewBag,
                        saveChartFile: true,
                        caption: chartEffectBySubstanceCreator.Title
                    );

                    panelSb.Append("\"</div>\"");
                    panelBuilder.AddPanel(
                        id: $"Panel_{key}",
                        title: name,
                        hoverText: key,
                        content: new HtmlString(sb.ToString())
                    );

                    panelBuilder.RenderPanel(sb, collapseSingleTab: true);
                }
            }
        }
    }
}
