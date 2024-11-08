using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Html;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Helpers {

    /// <summary>
    /// utility methods to automatically generate HTML tables from a collection of records.
    /// </summary>
    public static class TableHelpers {

        /// <summary>
        /// Renders a html table based on the list of records.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="name"></param>
        /// <param name="section"></param>
        /// <param name="items"></param>
        /// <param name="viewBag"></param>
        /// <param name="saveCsv"></param>
        /// <param name="attributes"></param>
        /// <param name="header"></param>
        /// <param name="hiddenProperties"></param>
        /// <param name="displayLimit"></param>
        /// <param name="caption"></param>
        /// <param name="rotate"></param>
        /// <returns></returns>
        public static HtmlString Table<TRecord>(
            string name,
            SummarySection section,
            IList<TRecord> items,
            ViewParameters viewBag,
            bool saveCsv,
            IDictionary<string, object> attributes = null,
            bool header = true,
            IList<string> hiddenProperties = null,
            int displayLimit = -1,
            string caption = null,
            bool rotate = false,
            string sectionLabel = null
        ) {
            var tableBuilder = new HtmlTableBuilder<TRecord>() {
                Id = StringExtensions.CreateFingerprint(section.SectionId + name),
                Section = section,
                TableName = name,
                TempPath = saveCsv ? viewBag.TempPath : null,
                ShowHeader = header,
                ShowLegend = true,
                HtmlAttributes = attributes,
                HiddenProperties = hiddenProperties,
                ViewBag = viewBag,
                Caption = caption,
                RotateTable = rotate,
                SectionLabel = sectionLabel

            };
            return tableBuilder.Build(items, displayLimit);
        }


        /// <summary>
        /// Renders a html table based on the list of records.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="name"></param>
        /// <param name="section"></param>
        /// <param name="items"></param>
        /// <param name="viewBag"></param>
        /// <param name="saveCsv"></param>
        /// <param name="identifierPropertyName"></param>
        /// <param name="parentPropertyName"></param>
        /// <param name="isDataNodePropertyName"></param>
        /// <param name="attributes"></param>
        /// <param name="hiddenProperties"></param>
        /// <returns></returns>
        public static HtmlString HierarchicalTable<TRecord>(
            string name,
            SummarySection section,
            IList<TRecord> items,
            ViewParameters viewBag,
            bool saveCsv,
            string identifierPropertyName,
            string parentPropertyName,
            string isDataNodePropertyName,
            bool header = true,
            IDictionary<string, object> attributes = null,
            IList<string> hiddenProperties = null,
            string caption = null,
            int displayLimit = -1,
            string sectionLabel = null
        ) {
            var identifierProperty = typeof(TRecord).GetPropertySingleOrDefault(identifierPropertyName);
            var parentProperty = typeof(TRecord).GetPropertySingleOrDefault(parentPropertyName);
            var isDataNodeProperty = !string.IsNullOrEmpty(isDataNodePropertyName) ? typeof(TRecord).GetPropertySingleOrDefault(isDataNodePropertyName) : null;
            var tableBuilder = new HtmlHierarchicalTableBuilder<TRecord>() {
                Section = section,
                Id = section.SectionId + name,
                TableName = name,
                ShowHeader = header,
                ShowLegend = true,
                ViewBag = viewBag,
                TempPath = saveCsv ? viewBag.TempPath : null,
                HtmlAttributes = attributes,
                HiddenProperties = hiddenProperties,
                IdentifierProperty = identifierProperty,
                ParentProperty = parentProperty,
                IsDataNodeProperty = isDataNodeProperty,
                Caption = caption,
            };
            return tableBuilder.Build(items, displayLimit);
        }

        /// <summary>
        /// Renders a description table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="sectionId"></param>
        /// <param name="record"></param>
        /// <param name="attributes"></param>
        /// <param name="hiddenProperties"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static HtmlString DescriptionTable<T>(
            string name,
            string sectionId,
            T record,
            ViewParameters viewBag,
            IDictionary<string, object> attributes = null,
            List<string> hiddenProperties = null,
            bool header = true,
            bool showLegend = true,
            string caption = null
        ) {
            var descriptionTableBuilder = new HtmlDescriptionTableBuilder<T>() {
                Id = StringExtensions.CreateFingerprint(sectionId + name),
                ShowHeader = header,
                ShowLegend = showLegend,
                Caption = caption,
                HtmlAttributes = attributes,
                HiddenProperties = hiddenProperties,
                ViewBag = viewBag,
            };
            return descriptionTableBuilder.Build(record);
        }

        /// <summary>
        /// Renders a html table based on the list of records.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="section"></param>
        /// <param name="items"></param>
        /// <param name="viewBag"></param>
        /// <param name="saveCsv"></param>
        /// <param name="header"></param>
        /// <param name="hiddenProperties"></param>
        /// <returns></returns>
        public static HtmlString CsvExportLink<TRecord>(
            string name,
            SummarySection section,
            IEnumerable<TRecord> items,
            ViewParameters viewBag,
            bool saveCsv,
            bool header = true,
            List<string> hiddenProperties = null
        ) {
            var tableBuilder = new HtmlCsvExportLinkBuilder<TRecord>() {
                Section = section,
                Id = StringExtensions.CreateFingerprint(section.SectionId + name),
                TableName = name,
                ShowHeader = header,
                HiddenProperties = hiddenProperties,
                ViewBag = viewBag,
                TempPath = saveCsv ? viewBag.TempPath : null,
            };
            return tableBuilder.Build(items);
        }

        /// <summary>
        /// Renders a download link for a data section
        /// </summary>
        /// <param name="dataSection"></param>
        /// <returns></returns>
        public static HtmlString CsvExportLink(
            CsvDataSummarySection dataSection
        ) {
            var sb = new StringBuilder();
            //Create a toolbar in HTML with a download link for this CSV section
            sb.Append("<div class='toolbar'>");
            //register in a data section
            sb.Append($"<a class='button icon_csv' data-csv-id='{dataSection.SectionGuid:N}' data-csv-name='{dataSection.CsvFileName}'>");
            sb.Append("<span>");
            sb.Append("</span>");
            sb.Append("</a>");
            sb.Append("</div>");
            return new HtmlString(sb.ToString());
        }

        /// <summary>
        /// Builds a custom table legend for the table.
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static HtmlString BuildCustomTableLegend(
            IList<string> columns,
            IList<string> description
        ) {
            var sb = new StringBuilder();
            sb.Append("<div class=\"table_info\">");
            sb.Append("<table><thead>");
            sb.AppendHeaderRow("Field", "Description");
            sb.Append("</thead><tbody>");
            for (int i = 0; i < columns.Count; i++) {
                sb.AppendTableRow(columns[i], description[i]);
            }
            sb.Append("</tbody></table>");
            sb.Append("</div>");
            return new HtmlString(sb.ToString());
        }

        #region StringBuilderExtensions

        public static StringBuilder AppendTable<TRecord>(
            this StringBuilder sb,
            SummarySection section,
            IList<TRecord> items,
            string name,
            ViewParameters viewBag,
            string identifierPropertyName = null,
            string parentPropertyName = null,
            string isDataNodePropertyName = null,
            bool header = true,
            string caption = null,
            bool saveCsv = false,
            int displayLimit = -1,
            bool sortable = true,
            IList<string> hiddenProperties = null,
            IList<string> tableClasses = null,
            bool rotate = false,
            bool isHierarchical = false,
            string sectionLabel = null
        ) {
            if (!rotate) {
                if (sortable) {
                    tableClasses = tableClasses ?? new List<string>();
                    tableClasses.Add("sortable");
                }
                if (isHierarchical) {
                    tableClasses = tableClasses ?? new List<string>();
                    tableClasses.Add("hierarchical");

                }
            }
            Dictionary<string, object> attributes = null;
            if (tableClasses?.Count > 0) {
                attributes = attributes ?? new Dictionary<string, object>();
                attributes["class"] = string.Join(" ", tableClasses);
            }
            if (isHierarchical) {
                return sb.Append(HierarchicalTable(
                    name,
                    section,
                    items,
                    viewBag,
                    saveCsv,
                    identifierPropertyName,
                    parentPropertyName,
                    isDataNodePropertyName,
                    header,
                    attributes,
                    hiddenProperties,
                    caption,
                    displayLimit,
                    sectionLabel
                  ));
            } else {
                return sb.Append(Table(
                    name,
                    section,
                    items,
                    viewBag,
                    saveCsv,
                    attributes,
                    header,
                    hiddenProperties,
                    displayLimit,
                    caption,
                    rotate,
                    sectionLabel
                    )
                );
            }
        }

        public static void AppendDescriptionTable<T>(
            this StringBuilder sb,
            string name,
            string sectionId,
            T record,
            ViewParameters viewBag,
            string caption = null,
            bool header = true,
            bool showLegend = true,
            List<string> hiddenProperties = null,
            IDictionary<string, object> attributes = null) {
            sb.Append(DescriptionTable(
                name: name,
                sectionId: sectionId,
                record: record,
                viewBag: viewBag,
                attributes: attributes,
                hiddenProperties: hiddenProperties,
                header: header,
                showLegend: showLegend,
                caption: caption
            ));
        }

        public static StringBuilder AppendCsvDataTableElement(
            this StringBuilder sb,
            string name,
            string caption,
            Guid sectionGuid,
            int displayLimit,
            string columnOrder = null
        ) {
            var id = StringExtensions.CreateFingerprint($"{sectionGuid:N}name");
            sb.Append($"<table id=\"{id}\"");
            sb.Append($" class=\"sortable csv-data-table\"");
            sb.Append($" csv-download-id=\"{sectionGuid:N}\"");
            sb.Append($" csv-download-name=\"{name}\"");
            if (!string.IsNullOrEmpty(caption)) {
                sb.Append($" csv-table-caption=\"{caption}\"");
            }
            if (displayLimit > 0) {
                sb.Append($" csv-max-records=\"{displayLimit}\"");
            }
            if (!string.IsNullOrEmpty(columnOrder)) {
                sb.Append($" csv-column-order=\"{columnOrder}\"");
            }
            sb.Append("></table>");
            return sb;
        }

        #endregion
    }
}
