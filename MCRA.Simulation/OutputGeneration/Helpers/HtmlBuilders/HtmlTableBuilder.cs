using MCRA.Utils.Csv;
using MCRA.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Html;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Web;

namespace MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders {

    /// <summary>
    /// Class that builds HTML tables based on a generic list of records.
    /// </summary>
    public class HtmlTableBuilder<TRecord> : HtmlGenericElementBuilder<TRecord> {

        /// <summary>
        /// The section for which to build the table
        /// </summary>
        public SummarySection Section { get; set; }

        /// <summary>
        /// The property that is the identifying property of each record (e.g., the 'id' field).
        /// </summary>
        public PropertyInfo IdentifierProperty { get; set; }

        /// <summary>
        /// If true, then the table header is shown. Otherwise, it is not shown.
        /// </summary>
        public bool ShowHeader { get; set; }

        /// <summary>
        /// If true, then the table is a rotated table with the columns as rows
        /// This shows the header in the first column and the values in subsequent columns
        /// </summary>
        public bool RotateTable { get; set; } = false;

        /// <summary>
        /// If true, an additional div is added before the table with a legend
        /// of the header fields (i.e., header name + description).
        /// </summary>
        public bool ShowLegend { get; set; }

        /// <summary>
        /// The filename of the csv export file.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The sectionLabel of the csv export file.
        /// </summary>
        public string SectionLabel { get; set; }
        /// <summary>
        /// The caption of the table.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Temporary path for saving intermediate CSV files
        /// </summary>
        public string TempPath { get; set; }

        public HtmlTableBuilder() {
            ShowHeader = true;
            ShowLegend = false;
        }

        /// <summary>
        /// Builds the html table.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="items"></param>
        /// <param name="displayLimit"></param>
        /// <returns></returns>
        public virtual HtmlString Build(
            IList<TRecord> items,
            int displayLimit = -1
        ) {
            if (items == null || !items.Any()) {
                return new HtmlString(string.Empty);
            }
            var sb = new StringBuilder();
            var visibleProperties = typeof(TRecord).GetVisibleProperties(HiddenProperties).ToList();
            var sectionGuid = Guid.NewGuid();
            if (!string.IsNullOrEmpty(TempPath)) {
                //Create data section for this file
                var csvTempFile = $"{TableName}-{Id}.csv";
                var dataSection = new CsvDataSummarySection(
                    TableName,
                    Path.Combine(TempPath ?? Path.GetTempPath(), csvTempFile),
                    ViewBag?.TitlePath,
                    visibleProperties,
                    ViewBag?.UnitsDictionary,
                    caption: Caption,
                    sectionLabel: SectionLabel
                );
                Section.DataSections.Add(dataSection);
                sectionGuid = dataSection.SectionGuid;
                //Write the CSV to the temp file
                var csvWriter = CsvOutputWriterFactory.Create();
                csvWriter.WriteToCsvFile(items, dataSection.CsvFileName, ShowHeader, createHeaderFormatter(), visibleProperties);
            }

            if (ShowLegend) {
                var legendBuilder = new HtmlLegendBuilder() {
                    ViewBag = ViewBag,
                };
                legendBuilder.Build(sb, visibleProperties);
            }
            buildTable(sb, sectionGuid, items, visibleProperties, displayLimit);

            return new HtmlString(sb.ToString());
        }

        protected void buildTable(
            StringBuilder sb,
            Guid idSection,
            IList<TRecord> items,
            IEnumerable<PropertyInfo> visibleProperties,
            int displayLimit
        ) {
            sb.AppendFormat("<table id=\"{0}\"", Id);
            //Create a toolbar in HTML with a download link for this CSV section, only
            //when there is a temporary file created
            if (!string.IsNullOrEmpty(TempPath)) {
                sb.Append($" csv-download-id='{idSection:N}'");
                sb.Append($" csv-download-name='{TableName}'");
                if (RotateTable) {
                    sb.Append(" csv-table-rotate='true'");
                }
                if (displayLimit > 0) {
                    sb.Append($" csv-max-records=\"{displayLimit}\"");
                }
                //if there is a class variable in the attributes (class="sortable" for example)
                //append the table class otherwise set it directly
                if (HtmlAttributes != null && HtmlAttributes.TryGetValue("class", out var classValue)) {
                    HtmlAttributes["class"] = $"{classValue} csv-data-table";
                } else {
                    sb.Append($" class=\"csv-data-table\"");
                }
            }
            if (HtmlAttributes != null) {
                foreach (var attribute in HtmlAttributes) {
                    sb.AppendFormat(" {0}=\"{1}\"", attribute.Key, attribute.Value);
                }
            }
            sb.Append(">");

            //Only render complete table if it is not resolved from CSV at run time
            if (string.IsNullOrEmpty(TempPath)) {
                if (!string.IsNullOrEmpty(Caption)) {
                    sb.Append($"<caption>{Caption}</caption>");
                }
                if (ShowHeader && !RotateTable) {
                    buildTableHeader(sb, visibleProperties);
                }
                buildTableBody(sb, idSection, items, visibleProperties);
            }
            sb.Append("</table>");
        }

        protected virtual void buildTableHeader(StringBuilder sb, IEnumerable<PropertyInfo> visibleProperties) {
            sb.Append("<thead><tr>");
            var formatter = createHeaderFormatter();
            var identifierPropName = IdentifierProperty?.Name ?? "";
            foreach (var property in visibleProperties) {
                if (!property.Name.StartsWith("__")) {
                    var propertyShortName = formatter.Invoke(property);
                    sb.AppendFormat("<th>{0}</th>", HttpUtility.HtmlEncode(propertyShortName));
                }
            }
            sb.Append("</tr></thead>");
        }

        protected virtual void buildTableBody(
            StringBuilder sb,
            Guid idSection,
            IList<TRecord> items,
            IEnumerable<PropertyInfo> visibleProperties
        ) {
            sb.AppendFormat("<tbody>");
            //Create the table directly in HTML
            if (RotateTable) {
                buildRotatedTableBody(sb, items, visibleProperties);
            } else {
                foreach (var item in items) {
                    buildTableRow(sb, item, visibleProperties);
                }
            }
            sb.Append("</tbody>");
        }

        protected virtual void buildTableRow(StringBuilder sb, TRecord record, IEnumerable<PropertyInfo> visibleProperties) {
            sb.Append("<tr");
            if (IdentifierProperty != null) {
                var identifierValue = record.PrintPropertyValue(IdentifierProperty);
                if (!string.IsNullOrEmpty(identifierValue)) {
                    sb.Append($" data-tt-id=\"{HttpUtility.HtmlEncode(identifierValue)}\"");
                }
            }
            sb.Append(">");
            foreach (var property in visibleProperties) {
                //check for format attribute containing raw HTML specifier
                var formatAttr = property.GetAttribute<DisplayFormatAttribute>(false);
                var isRaw = formatAttr?.DataFormatString == "RawHTML";
                sb.Append("<td>");
                if (isRaw) {
                    sb.Append(property.GetValue(record));
                } else {
                    sb.Append(HttpUtility.HtmlEncode(record.PrintPropertyValue(property)));
                }
                sb.Append("</td>");
            }
            sb.Append("</tr>");
        }

        protected virtual void buildRotatedTableBody(
            StringBuilder sb,
            IList<TRecord> items,
            IEnumerable<PropertyInfo> visibleProperties
        ) {
            //get the properties and construct row builders as separate stringbuilders
            //omit the 'hidden' columns starting with two underscores
            var rowBuilders = visibleProperties
                .Where(p => !p.Name.StartsWith("__"))
                .Select(p => (p, new StringBuilder())).ToList();

            //build the header column first
            var formatter = createHeaderFormatter();
            foreach ((var property, var builder) in rowBuilders) {
                var propertyShortName = formatter.Invoke(property);
                builder.Append($"<td>{HttpUtility.HtmlEncode(propertyShortName)}</td>");
            }
            //go through the records and append a column for each property
            foreach (var record in items) {
                foreach ((var property, var builder) in rowBuilders) {
                    builder.Append("<td>");
                    builder.Append(HttpUtility.HtmlEncode(record.PrintPropertyValue(property)));
                    builder.Append("</td>");
                }
            }
            //build the rows
            foreach ((_, var builder) in rowBuilders) {
                sb.Append($"<tr>{builder}</tr>");
            }
        }
    }
}
