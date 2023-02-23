using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmVsModelledIndividualDayConcentrationsBySubstanceSectionView : SectionView<HbmVsModelledIndividualDayConcentrationsBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.HbmBoxPlotRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            if (Model.HbmBoxPlotRecords.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add("ExposureRoute");
            }

            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                "HbmVsModelledConcentrationsBySubstancePercentiles",
                Model,
                Model.HbmBoxPlotRecords,
                ViewBag
            );

            var boxPlotChartCreator = new HbmVsModelledIndividualDayConcentrationBySubstanceBoxPlotChartCreator(Model, ViewBag.GetUnit("MonitoringConcentrationUnit"));
            sb.AppendChart(
                "HbmVsModelledConcentrationsBySubstanceBoxPlot",
                boxPlotChartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption: boxPlotChartCreator.Title,
                saveChartFile: true,
                chartData: percentileDataSection
            );

            sb.AppendTable(
                Model,
                Model.HbmBoxPlotRecords,
                "HbmVsModelledConcentrationsBySubstance",
                ViewBag,
                caption: "Cumulative monitoring and modelled day concentrations.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
