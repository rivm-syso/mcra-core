using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmCumulativeIndividualDayDistributionsSectionView : SectionView<HbmCumulativeIndividualDayDistributionsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            hiddenProperties.Add("SubstanceCode");
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                "CumulativeDayConcentrationsPercentiles", Model, Model.HbmBoxPlotRecords,
                ViewBag, true, new List<string>()
            );
            var chartCreator = new HbmCumulativeIndividualDayDistributionsBoxPlotChartCreator(Model, ViewBag.GetUnit("MonitoringConcentrationUnit"));
            sb.AppendChart(
                "CumulativeDayConcentrationsBoxPlotChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption: chartCreator.Title,
                saveChartFile: true,
                chartData: percentileDataSection
            );

            //Render HTML
            sb.AppendTable(
                Model,
                Model.Records,
                "CumulativeDayConcentrationsTable",
                ViewBag,
                caption: "Human monitoring individual day measurement distribution endpoint cumulative substance.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
