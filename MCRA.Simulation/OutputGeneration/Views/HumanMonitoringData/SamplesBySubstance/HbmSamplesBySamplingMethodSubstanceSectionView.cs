using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            //if (Model.Records.All(r => r.CensoredValuesMeasurements == 0)) {
            hiddenProperties.Add("CensoredValuesMeasurements");
            //}
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
                Model.Records,
                "HumanMonitoringSamplesPerSamplingMethodSubstanceTable",
                ViewBag,
                caption: "Human monitoring samples per sampling method and substance.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
