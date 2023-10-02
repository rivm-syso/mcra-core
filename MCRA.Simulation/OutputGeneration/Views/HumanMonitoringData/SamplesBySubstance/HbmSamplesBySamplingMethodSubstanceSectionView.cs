using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

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

            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                "BoxplotPercentiles", Model, Model.HbmPercentilesRecords,
                ViewBag, true, new List<string>()
            );
            var percentileFullDataSection = DataSectionHelper.CreateCsvDataSection(
                "BoxPlotFullPercentiles", Model, Model.HbmPercentilesAllRecords,
                ViewBag, true, new List<string>()
            );

            sb.Append("<div class=\"figure-container\">");
            var chartCreator = new HbmDataBoxPlotChartCreator(Model, ViewBag.GetUnit("HbmConcentrationUnit"));
            sb.AppendChart(
                "HBMSampleConcentrationsBoxPlotChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption: chartCreator.Title,
                saveChartFile: true,
                chartData: percentileDataSection
            );

            var chartCreator1 = new HbmFullDataBoxPlotChartCreator(Model, ViewBag.GetUnit("HbmConcentrationUnit"));
            sb.AppendChart(
                "HBMSampleConcentrationsFullBoxPlotChart",
                chartCreator1,
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption: chartCreator1.Title,
                saveChartFile: true,
                chartData: percentileFullDataSection
            );
            sb.Append("</div>");
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
                sb.Append("<p>Press the download link to download the outlier info per substance and biological matrix."+
                    "Outliers are outside the range (Q1 - 3 * IQR , Q3 + 3 * IQR). </p>");
                sb.Append(TableHelpers.CsvExportLink("HbmIndividualOutlierConcentrationTable", Model, Model.OutlierRecords, ViewBag, true, true));
            }
        }
    }
}
