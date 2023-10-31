using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.Charting;
using MCRA.Utils.ExtensionMethods;

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
                var matrix = biologicalMatrix.GetShortDisplayName();
                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    $"BoxplotPercentiles{matrix}",
                    Model,
                    Model.HbmPercentilesRecords[biologicalMatrix],
                    ViewBag,
                    true,
                    new List<string>()
                );
                var percentileFullDataSection = DataSectionHelper.CreateCsvDataSection(
                    $"BoxPlotFullPercentiles{matrix}",
                    Model,
                    Model.HbmPercentilesAllRecords[biologicalMatrix],
                    ViewBag,
                    true,
                    new List<string>()
                );

                var unitKey = biologicalMatrix.GetDisplayName();
                var filenameInsert = $"{unitKey}";
                var numberOfRecords = Model.HbmPercentilesRecords[biologicalMatrix].Count;
                var chartCreator = new HbmDataBoxPlotChartCreator(Model, biologicalMatrix);
                var chartCreatorFull = new HbmFullDataBoxPlotChartCreator(Model, biologicalMatrix);
                var figCaption = $"{unitKey} individual concentrations by substance. " + chartCreator.Title;
                panelBuilder.AddPanel(
                    id: $"Panel_{unitKey}",
                    title: $"{unitKey} ({numberOfRecords})",
                    hoverText: unitKey,
                    content: ChartHelpers.Chart(
                        name: $"HBMSampleConcentrationsBoxPlotChart{matrix}",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: figCaption,
                        chartData: percentileDataSection
                    ),
                    additionalContent: ChartHelpers.Chart(
                        name: $"HBMSampleConcentrationsFullBoxPlotChart{matrix}",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreatorFull,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: figCaption,
                        chartData: percentileFullDataSection
                    )
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
