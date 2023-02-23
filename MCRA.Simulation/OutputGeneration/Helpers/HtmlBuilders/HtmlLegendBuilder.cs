using MCRA.Utils.ExtensionMethods;
using System.Reflection;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders {

    /// <summary>
    /// Class that builds HTML tables based on a generic list of records.
    /// </summary>
    public class HtmlLegendBuilder : HtmlElementBuilder {

        public void Build(StringBuilder sb, IEnumerable<PropertyInfo> visibleProperties) {
            if (visibleProperties.All(p => string.IsNullOrEmpty(p.GetDescription()))) {
                return;
            }
            sb.Append("<div class=\"table_info\">");
            sb.Append("<table>");
            sb.Append("<thead><tr><th>Field</th><th>Description</th></tr></thead>");
            sb.Append("<tbody>");
            var formatter = createHeaderFormatter();
            var descriptionFormatter = createDescriptionFormatter();
            foreach (var property in visibleProperties) {
                var propertyShortName = formatter.Invoke(property);
                var propertyDescription = descriptionFormatter.Invoke(property);
                if (!string.IsNullOrEmpty(propertyDescription)) {
                    sb.AppendTableRow(propertyShortName, propertyDescription);
                }
            }
            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("</div>");
        }
    }
}
