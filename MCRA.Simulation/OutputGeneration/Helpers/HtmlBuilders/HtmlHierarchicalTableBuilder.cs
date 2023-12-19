using MCRA.Utils.Csv;
using MCRA.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Html;
using System.Reflection;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders {

    /// <summary>
    /// This class builds hierarchical HTML tables (tree-tables) based on a generic list of records.
    /// </summary>
    public class HtmlHierarchicalTableBuilder<TRecord> : HtmlTableBuilder<TRecord> {

        /// <summary>
        /// The property that points to the parent of each record.
        /// </summary>
        public PropertyInfo ParentProperty { get; set; }

        /// <summary>
        /// The property (a boolean property) that tells us whether the record
        /// is a data-node or a tree-node without data.
        /// </summary>
        public PropertyInfo IsDataNodeProperty { get; set; }

        /// <summary>
        /// Builds the html table.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="items"></param>
        /// <param name="displayLimit"></param>
        /// <returns></returns>
        public override HtmlString Build(
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
                    Caption,
                    new TreeTableProperties {
                        IdFieldName = IdentifierProperty.Name,
                        ParentFieldName = ParentProperty.Name,
                        IsDataNodeFieldName = IsDataNodeProperty.Name
                    }
                );
                Section.DataSections.Add(dataSection);
                sectionGuid = dataSection.SectionGuid;
                //Write the CSV to the temp file
                var csvWriter = new CsvWriter();
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

        /// <summary>
        /// Override to also include hierarchical data in each table row.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="stringBuilder"></param>
        /// <param name="record"></param>
        /// <param name="visibleProperties"></param>
        protected override void buildTableRow(StringBuilder stringBuilder, TRecord record, IEnumerable<PropertyInfo> visibleProperties) {
            stringBuilder.Append("<tr");
            if (IdentifierProperty != null) {
                var identifierValue = record.PrintPropertyValue(IdentifierProperty);
                if (!string.IsNullOrEmpty(identifierValue)) {
                    _ = stringBuilder.Append(value: $" data-tt-id=\"{identifierValue}\"");
                }
                if (ParentProperty != null) {
                    var parentValue = record.PrintPropertyValue(ParentProperty);
                    if (!string.IsNullOrEmpty(parentValue)) {
                        stringBuilder.Append(value: $" data-tt-parent-id=\"{parentValue}\"");
                    }
                }
                if (IsDataNodeProperty != null && (bool)IsDataNodeProperty.GetValue(record, null)) {
                    stringBuilder.Append(" class=\"data-row\"");
                } else {
                    stringBuilder.Append(" class=\"treenode-row\"");
                }
            }
            stringBuilder.Append(">");
            foreach (var property in visibleProperties) {
                stringBuilder.Append("<td>");
                stringBuilder.Append(record.PrintPropertyValue(property));
                stringBuilder.Append("</td>");
            }
            stringBuilder.Append("</tr>");
        }
    }
}
