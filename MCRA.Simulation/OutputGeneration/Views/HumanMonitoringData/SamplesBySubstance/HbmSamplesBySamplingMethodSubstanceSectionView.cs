﻿using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmSamplesBySamplingMethodSubstanceSectionView : SectionView<HbmSamplesBySamplingMethodSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.RestrictedUpperPercentile.HasValue) {
                var upper = Model.RestrictedUpperPercentile.Value;
                sb.AppendWarning("This section cannot be rendered because the sample size is insufficient for reporting the selected percentiles in accordance with the privacy guidelines." +
                    $" For the given sample size, only percentile values below p{upper:#0.##} can be reported.");
            } else {
                var hiddenProperties = new List<string>();
                if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                    hiddenProperties.Add("ExposureRoute");
                }

                var records = Model.Records
                    .Where(r => r.MissingValueMeasurementsTotal < r.SamplesTotal)
                    .ToList();
                var numSubstances = records.Select(r => r.SubstanceCode).Distinct().Count();
                var numMatrixSamplingTypes = records.Select(r => (r.BiologicalMatrix, r.SamplingType)).Distinct().Count();
                var missingCombinations = Model.Records.Count - records.Count;
                var description = $"Human biomonitoring measurements for {numSubstances} substances measured in {numMatrixSamplingTypes} biological matrix - sampling type combinations.";
                if (missingCombinations > 0) {
                    description += $" No measurements available for {missingCombinations} combinations of matrix - sampling type and substance.";
                }
                sb.AppendDescriptionParagraph(description);
                var panelBuilder = new HtmlTabPanelBuilder();
                var samplingMethods = Model.HbmPercentilesRecords.Keys.ToList();
                foreach (var samplingMethod in samplingMethods) {
                    var matrixShortName = samplingMethod.Name;
                    var percentileAllDataSection = DataSectionHelper.CreateCsvDataSection(
                        $"BoxPlotFullPercentiles{matrixShortName}",
                        Model,
                        Model.HbmPercentilesAllRecords[samplingMethod],
                        ViewBag
                    );
                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        $"BoxplotPercentiles{matrixShortName}",
                        Model,
                        Model.HbmPercentilesRecords[samplingMethod],
                        ViewBag
                    );

                    var matrixSamplingTypeName = $"{samplingMethod.BiologicalMatrix.GetDisplayName()} - {samplingMethod.SampleTypeCode?.ToLower()}";
                    var filenameInsert = matrixSamplingTypeName;
                    var numberOfRecords = Model.HbmPercentilesRecords[samplingMethod].Count;

                    var sbMatrix = new StringBuilder();
                    sbMatrix.Append("<div class=\"figure-container\">");
                    var chartCreatorAll = new HbmAllDataBoxPlotChartCreator(Model, samplingMethod, Model.ShowOutliers);
                    sbMatrix.AppendChart(
                            $"HBMSampleConcentrationsAllBoxPlotChart{matrixShortName}",
                            chartCreatorAll,
                            ChartFileType.Svg,
                            Model,
                            ViewBag,
                            chartCreatorAll.Title,
                            saveChartFile: true,
                            chartData: percentileAllDataSection
                        );
                    var chartCreator = new HbmDataBoxPlotChartCreator(Model, samplingMethod, Model.ShowOutliers);
                    sbMatrix.AppendChart(
                            $"HBMSampleConcentrationsBoxPlotChart{matrixShortName}",
                            chartCreator,
                            ChartFileType.Svg,
                            Model,
                            ViewBag,
                            chartCreator.Title,
                            saveChartFile: true,
                            chartData: percentileDataSection
                        );
                    sbMatrix.Append("</div>");

                    panelBuilder.AddPanel(
                        id: $"Panel_{matrixSamplingTypeName}",
                        title: $"{matrixSamplingTypeName} ({numberOfRecords})",
                        hoverText: matrixSamplingTypeName,
                        content: new HtmlString(sbMatrix.ToString())
                        );
                }
                panelBuilder.RenderPanel(sb);

                var caption = "Human biomonitoring samples per sampling method and substance.";
                if (Model.HasLodLoqRange) {
                    caption += " A range for the LOD or LOQ indicates both the minumum and maximum values.";
                }
                sb.AppendTable(
                    Model,
                    records,
                    "HumanMonitoringSamplesPerSamplingMethodSubstanceTable",
                    ViewBag,
                    caption: caption,
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );

                if (Model.OutlierRecords.Any()) {
                    sb.Append("<p>Press the download link to download the outlier info per substance and biological matrix." +
                        "Outliers are outside the range (Q1 - 3 * IQR , Q3 + 3 * IQR). </p>");
                    sb.Append(TableHelpers.CsvExportLink("HbmIndividualOutlierConcentrationTable", Model, Model.OutlierRecords, ViewBag, true, true));
                } else {
                    sb.Append("<p>* No outliers detected. *</p>");
                }
            }
        }
    }
}
