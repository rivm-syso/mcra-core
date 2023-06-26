using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CumulativeIndividualConcentrationsSectionView : SectionView<CumulativeIndividualConcentrationsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> {
                "SubstanceName",
                "SubstanceCode"
            };
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add("ExposureRoute");
            }

            var chartCreator = new CumulativeIndividualConcentrationsBoxPlotChartCreator(Model, ViewBag.GetUnit("MonitoringConcentrationUnit"));
            sb.AppendChart(
                "CumulativeIndividualConcentrationsBoxPlot",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption: chartCreator.Title,
                saveChartFile: true
            );

            sb.AppendTable(
                Model,
                Model.Records,
                "CumulativeIndividualConcentrationsdTable",
                ViewBag,
                caption: "Cumulative monitoring and modelled individual concentrations.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
