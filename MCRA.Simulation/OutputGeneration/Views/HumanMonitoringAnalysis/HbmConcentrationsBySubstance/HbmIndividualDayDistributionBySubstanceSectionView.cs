using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualDayDistributionBySubstanceSectionView : SectionView<HbmIndividualDayDistributionBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
    
            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                "HbmIndividualDayDistributionBySubstancePercentiles", Model, Model.HbmBoxPlotRecords,
                ViewBag, true, new List<string>()
            );
            var chartCreator = new HbmDayConcentrationsBySubstanceBoxPlotChartCreator(Model, ViewBag.GetUnit("MonitoringConcentrationUnit"));
            sb.AppendChart(
                "HBMIndividualDayConcentrationBySubstanceBoxPlotChart",
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
                "HbmConcentrationsBySubstanceTable",
                ViewBag,
                caption: "Human monitoring day concentrations by substance.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
