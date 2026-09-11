using System.Text;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CumulativeIndividualConcentrationsSectionView : SectionView<CumulativeIndividualConcentrationsSection> {
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
            var chartCreator = new CumulativeIndividualConcentrationsBoxPlotChartCreator(Model, unitString);
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
