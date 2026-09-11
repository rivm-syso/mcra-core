using System.Text;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmVsModelledIndividualDayConcentrationsBySubstanceSectionView : SectionView<HbmVsModelledIndividualDayConcentrationsBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add(nameof(BiologicalMatrixConcentrationPercentilesRecord.BiologicalMatrix));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add(nameof(BiologicalMatrixConcentrationPercentilesRecord.ExposureRoute));
            }

            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                "HbmVsModelledConcentrationsBySubstancePercentiles",
                Model,
                Model.Records,
                ViewBag
            );
            var unitString = $"Monitoring concentration ({Model.TargetUnit.GetShortDisplayName()}) vs modelled concentration ({ViewBag.GetUnit("ModelledExposureUnit")})";
            var boxPlotChartCreator = new HbmVsModelledIndividualDayConcentrationBySubstanceBoxPlotChartCreator(Model, unitString);
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
                Model.Records,
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
