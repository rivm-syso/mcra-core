using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmDistributionBySubstanceTimePointSectionView<S, T1, T2>
        : SectionView<HbmConcentrationByDescriptorSectionBase<S, T1, T2>>
        where S : IHbmExposureContributorKey, new()
        where T1 : HbmConcentrationDistributionRecordBase<S>, new()
        where T2 : HbmBoxPlotRecordBase<S>, new() {

        public override void RenderSectionHtml(StringBuilder sb) {
            var day = string.Empty;
            if (Model.ExposureType == ExposureType.Acute) {
                day = "day";
            }
            var details = string.Empty;
            var caption = $"Human biomonitoring individual {day} concentrations by substance.";
            if (Model.DetailsSection) {
                details = "Details";
                caption = $"Human biomonitoring individual {day} concentrations by substance before matrix conversion.";
            }
            var positivesRecords = Model.Records
                .Where(r => r.MeanPositives > 0)
                .ToList();
            var missingActiveSubstanceData = Model.Records
                .GroupBy(r => r.GetDescriptorKey())
                .Where(records => records.All(r => r.SourceSamplingMethods == null))
                .ToList();
            if (missingActiveSubstanceData.Count > 0) {
                var description = $"Note: concentration data missing for {missingActiveSubstanceData.Count} " +
                    $"active substances ({string.Join(", ", missingActiveSubstanceData.Select(r => r.Key))}).";
                sb.AppendWarning(description);
            }

            if (Model.RestrictedUpperPercentile.HasValue) {
                var upper = Model.RestrictedUpperPercentile.Value;
                sb.AppendWarning("This section cannot be rendered because the sample size is insufficient for reporting the selected percentiles in accordance with the privacy guidelines." +
                    $" For the given sample size, only percentile values below p{upper:#0.##} can be reported.");
            } else {
                if (positivesRecords.Any()) {
                    var panelBuilder = new HtmlTabPanelBuilder();
                    foreach (var boxPlotRecord in Model.HbmBoxPlotRecords) {
                        var targetCode = boxPlotRecord.Key.Code;
                        var targetName = boxPlotRecord.Key.GetDisplayName();

                        var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                            name: $"HbmConcentrationsBySubstance{details}Percentiles_{targetCode}",
                            section: Model,
                            items: boxPlotRecord.Value,
                            viewBag: ViewBag
                        );

                        var chartCreator = new HbmConcentrationBySubstanceBoxPlotChartCreator<S, T2>    (
                            Model.HbmBoxPlotRecords[boxPlotRecord.Key],
                            boxPlotRecord.Key,
                            Model.SectionId,
                            Model.HbmBoxPlotRecords[boxPlotRecord.Key].FirstOrDefault()?.Unit ?? string.Empty,
                            Model.ShowOutliers
                        );

                        var numberOfRecords = boxPlotRecord.Value.Count;
                        var warning = Model.HbmBoxPlotRecords[boxPlotRecord.Key].Any(c => c.P95 == 0) ? "The asterisk indicates substances with positive measurements above an upper whisker of zero." : string.Empty;
                        var figCaption = Model.DetailsSection
                            ? $"{targetName} individual {day} concentrations by substance before conversion. " + chartCreator.Title + $" {warning}"
                            : $"{targetName} individual {day} concentrations by substance. " + chartCreator.Title + $" {warning}";
                        panelBuilder.AddPanel(
                            id: $"Panel_{targetCode}",
                            title: $"{targetName} ({numberOfRecords})",
                            hoverText: targetName,
                            content: ChartHelpers.Chart(
                                name: $"HbmConcentrationsBySubstanceTimePoints{details}BoxPlots_{targetCode}",
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

                    if (Model.StratifiedHbmBoxPlotRecords != null && Model.StratifiedHbmBoxPlotRecords.SelectMany(c => c.Value).Any()) {
                        foreach (var targetBoxPlotRecords in Model.StratifiedHbmBoxPlotRecords) {
                            var targetCode = targetBoxPlotRecords.Key.Code;
                            var targetName = targetBoxPlotRecords.Key.GetDisplayName();

                            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                                name: $"HbmConcentrationsBySubstance{details}Percentiles_{targetCode}",
                                section: Model,
                                items: targetBoxPlotRecords.Value,
                                viewBag: ViewBag
                            );

                            var chartCreator = new HbmStratifiedBoxPlotChartCreator<S, T2>   (
                                Model.StratifiedHbmBoxPlotRecords[targetBoxPlotRecords.Key],
                                targetBoxPlotRecords.Key,
                                Model.SectionId,
                                Model.StratifiedHbmBoxPlotRecords[targetBoxPlotRecords.Key].FirstOrDefault()?.Unit ?? string.Empty,
                                Model.ShowOutliers
                            );

                            var numberOfRecords = targetBoxPlotRecords.Value.Count;
                            var warning = Model.StratifiedHbmBoxPlotRecords[targetBoxPlotRecords.Key].Any(c => c.P95 == 0)
                                ? "The asterisk indicates substances with positive measurements above an upper whisker of zero."
                                : string.Empty;
                            var figCaption = Model.DetailsSection
                                ? $"{targetName} individual {day} concentrations by substance before conversion. " + chartCreator.Title + $" {warning}"
                                : $"{targetName} individual {day} concentrations by substance. " + chartCreator.Title + $" {warning}";
                            panelBuilder.AddPanel(
                                id: $"Panel_{targetCode}",
                                title: $"{targetName} stratified",
                                hoverText: targetName,
                                content: ChartHelpers.Chart(
                                    name: $"HbmConcentrationsBySubstance{details}BoxPlots_{targetCode}",
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
                    }
                    panelBuilder.RenderPanel(sb, collapseSingleTab: true);

                    var hiddenProperties = new List<string>();
                    if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                        hiddenProperties.Add("BiologicalMatrix");
                    }
                    if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                        hiddenProperties.Add("ExposureRoute");
                    }
                    if (Model.Records.All(r => string.IsNullOrEmpty(r.Stratification))) {
                        hiddenProperties.Add("Stratification");
                    }
                    if (Model.Records.All(r => string.IsNullOrEmpty(r.ExpressionType))) {
                        hiddenProperties.Add("ExpressionType");
                    }
                    if (Model.Records.All(r => double.IsNaN(r.MedianAllLowerBoundPercentile))) {
                        hiddenProperties.Add("MedianAllMedianPercentile");
                        hiddenProperties.Add("MedianAllLowerBoundPercentile");
                        hiddenProperties.Add("MedianAllUpperBoundPercentile");
                    } else {
                        hiddenProperties.Add("MedianAll");
                    }

                    sb.AppendTable(
                        Model,
                        positivesRecords,
                        $"HbmConcentrationsBySubstance{details}Table",
                        ViewBag,
                        caption: caption,
                        saveCsv: true,
                        header: true,
                        hiddenProperties: hiddenProperties
                    );
                } else {
                    sb.AppendNotification("No concentrations available.");
                }
            }
        }
    }
}
