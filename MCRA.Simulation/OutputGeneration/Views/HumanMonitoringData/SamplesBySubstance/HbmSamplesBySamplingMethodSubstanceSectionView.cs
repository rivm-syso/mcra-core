using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmSamplesBySamplingMethodSubstanceSectionView : SectionView<HbmSamplesBySamplingMethodSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add("ExposureRoute");
            }
            if (Model.Records.All(r => r.NonDetects == 0)) {
                hiddenProperties.Add("NonDetects");
            }
            if (Model.Records.All(r => r.NonQuantifications == 0)) {
                hiddenProperties.Add("NonQuantifications");
            }

            var records = Model.Records
                .Where(r => r.MissingValueMeasurements < r.NumberOfSamples)
                .ToList();
            var numSubstances = records.Select(r => r.SubstanceCode).Distinct().Count();
            var numMatrices = records.Select(r => r.BiologicalMatrix).Distinct().Count();
            var missingCombinations = Model.Records.Count - records.Count;
            var description = $"Human monitoring measurements for {numSubstances} substances measured in {numMatrices} biological matrices.";
            if (missingCombinations > 0) {
                description += $" No measurements available for {missingCombinations} combinations of matrix and substance.";
            }
            sb.AppendDescriptionParagraph(description);
            var panelBuilder = new HtmlTabPanelBuilder();
            var biologicalMatrices = Model.HbmPercentilesRecords.Keys.ToList();
            foreach (var biologicalMatrix in biologicalMatrices) {
                var matrixShortName = biologicalMatrix.GetShortDisplayName();
                var percentileAllDataSection = DataSectionHelper.CreateCsvDataSection(
                    $"BoxPlotFullPercentiles{matrixShortName}",
                    Model,
                    Model.HbmPercentilesAllRecords[biologicalMatrix],
                    ViewBag
                );
                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    $"BoxplotPercentiles{matrixShortName}",
                    Model,
                    Model.HbmPercentilesRecords[biologicalMatrix],
                    ViewBag
                );

                var matrixName = biologicalMatrix.GetDisplayName();
                var filenameInsert = $"{matrixName}";
                var numberOfRecords = Model.HbmPercentilesRecords[biologicalMatrix].Count;

                var sbMatrix = new StringBuilder();
                sbMatrix.Append("<div class=\"figure-container\">");
                var chartCreatorAll = new HbmAllDataBoxPlotChartCreator(Model, biologicalMatrix);
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
                var chartCreator = new HbmDataBoxPlotChartCreator(Model, biologicalMatrix);
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
                    id: $"Panel_{matrixName}",
                    title: $"{matrixName} ({numberOfRecords})",
                    hoverText: matrixName,
                    content: new HtmlString(sbMatrix.ToString())
                    );
            }
            panelBuilder.RenderPanel(sb);

            sb.AppendTable(
                Model,
                records,
                "HumanMonitoringSamplesPerSamplingMethodSubstanceTable",
                ViewBag,
                caption: "Human monitoring samples per sampling method and substance.",
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
