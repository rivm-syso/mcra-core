using System.Text;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CumulativeDayConcentrationsSectionView : SectionView<CumulativeDayConcentrationsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> {
                nameof(BiologicalMatrixConcentrationPercentilesRecord.SubstanceName),
                nameof(BiologicalMatrixConcentrationPercentilesRecord.SubstanceCode)
            };
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add(nameof(BiologicalMatrixConcentrationPercentilesRecord.BiologicalMatrix));
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add(nameof(BiologicalMatrixConcentrationPercentilesRecord.ExposureRoute));
            }
            var unitString = $"Monitoring concentration ({Model.TargetUnit.GetShortDisplayName()}) vs modelled concentration ({ViewBag.GetUnit("ModelledExposureUnit")})";
            var chartCreator = new CumulativeDayConcentrationsBoxPlotChartCreator(Model, unitString);
            sb.AppendChart(
                "CumulativeDayConcentrationsBoxPlot",
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
                "CumulativeDayConcentrationsTable",
                ViewBag,
                caption: "Cumulative monitoring and modelled day concentrations.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
