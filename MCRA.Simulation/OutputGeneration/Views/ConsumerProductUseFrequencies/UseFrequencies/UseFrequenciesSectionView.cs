using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UseFrequenciesSectionView : SectionView<UseFrequenciesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (Model.Records.Any()) {
                var panelBuilder = new HtmlTabPanelBuilder();
                var groups = Model
                    .Records
                    .GroupBy(c => c.SurveyCode)
                    .ToList();
                foreach (var group in groups) {
                    var panelSb = new StringBuilder();
                    panelSb.AppendTable(
                        Model,
                        group.ToList(),
                        $"ConsumerProductsUseFrequencies_{group.Key}Table",
                        ViewBag,
                        caption: $"Use frequency statistics for consumer products ({group.Key}).",
                        saveCsv: true,
                        header: true,
                        sortable: true,
                        displayLimit: 20
                    );
                    panelBuilder.AddPanel(
                        id: $"Panel_{group.Key}",
                        title: group.Key,
                        hoverText: group.Key,
                        content: new HtmlString(panelSb.ToString())
                    );
                }
                panelBuilder.RenderPanel(sb, collapseSingleTab: true);
            }
        }
    }
}
