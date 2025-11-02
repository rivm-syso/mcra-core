using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PbkModelDefinitionSummarySectionView : SectionView<PbkModelDefinitionSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.SbmlModel != null) {
                var diagramCreator = new PbkModelDiagramCreator(Model);
                sb.AppendChart(
                    name: $"PbkModelDiagram_{Model.ModelCode}",
                    section: Model,
                    viewBag: ViewBag,
                    chartCreator: diagramCreator,
                    fileType: ChartFileType.Svg,
                    saveChartFile: true,
                    caption: diagramCreator.Title
                );
            }
            if (Model.Records.Count > 0) {
                sb.AppendTable(
                   Model,
                   Model.Records,
                   $"PbkModelCompartmentsTable_{Model.ModelCode}",
                   ViewBag,
                   caption: $"PBK model compartments {Model.ModelCode}.",
                   saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph($"PBK model {Model.ModelCode} does not have compartments.");
            }
        }
    }
}