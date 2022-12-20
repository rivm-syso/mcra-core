using Microsoft.AspNetCore.Html;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders {

    /// <summary>
    /// Utility methods for rendering html panels.
    /// </summary>
    public class HtmlTabPanelBuilder {

        internal class TabPanelItem {
            public string Id { get; set; }
            public string Title { get; set; }
            public string HoverText { get; set; }
            public HtmlString Content { get; set; }
        }

        private List<TabPanelItem> _tabPanelItems { get; set; } = new List<TabPanelItem>();

        public void AddPanel(string id, string title, string hoverText, HtmlString content) {
            var item = new TabPanelItem() {
                Id = id,
                Title = title,
                HoverText = hoverText,
                Content = content
            };
            _tabPanelItems.Add(item);
        }

        public void RenderPanel(StringBuilder sb) {
            renderPanel(sb, _tabPanelItems);
        }

        private static void renderPanel(StringBuilder sb, List<TabPanelItem> panelItems) {
            sb.Append($"<div class='tab-panel'>");
            sb.Append("<ul class='tab-panel-header'>");
            foreach (var item in panelItems) {
                appendTabPaneHeader(sb, item.Id, item.Title, item.HoverText);
            }
            sb.Append("</ul>");
            sb.Append("<div class='tab-panel-content'>");
            foreach (var item in panelItems) {
                appendTabContent(sb, item.Id, item.Content);
            }
            sb.Append("</div>");
            sb.Append("</div>");
        }

        private static void appendTabPaneHeader(StringBuilder sb, string id, string title, string hoverText) {
            sb.Append($@"<li><a data-toggle='tab' href='#{id}' title='{hoverText}'>{title}</a></li>");
        }

        private static void appendTabContent(StringBuilder sb, string id, HtmlString content) {
            sb.Append($@"<div id='{id}' class='tab-pane'>{content}</div>");
        }
    }
}
