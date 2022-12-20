using MCRA.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Html;
using System.Reflection;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders {

    /// <summary>
    /// Class that builds HTML tables based on a generic list of records.
    /// </summary>
    public class HtmlDescriptionTableBuilder<TRecord> : HtmlGenericElementBuilder<TRecord> {

        /// <summary>
        /// The caption of the table.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// If true, then the table header is shown. Otherwise, it is not shown.
        /// </summary>
        public bool ShowHeader { get; set; }

        /// <summary>
        /// If true, an additional div is added before the table with a legend
        /// of the header fields (i.e., header name + description).
        /// </summary>
        public bool ShowLegend { get; set; }

        /// <summary>
        /// Initializes a new <see cref="HtmlDescriptionTableBuilder{TRecord}" /> instance.
        /// </summary>
        public HtmlDescriptionTableBuilder() {
            ShowHeader = true;
        }

        /// <summary>
        /// Builds the html table.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public HtmlString Build(TRecord item) {
            if (item == null) {
                return new HtmlString(string.Empty);
            }
            var sb = new StringBuilder();
            var visibleProperties = typeof(TRecord).GetVisibleProperties(HiddenProperties);
            if (ShowLegend) {
                var legendBuilder = new HtmlLegendBuilder() {
                    ViewBag = ViewBag,
                };
                legendBuilder.Build(sb, visibleProperties);
            }
            buildTable(sb, item, visibleProperties);
            return new HtmlString(sb.ToString());
        }

        protected virtual void buildTable(
            StringBuilder stringBuilder,
            TRecord item,
            IEnumerable<PropertyInfo> visibleProperties
        ) {
            stringBuilder.AppendFormat("<table id=\"{0}\"", Id);
            if (HtmlAttributes != null) {
                foreach (var attribute in HtmlAttributes) {
                    stringBuilder.AppendFormat(" {0}=\"{1}\"", attribute.Key, attribute.Value);
                }
            }
            stringBuilder.Append(">");
            if (!string.IsNullOrEmpty(Caption)) {
                stringBuilder.Append($"<caption>{Caption}</caption>");
            }
            if (ShowHeader) {
                stringBuilder.Append("<thead><tr>");
                stringBuilder.Append("<th>Key</th>");
                stringBuilder.Append("<th>Value</th>");
                stringBuilder.Append("</tr></thead>");
            }
            stringBuilder.Append("<tbody>");
            var formatter = createHeaderFormatter();
            foreach (var property in visibleProperties) {
                var propertyShortName = formatter.Invoke(property);
                stringBuilder.AppendFormat("<tr>");
                stringBuilder.AppendFormat("<td>{0}</td>", propertyShortName);
                stringBuilder.AppendFormat("<td>{0}</td>", item.PrintPropertyValue(property));
                stringBuilder.AppendFormat("</tr>");
            }
            stringBuilder.Append("</tbody>");
            stringBuilder.Append("</table>");
        }
    }
}
