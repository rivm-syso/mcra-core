using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class InternalVersusExternalExposuresSectionView : SectionView<InternalVersusExternalExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var panelBuilder = new HtmlTabPanelBuilder();
            foreach (var item in Model.TargetUnits) {
                var chartCreator = new InternalVersusExternalExposuresScatterChartCreator(
                    Model,
                    item
                );
                panelBuilder.AddPanel(
                    id: item.Target.Code,
                    title: item.GetShortDisplayName(),
                    hoverText: item.GetShortDisplayName(),
                    content: ChartHelpers.Chart(
                        name: $"InternalVersusExternalExposuresScatterChart_{item.Target.Code}",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: chartCreator.Title
                    )
                );
            }
            panelBuilder.RenderPanel(sb);
        }
    }
}
