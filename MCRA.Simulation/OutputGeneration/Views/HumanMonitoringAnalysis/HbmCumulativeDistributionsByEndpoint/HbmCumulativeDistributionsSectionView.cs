using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmCumulativeDistributionsSectionView<S, T1, T2>
        : SectionView<HbmConcentrationByDescriptorSectionBase<S, T1, T2>>
        where S : IHbmExposureContributorKey, new()
        where T1 : HbmConcentrationDistributionRecordBase<S>, new()
        where T2 : HbmBoxPlotRecordBase<S>, new() {

        public override void RenderSectionHtml(StringBuilder sb) {
            var day = string.Empty;
            if (Model.ExposureType == ExposureType.Acute) {
                day = "day";
            }
            if (Model.RestrictedUpperPercentile.HasValue) {
                var upper = Model.RestrictedUpperPercentile.Value;
                sb.AppendWarning("This section cannot be rendered because the sample size is insufficient for reporting the selected percentiles in accordance with the privacy guidelines." +
                    $" For the given sample size, only percentile values below p{upper:#0.##} can be reported.");
            } else {
                if (Model.Records.Any()) {
                    var panelBuilder = new HtmlTabPanelBuilder();
                    var item = Model.HbmBoxPlotRecords.First();
                    var targetCode = item.Key.Code;
                    var targetName = item.Key.GetDisplayName();
                    {
                        var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                            "HbmCumulativeConcentrationsPercentiles",
                            Model,
                            item.Value,
                            ViewBag
                        );
                        var chartCreator = new HbmCumulativeDistributionsBoxPlotChartCreator<S, T2>(item.Value, Model.ExposureType, Model.SectionId);
                        panelBuilder.AddPanel(
                            id: $"Panel_{targetCode}",
                            title: $"{targetName}",
                            hoverText: targetName,
                            content: ChartHelpers.Chart(
                                name: "HbmCumulativeConcentrationsBoxPlotChart",
                                section: Model,
                                viewBag: ViewBag,
                                chartCreator: chartCreator,
                                fileType: ChartFileType.Svg,
                                saveChartFile: true,
                                caption: chartCreator.Title,
                                chartData: percentileDataSection
                            ));
                    }
                    var targetBoxPlotRecords = Model.StratifiedHbmBoxPlotRecords.First();
                    if (targetBoxPlotRecords.Value.Count != 0) {
                        var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                            name: $"HbmConcentrationsBySubstancePercentiles_{targetCode}",
                            section: Model,
                            items: [targetBoxPlotRecords],
                            viewBag: ViewBag
                        );

                        var chartCreator = new HbmStratifiedBoxPlotChartCreator<S, T2>(
                            targetBoxPlotRecords.Value,
                            targetBoxPlotRecords.Key,
                            Model.SectionId,
                            targetBoxPlotRecords.Value.FirstOrDefault()?.Unit ?? string.Empty,
                            Model.ShowOutliers
                        );

                        var warning = targetBoxPlotRecords.Value.Any(c => c.P95 == 0)
                            ? "The asterisk indicates substances with positive measurements above an upper whisker of zero."
                            : string.Empty;
                        var figCaption = $"{targetName} individual {day} concentrations cumulative. " + chartCreator.Title + $" {warning}";
                        panelBuilder.AddPanel(
                            id: $"Panel_{targetCode}",
                            title: $"{targetName} stratified",
                            hoverText: targetName,
                            content: ChartHelpers.Chart(
                                name: $"HbmConcentrationsBySubstanceBoxPlots_{targetCode}",
                                section: Model,
                                viewBag: ViewBag,
                                chartCreator: chartCreator,
                                fileType: ChartFileType.Svg,
                                saveChartFile: true,
                                caption: figCaption,
                                chartData: percentileDataSection
                            )
                        );
                    }

                    panelBuilder.RenderPanel(sb, collapseSingleTab: true);
                    var hiddenProperties = new List<string> {
                        "SubstanceCode",
                        "SourceSamplingMethods",
                    };
                    if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                        hiddenProperties.Add("ExposureRoute");
                    }
                    if (Model.Records.All(r => string.IsNullOrEmpty(r.Stratification))) {
                        hiddenProperties.Add("Stratification");
                    }
                    if (Model.Records.All(r => string.IsNullOrEmpty(r.ExpressionType))) {
                        hiddenProperties.Add("ExpressionType");
                    }
                    if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                        hiddenProperties.Add("BiologicalMatrix");
                    }
                    if (Model.Records.All(r => !r.MedianAllUncertaintyValues?.Any() ?? true)) {
                        hiddenProperties.Add("MedianAllMedianPercentile");
                        hiddenProperties.Add("MedianAllLowerBoundPercentile");
                        hiddenProperties.Add("MedianAllUpperBoundPercentile");
                    }

                    sb.AppendTable(
                        Model,
                        Model.Records,
                        "HbmCumulativeConcentrationsTable",
                        ViewBag,
                        caption: $"Human biomonitoring individual {day} measurement distribution endpoint cumulative substance.",
                        saveCsv: true,
                        header: true,
                        hiddenProperties: hiddenProperties
                    );
                } else {
                    sb.AppendDescriptionParagraph($"After removing individual{day}s with missing values, no data are left.");
                }
            }
        }
    }
}
