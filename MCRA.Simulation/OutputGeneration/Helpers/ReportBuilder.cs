using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;

namespace MCRA.Simulation.OutputGeneration.Helpers {

    /// <summary>
    /// Methods to automatically generate the HTML for summarizing a section-based report.
    /// </summary>
    public class ReportBuilder {

        SummaryToc _summaryToc;

        public ReportBuilder(SummaryToc summaryToc) {
            _summaryToc = summaryToc;
        }

        /// <summary>
        /// Renders the given section as html.
        /// </summary>
        /// <param name="outputInfo"></param>
        /// <param name="resolveChartsAndTables">
        /// If true, then dummy table and chart sections are replaced with the specific table/chart contents.
        /// </param>
        /// <param name="tempPath">
        /// Should be specified when replacing table/chart contents to store temp csv/image files.
        /// </param>
        /// <param name="inlineCharts">Render the images inline as base64 PNG img and embedded svg tags</param>
        /// <returns></returns>
        public string RenderReport(
            OutputInfo outputInfo,
            bool resolveChartsAndTables,
            string tempPath,
            bool inlineCharts = false
        ) {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html>");
            sb.Append($"<html>");
            sb.Append("<head>");
            sb.Append("<meta charset=\"UTF-8\" />");
            sb.Append($"<style>{loadCss()}</style>");
            sb.Append("</head>");
            sb.Append($"<body>");
            if (outputInfo != null) {
                sb.Append(renderOutputInfo(outputInfo));
            }
            if (_summaryToc?.SubSectionHeaders?.Any() ?? false) {
                foreach (var hdr in _summaryToc.SubSectionHeaders.OrderBy(h => h.Order)) {
                    //render Section HTML from builder
                    var html = RenderSection(hdr);
                    sb.Append(html);
                }
            }
            sb.Append("</body>");
            sb.Append("</html>");

            var reportHtml = sb.ToString();
            if (resolveChartsAndTables && !string.IsNullOrEmpty(tempPath)) {
                reportHtml = ResolveChartsAndTables(reportHtml, tempPath, inlineCharts);
            }
            return reportHtml;
        }

        /// <summary>
        /// Generates a html report of the specified sub-section.
        /// </summary>
        /// <param name="sectionHeader"></param>
        /// <param name="outputInfo"></param>
        /// <param name="resolveChartsAndTables">
        /// If true, then dummy table and chart sections are replaced with the specific table/chart contents.
        /// </param>
        /// <param name="tempPath">
        /// Should be specified when replacing table/chart contents to store temp csv/image files.
        /// </param>
        /// <param name="inlineCharts">Render the images inline as base64 PNG img and embedded svg tags</param>
        /// <returns></returns>
        public string RenderPartialReport(
            SectionHeader sectionHeader,
            OutputInfo outputInfo,
            bool resolveChartsAndTables,
            string tempPath,
            bool inlineCharts = false
        ) {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html>");
            sb.Append($"<html>");
            sb.Append("<head>");
            sb.Append($"<style>{loadCss()}</style>");
            sb.Append("</head>");
            sb.Append($"<body>");
            if (outputInfo != null) {
                sb.Append(renderOutputInfo(outputInfo, sectionHeader));
            }
            var html = RenderSection(sectionHeader);
            if (resolveChartsAndTables && !string.IsNullOrEmpty(tempPath)) {
                html = ResolveChartsAndTables(html, tempPath, inlineCharts);
            }
            sb.Append(html);
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }

        /// <summary>
        /// Generates html of the specified section headers.
        /// </summary>
        /// <param name="sectionHeaders"></param>
        /// <returns></returns>
        public string RenderSections(IEnumerable<SectionHeader> sectionHeaders) {
            var sb = new StringBuilder();
            foreach (var header in sectionHeaders) {
                renderSectionRecursive(sb, header, 1);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Renders the html of the provided section header.
        /// </summary>
        /// <param name="sectionHeader"></param>
        /// <param name="skipTopHeader">If true, the first section header will not be rendered.</param>
        /// <returns></returns>
        public string RenderSection(SectionHeader sectionHeader, bool skipTopHeader = false, int htmlHeaderLevel = 0) {
            var headerLevel = htmlHeaderLevel == 0 ? (sectionHeader?.Depth + 1 ?? 1) : htmlHeaderLevel;
            var sb = new StringBuilder();
            sb.Append($"<div class='section' data-section-id='{sectionHeader.SectionId}'>");
            if (sectionHeader.HasSectionData && !skipTopHeader) {
                renderSectionHeader(sb, sectionHeader.Name, headerLevel, sectionHeader.SectionHash);
            }
            renderSectionContent(sb, sectionHeader);
            foreach (var subSectionInfo in sectionHeader.SubSectionHeaders.OrderBy(h => h.Order)) {
                renderSectionRecursive(sb, subSectionInfo, headerLevel + 1);
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the html of the section contents of the specified section header.
        /// </summary>
        /// <param name="sectionHeader"></param>
        /// <returns></returns>
        public string RenderSectionContent(SectionHeader sectionHeader) {
            var sb = new StringBuilder();
            renderSectionContent(sb, sectionHeader);
            return sb.ToString();
        }

        /// <summary>
        /// Resolve (standard) action settings stubs in the templateHtml parameter
        /// create a table with the setting and optionally fill the values in dedicated
        /// span sections.
        /// </summary>
        /// <param name="templateHtml">The template html to process.</param>
        /// <param name="settings">Dictionary of standard action settings indexed by name.</param>
        /// <param name="headerTitle">Title for the header, may be omitted.</param>
        /// <returns>Pre-processed template with embedded standard action settings.</returns>
        public static string ResolveSettingsStubs(
            string templateHtml,
            IDictionary<string, string> settings,
            string headerTitle = null
        ) {
            var xmlDoc = new XmlDocument();
            //load the output template as XHTML
            xmlDoc.LoadXml(templateHtml);
            //try to find any div nodes with the 'settings-outlet' class
            //fill these with the settings content
            var nodes = xmlDoc.SelectNodes("//div[@class='settings-outlet']");
            //only place section if any section placeholder is found
            foreach (XmlElement n in nodes) {
                var sb = new StringBuilder();
                //when headertitle is given and header level > 0: add a header with the provided title
                if (!string.IsNullOrEmpty(headerTitle) &&
                    int.TryParse(n.GetAttribute("header-level"), out int level) &&
                    level > 0
                ) {
                    sb.Append($"<h{level} class='sectionHeader'>{headerTitle}</h{level}>");
                }
                sb.Append("<table><thead><th>Setting name</th><th>Value</th></thead><tbody><tr>");
                sb.Append(string.Join("</tr><tr>", settings
                        .Select(s => $"<td>{HttpUtility.HtmlEncode(s.Key)}</td>" +
                                     $"<td>{HttpUtility.HtmlEncode(s.Value)}</td>")));
                sb.Append("</tr></tbody></table>");

                n.InnerXml = sb.ToString();
            }
            //Resolve any <span> elements with class 'setting-value' and insert the value of
            //the corresponding setting
            nodes = xmlDoc.SelectNodes("//span[@setting-name]");
            foreach (XmlElement n in nodes) {
                var settingName = n.GetAttribute("setting-name");
                if (settings.TryGetValue(settingName, out var settingValue)) {
                    n.InnerText = settingValue;
                }
            }
            return xmlDoc.OuterXml;
        }

        /// <summary>
        /// Resolve all TOC paths to Guids in the specified template HTML
        /// </summary>
        /// <param name="templateHtml">the standard action template html</param>
        /// <returns>the template HTML with all paths resolved to Guids from the TOC tree</returns>
        public string ResolveReportTemplate(string templateHtml) {
            //read the template HTML into a XML document
            try {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(templateHtml);
                //resolve <div> elements which represent a whole section
                var nodes = xmlDoc.SelectNodes("//div[@class='section-outlet' and @section-path]");
                foreach (XmlElement n in nodes) {
                    var sectionPathAttr = n.Attributes["section-path"];
                    var sectionPath = sectionPathAttr.Value;
                    //split the path string separated by ' | '
                    var pathElements = sectionPath.Split('|');
                    var isRecursive = false;
                    if (sectionPath.Contains("*")) {
                        isRecursive = true;
                        pathElements = pathElements.Where(c => c != "*").ToArray();
                    }
                    //strip wildcard
                    //retrieve CSV data formatted as table rows
                    var header = _summaryToc?.GetSubSectionHeaderByTitlePath(pathElements);

                    if (header != null) {
                        n.SetAttribute("class", "section"); //<div class='section' ...
                        n.SetAttribute("data-section-id", header.SectionId.ToString());
                        n.Attributes.Remove(sectionPathAttr);
                        //render the inner XML for this node, which consists of the (optional) header title
                        //and the section content 'stub' which is resolved at run time
                        var sb = new StringBuilder();
                        var title = n.HasAttribute("section-title") ? n.GetAttribute("section-title") : header.Name;
                        var htmlHeaderLevel = 2;
                        if (!string.IsNullOrEmpty(title)) {
                            if (!int.TryParse(n.GetAttribute("header-level"), out htmlHeaderLevel)) {
                                htmlHeaderLevel = 2;
                            }
                            renderSectionHeader(sb, title, htmlHeaderLevel, header.SectionHash);
                        }
                        //append another div with the section outlet which will be retrieved runtime
                        if (isRecursive) {
                            sb.Append($"<div class=\"section-outlet\" render-recursive=\"true\" section-id=\"{header.SectionId}\" header-level=\"{htmlHeaderLevel}\" ></div>");
                        } else {
                            sb.Append($"<div class=\"section-outlet\" section-id=\"{header.SectionId}\"></div>");
                        }

                        //set the inner xml of the node
                        n.InnerXml = sb.ToString();
                    }
                }

                nodes = xmlDoc.SelectNodes("//div[@class='section-outlet' and @section-label]");
                foreach (XmlElement n in nodes) {
                    var sectionLabelAttr = n.Attributes["section-label"];
                    var sectionLabel = sectionLabelAttr.Value;

                    var isRecursive = false;
                    if (sectionLabel.Contains("*")) {
                        isRecursive = true;
                        sectionLabel = sectionLabel.Replace("*", string.Empty);
                    }
                    var header = _summaryToc?.GetSubSectionHeaderBySectionLabel(sectionLabel);
                    if (header != null) {
                        n.SetAttribute("class", "section"); //<div class='section' ...
                        n.SetAttribute("data-section-id", header.SectionId.ToString());
                        n.Attributes.Remove(sectionLabelAttr);
                        //render the inner XML for this node, which consists of the (optional) header title
                        //and the section content 'stub' which is resolved at run time
                        var sb = new StringBuilder();
                        var title = n.HasAttribute("section-title") ? n.GetAttribute("section-title") : header.Name;
                        var htmlHeaderLevel = 2;
                        if (!string.IsNullOrEmpty(title)) {
                            if (!int.TryParse(n.GetAttribute("header-level"), out htmlHeaderLevel)) {
                                htmlHeaderLevel = 2;
                            }
                            renderSectionHeader(sb, title, htmlHeaderLevel, header.SectionHash);
                        }
                        //append another div with the section outlet which will be retrieved runtime
                        if (isRecursive) {
                            sb.Append($"<div class=\"section-outlet\" render-recursive=\"true\" section-id=\"{header.SectionId}\" header-level=\"{htmlHeaderLevel}\"></div>");
                        } else {
                            sb.Append($"<div class=\"section-outlet\" section-id=\"{header.SectionId}\"></div>");
                        }

                        //set the inner xml of the node
                        n.InnerXml = sb.ToString();
                    }
                }

                nodes = xmlDoc.SelectNodes("//div[@class='chart-outlet' and @title-path and @chart-name]");
                if (nodes.Count > 0) {
                    //resolve individual charts, create a dictionary of charts from the summary's chart list
                    var chartsDict = new Dictionary<string, ChartHeader>(StringComparer.OrdinalIgnoreCase);
                    foreach (var h in _summaryToc.ChartHeaders) {
                        var titlePath = string.Join("|", h.TitlePath.Split('|').Select(r => r.Trim()));
                        //if there are duplicates: last chart wins
                        chartsDict[$"{titlePath}\a{h.Name}"] = h;
                    }
                    foreach (XmlElement n in nodes) {
                        var titlePathAttr = n.Attributes["title-path"];
                        var titlePath = string.Join("|", titlePathAttr.Value.Split('|').Select(r => r.Trim()));
                        var chartNameAttr = n.Attributes["chart-name"];
                        var chartName = chartNameAttr.Value;

                        var key = $"{titlePath}\a{chartName}";
                        if (chartsDict.TryGetValue(key, out var chartHeader)) {
                            n.Attributes.Remove(titlePathAttr);
                            n.Attributes.Remove(chartNameAttr);
                            //render the inner XML for this node, which consists of an image tag
                            //'stub' which is resolved at run time
                            var sb = new StringBuilder();
                            sb.AppendChartImageElement(
                                chartHeader.Name,
                                chartHeader.FileExtension,
                                chartHeader.SectionId,
                                chartHeader.Caption
                            );
                            //set the inner xml of the node
                            n.InnerXml = sb.ToString();
                        }
                    }
                }

                //resolve individual tables
                nodes = xmlDoc.SelectNodes("//div[@class='table-outlet' and @title-path and @table-name]");
                if (nodes.Count > 0) {
                    //resolve individual tables, create a dictionary of tables from the summary's table list
                    var tablesDict = new Dictionary<string, CsvDataHeader>(StringComparer.OrdinalIgnoreCase);
                    foreach (var h in _summaryToc.DataHeaders) {
                        var titlePath = string.Join("|", h.TitlePath.Split('|').Select(r => r.Trim()));
                        //if there are duplicates: last chart wins
                        tablesDict[$"{titlePath}\a{h.Name}"] = h;
                    }
                    foreach (XmlElement n in nodes) {
                        var titlePathAttr = n.Attributes["title-path"];
                        var titlePath = string.Join("|", titlePathAttr.Value.Split('|').Select(r => r.Trim()));
                        var tableNameAttr = n.Attributes["table-name"];
                        var tableName = tableNameAttr.Value;
                        var tableCaptionAttr = n.Attributes["table-caption"];
                        var tableCaption = tableCaptionAttr?.Value;
                        var columnOrderAttr = n.Attributes["column-order"];
                        var columnOrder = columnOrderAttr?.Value;
                        var rowLimitAttr = n.Attributes["max-row-count"];
                        if (!int.TryParse(rowLimitAttr?.Value, out var rowLimit)) {
                            rowLimit = 0;
                        }

                        var key = $"{titlePath}\a{tableName}";
                        if (tablesDict.TryGetValue(key, out var tableHeader)) {
                            n.Attributes.Remove(titlePathAttr);
                            n.Attributes.Remove(tableNameAttr);
                            n.Attributes.Remove(tableCaptionAttr);
                            n.Attributes.Remove(columnOrderAttr);
                            n.Attributes.Remove(rowLimitAttr);
                            //render the inner XML for this node, which consists of an image tag
                            //'stub' which is resolved at run time
                            var sb = new StringBuilder();
                            sb.AppendCsvDataTableElement(
                                tableHeader.Name,
                                tableCaption,
                                tableHeader.SectionId,
                                rowLimit,
                                columnOrder
                            );
                            //set the inner xml of the node
                            n.InnerXml = sb.ToString();
                        }
                    }
                }

                var htmlWithData = xmlDoc.OuterXml;
                return htmlWithData;
            } catch (Exception ex) {
                return $"<div class='warning'>Error resolving standard section anchors: {HttpUtility.HtmlEncode(ex)}</div>";
            }
        }


        private static string renderOutputInfo(
            OutputInfo output,
            SectionHeader sectionHeader = null
        ) {
            var sb = new StringBuilder();
            if (sectionHeader != null) {
                sb.Append($"<h1 class='title'>{output.Title}: {sectionHeader.Name}</h1>");
                sb.Append($"<p>MCRA partial report of project {output.Title}, section: {sectionHeader.TitlePath}.</p>");
                sb.Append($"<p>{output.Description ?? string.Empty}</p>");
            } else {
                sb.Append($"<h1 class='title'>{output.Title}</h1>");
                if (!string.IsNullOrEmpty(output.Description)) {
                    sb.Append($"<p>{output.Description}</p>");
                }
            }
            sb.Append("<div>");
            sb.Append("<table>");
            sb.Append("<tr>");
            sb.Append("<td>MCRA version</td>");
            sb.Append($"<td>{output.BuildVersion} (build date: {output.BuildDate})</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td>Output created</td>");
            sb.Append($"<td>{output.DateCreated}</td>");
            sb.Append("</tr>");
            sb.Append("<tr>");
            sb.Append("<td>Execution time</td>");
            sb.Append($"<td>{output.ExecutionTime}</td>");
            sb.Append("</tr>");
            sb.Append("</table>");
            sb.Append("</div>");
            return sb.ToString();
        }

        private string renderSectionRecursive(StringBuilder sb, SectionHeader sectionHeader, int headerLevel) {
            sb.Append($"<div class='section' data-section-id='{sectionHeader.SectionId}'>");
            if (sectionHeader.HasSectionData) {
                renderSectionHeader(sb, sectionHeader.Name, headerLevel, sectionHeader.SectionHash);
            }
            renderSectionContent(sb, sectionHeader);
            foreach (var subHeader in sectionHeader.SubSectionHeaders.OrderBy(h => h.Order)) {
                renderSectionRecursive(sb, subHeader, headerLevel + 1);
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        private void renderSectionHeader(StringBuilder sb, string displayName, int headerLevel, int sectionHash) {
            var htmlHeaderLevel = headerLevel < 6 ? headerLevel : 6;
            sb.Append($"<h{htmlHeaderLevel} class='sectionHeader' id='{sectionHash}'>");
            sb.Append(HttpUtility.HtmlEncode(displayName));
            sb.Append($"</h{htmlHeaderLevel}>");
        }

        private void renderSectionContent(StringBuilder sb, SectionHeader header) {
            try {
                var sectionHtml = header.GetSummarySectionHtml();
                if (!string.IsNullOrWhiteSpace(sectionHtml)) {
                    sb.Append(sectionHtml);
                }
            } catch (Exception ex) {
                sb.Append($"<p>Failed to render the view of section {header.SummarySectionName}.</p>");
                if (System.Diagnostics.Debugger.IsAttached) {
                    sb.Append($"<p>Error: {ex.Message}</p>");
                    sb.Append($"<p>Error: {ex.StackTrace}</p>");
                }
            }
        }

        /// <summary>
        /// Generate the table contents for a CSV data table in HTML
        /// </summary>
        /// <param name="dataId">the Guid of the data section in the toc</param>
        /// <param name="limitRows">the maximum amount of rows (0 for all rows)</param>
        /// <param name="tempPath">the temp folder to save the intermediate CSV file to</param>
        /// <param name="renderFullTable">include thead and tbody elements</param>
        /// <param name="caption">table caption when full table is rendered</param>
        /// <param name="columnOrder">if specified, these are the columns to show in this order</param>
        /// <param name="isTree">if true, the hierarchy defining data should be incorporated</param>
        /// <returns>An optional thead element and tbody element and/or the rows for the tbody</returns>
        public string RetrieveCsvTableContentsHtml(
            Guid dataId,
            int limitRows,
            string tempPath,
            bool renderFullTable = false,
            string caption = null,
            string columnOrder = null,
            bool isTree = false
        ) {
            var sb = new StringBuilder();
            var tempFileName = Path.Combine(tempPath, $"CsvData{dataId:N}.csv");
            var useLimit = limitRows > 0;
            var rowCount = 0;
            var truncateInfoInsertPosition = 0;
            var dataHeader = _summaryToc?.GetDataHeader(dataId);
            if (dataHeader == null) {
                return string.Empty;
            }
            dataHeader.SaveCsvFile(_summaryToc.SectionManager, tempFileName);
            var isTreeTable = dataHeader.TreeTableProperties != null;
#if DEBUG
            //for reference: add column info as comment in the returned html
            sb.AppendLine("<!--");
            sb.AppendLine("Types:   " + string.Join(" | ", dataHeader.ColumnTypes));
            sb.AppendLine("Formats: " + string.Join(" | ", dataHeader.DisplayFormats));
            sb.AppendLine("Units:   " + string.Join(" | ", dataHeader.Units));
            sb.AppendLine("-->");
#endif
            using (var fs = new FileStream(tempFileName, FileMode.Open, FileAccess.Read))
            using (var csvReader = new CsvDataReader(fs, fieldTypes: dataHeader.GetTypes(), encoding: Encoding.Default, allowDuplicateHeaders: true)) {

                int[] fieldIndices = null;
                var fieldCount = csvReader.FieldCount;
                //check if we need to override the column ordering
                if (!string.IsNullOrEmpty(columnOrder)) {
                    //create array of integers from column order string
                    //formatted as a comma separated list, if a number can't
                    //be parsed, set to -1
                    fieldIndices = columnOrder.Split(',')
                        .Select(s => int.TryParse(s, out var n) ? n : -1)
                        .ToArray();
                    fieldCount = fieldIndices.Length;
                }
                //add headers according to optional override of column ordering
                var shownHeaders = fieldIndices == null
                         ? csvReader.Header
                         : fieldIndices.Where(i => i >= 0)
                        .Select(i => csvReader.Header[i]);

                //Optionally build the table head and body
                if (renderFullTable) {
                    //backward compatibility: caption parameter was used when caption was
                    //saved in an attribute of the table placeholder, now it is part of the tableHeader
                    if (string.IsNullOrEmpty(caption)) {
                        caption = dataHeader.Caption;
                    }
                    if (!string.IsNullOrEmpty(caption)) {
                        var encodedCaption = HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(caption));
                        sb.Append($"<caption>{encodedCaption}</caption>");
                    }
                    sb.Append("<thead><th>");

                    sb.AppendLine(
                        string.Join("</th><th>",
                            shownHeaders.Where(h => !h.StartsWith("__"))
                                .Select(h => HttpUtility.HtmlEncode(h))
                    ));
                    sb.Append("</th></thead><tbody>");
                    truncateInfoInsertPosition = sb.Length;
                }

                //build the table rows
                var maxCount = limitRows;
                while ((!useLimit || (rowCount++ < maxCount)) && csvReader.Read()) {
                    var rowBuilder = new StringBuilder("<tr");
                    var fieldBuilder = new StringBuilder();

                    //properties for hierarchical tables
                    string identifierValue = null;
                    string parentIdValue = null;
                    var isDataNode = false;

                    for (int i = 0; i < fieldCount; i++) {
                        var idx = fieldIndices?[i] ?? i;
                        //skip values if they are < 0 (when a columnorder parse failed)
                        if (idx < 0) {
                            continue;
                        }

                        var valueType = csvReader.GetFieldType(idx);
                        var value = csvReader.GetValue(idx);
                        var strValue = HttpUtility.HtmlEncode(valueType.PrintValue(value, dataHeader.DisplayFormats[idx]));
                        var fieldName = csvReader.Header[idx];

                        //special case, if this is a hierarchical table
                        if (isTreeTable) {
                            if (fieldName.Equals(dataHeader.TreeTableProperties.IdFieldName)) {
                                identifierValue = strValue;
                            } else if (fieldName.Equals(dataHeader.TreeTableProperties.ParentFieldName)) {
                                parentIdValue = strValue;
                            } else if (fieldName.Equals(dataHeader.TreeTableProperties.IsDataNodeFieldName)) {
                                isDataNode = bool.Parse(value.ToString());
                            }
                        }
                        if (!fieldName.StartsWith("__")) {
                            fieldBuilder.Append("<td>");
                            fieldBuilder.Append(strValue);
                            fieldBuilder.Append("</td>");
                        }
                    }
                    if (isTreeTable) {
                        if (isDataNode) {
                            //if the current request is not for a tree
                            //then skip all data nodes
                            if (!isTree) {
                                //stop processing this row, increment row limit (maxCount),
                                //so this skipped row does not add to the total limited row count
                                maxCount++;
                                continue;
                            }
                            rowBuilder.Append(" class=\"data-row\"");
                        } else {
                            rowBuilder.Append(" class=\"treenode-row\"");
                        }
                        if (!string.IsNullOrEmpty(identifierValue)) {
                            rowBuilder.Append($" data-tt-id=\"{identifierValue}\"");
                        }
                        if (!string.IsNullOrEmpty(parentIdValue)) {
                            rowBuilder.Append($" data-tt-parent-id=\"{parentIdValue}\"");
                        }
                    }

                    rowBuilder.Append(">");
                    sb.Append(rowBuilder);
                    sb.Append(fieldBuilder);
                    sb.Append("</tr>");
                }
                //if limit is used and there are still records
                //notify by inserting a column before the data
                if (useLimit && csvReader.Read()) {
                    sb.Insert(truncateInfoInsertPosition,
                                $"<tr class='warning truncated-rows tablesorter-ignoreRow'>" +
                                $"<td colspan='{fieldCount}'><strong>Note</strong>: " +
                                $"This table shows only the first {limitRows} rows. " +
                                "To view all data, <span class='hide-in-print'>click the sort button " +
                                "or </span>use the CSV download function.</td></tr>");
                }
                if (renderFullTable) {
                    sb.Append("</tbody>");
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// Generate the table contents for a CSV data table in a HTML rotated table
        /// with the headers as first column of the table
        /// </summary>
        /// <param name="dataId">the Guid of the data section in the toc</param>
        /// <param name="tempPath">the temp folder to save the intermediate CSV file to</param>
        /// <param name="caption">table caption when full table is rendered</param>
        /// <param name="columnOrder">if specified, these are the columns to show as rows in this order</param>
        /// <returns>A thead element and tbody element and the rotated table for the tbody</returns>
        public string RetrieveCsvRotatedTableContentsHtml(
            Guid dataId,
            string tempPath,
            string caption = null,
            string columnOrder = null
        ) {
            var sb = new StringBuilder();
            var tempFileName = Path.Combine(tempPath, $"CsvData{dataId:N}.csv");
            var dataHeader = _summaryToc?.GetDataHeader(dataId);
            if (dataHeader == null) {
                return string.Empty;
            }
            dataHeader.SaveCsvFile(_summaryToc.SectionManager, tempFileName);
#if DEBUG
            //for reference: add column info as comment in the returned html
            sb.AppendLine("<!--");
            sb.AppendLine("Types:   " + string.Join(" | ", dataHeader.ColumnTypes));
            sb.AppendLine("Formats: " + string.Join(" | ", dataHeader.DisplayFormats));
            sb.AppendLine("Units:   " + string.Join(" | ", dataHeader.Units));
            sb.AppendLine("-->");
#endif
            using (var fs = new FileStream(tempFileName, FileMode.Open, FileAccess.Read))
            using (var csvReader = new CsvDataReader(fs, fieldTypes: dataHeader.GetTypes(), encoding: Encoding.Default, allowDuplicateHeaders: true)) {

                int[] fieldIndices = null;
                var fieldCount = csvReader.FieldCount;
                //check if we need to override the column ordering
                if (!string.IsNullOrEmpty(columnOrder)) {
                    //create array of integers from column order string
                    //formatted as a comma separated list, if a number can't
                    //be parsed, set to -1
                    fieldIndices = columnOrder.Split(',')
                        .Select(s => int.TryParse(s, out var n) ? n : -1)
                        .ToArray();
                    fieldCount = fieldIndices.Length;
                }
                //add headers according to optional override of column ordering
                var shownHeaders = fieldIndices == null
                         ? csvReader.Header
                         : fieldIndices.Where(i => i >= 0)
                        .Select(i => csvReader.Header[i]);

                //construct row builders as separate stringbuilders
                //initialized with the column header as first value
                //omit any 'hidden' columns starting with two underscores
                var rowBuilders = shownHeaders
                    .Select(h => new StringBuilder($"<td>{HttpUtility.HtmlEncode(h)}</td>"))
                    .ToArray();

                //build the table by appending to the row builders
                while (csvReader.Read()) {
                    for (int i = 0; i < fieldCount; i++) {
                        var idx = fieldIndices?[i] ?? i;
                        //skip values if they are < 0 (when a columnorder parse failed)
                        if (idx < 0) {
                            continue;
                        }

                        var valueType = csvReader.GetFieldType(idx);
                        var value = csvReader.GetValue(idx);
                        var strValue = HttpUtility.HtmlEncode(valueType.PrintValue(value, dataHeader.DisplayFormats[idx]));
                        var fieldName = csvReader.Header[idx];

                        rowBuilders[idx].Append($"<td>{strValue}</td>");
                    }
                }
                //build the rotated table
                //add caption from either the parameter (override)
                //or the data header value, by default
                if (string.IsNullOrEmpty(caption)) {
                    caption = dataHeader.Caption;
                }
                if (!string.IsNullOrEmpty(caption)) {
                    var encodedCaption = HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(caption));
                    sb.Append($"<caption>{encodedCaption}</caption>");
                }
                sb.Append("<tbody>");
                foreach (var builder in rowBuilders) {
                    sb.Append($"<tr>{builder}</tr>");
                }
                sb.Append("</tbody>");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Replaces all table and chart section contents (i.e., dummy elements) with contents
        /// of table and chart sections.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="tempPath"></param>
        /// <param name="inline">Render the images inline as base64 PNG img and embedded svg tags</param>
        /// <returns></returns>
        public string ResolveChartsAndTables(string html, string tempPath, bool inline = false) {
            if (tempPath != null) {
                html = resolveTableContents(html, 200, tempPath);
                if (inline) {
                    html = inlineChartContents(html, tempPath);
                } else {
                    html = resolveChartContents(html, tempPath);
                }
            }
            return html;
        }

        /// <summary>
        /// Retrieve the section's data which is saved in CSV format and append it
        /// to the table body elements as needed.
        /// First load the html into an XML document, find the tags and append the rows
        /// </summary>
        /// <param name="sectionHtml">html string complying to XHTML standard</param>
        /// <param name="limitRows">Limit csv table rows to this maximum number of rows</param>
        /// <param name="tempPath">Limit csv table rows to this maximum number of rows</param>
        /// <returns>html string with table data</returns>
        private string resolveTableContents(string sectionHtml, int limitRows, string tempPath) {
            try {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sectionHtml);
                //old style, via table body, resolve the rows
                var nodes = xmlDoc.SelectNodes("//tbody[@class='table-csv' and @data-csv-id]");
                foreach (XmlNode n in nodes) {
                    var tableId = new Guid(n.Attributes["data-csv-id"].Value);
                    int maxRecords = limitRows;
                    if (!int.TryParse(n.Attributes["csv-max-records"]?.Value, out var maxRows)) {
                        maxRows = limitRows;
                    }
                    //retrieve CSV data formatted as table rows
                    var tableRowsHtml = RetrieveCsvTableContentsHtml(tableId, maxRecords, tempPath);
                    n.InnerXml = tableRowsHtml;
                }
                //new method, whole table
                nodes = xmlDoc.SelectNodes("//table[contains(@class, 'csv-data-table') and @csv-download-id]");
                foreach (XmlNode n in nodes) {
                    var tableId = new Guid(n.Attributes["csv-download-id"].Value);
                    var caption = n.Attributes["csv-table-caption"]?.Value;
                    if (!int.TryParse(n.Attributes["csv-max-records"]?.Value, out var maxRows)) {
                        maxRows = limitRows;
                    }
                    var columnOrder = n.Attributes["csv-column-order"]?.Value;
                    _ = bool.TryParse(n.Attributes["csv-table-rotate"]?.Value, out var rotate);
                    //retrieve CSV data formatted as the whole table
                    var tableRowsHtml = rotate
                                      ? RetrieveCsvRotatedTableContentsHtml(tableId, tempPath, caption, columnOrder)
                                      : RetrieveCsvTableContentsHtml(tableId, maxRows, tempPath, true, caption, columnOrder);
                    n.InnerXml = tableRowsHtml;
                }

                var htmlWithData = xmlDoc.OuterXml;
                return htmlWithData;
            } catch (Exception ex) {
                return $"<div class='warning'>Error rendering section table data: {HttpUtility.HtmlEncode(ex)}</div>";
            }
        }

        /// <summary>
        /// Retrieve the section's charts and include these in the generated html.
        /// </summary>
        /// <param name="sectionHtml">html string complying to XHTML standard</param>
        /// <param name="tempPath"></param>
        /// <returns>html string with table data</returns>
        private string resolveChartContents(string sectionHtml, string tempPath) {
            try {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sectionHtml);
                var chartNodes = xmlDoc.SelectNodes("//img[(@class='chart-png dummy' or @class='chart-svg dummy') and @chart-id]");
                foreach (XmlNode n in chartNodes) {
                    var chartId = new Guid(n.Attributes["chart-id"].Value);
                    var filename = saveTempImageFile(chartId, tempPath);
                    var srcAttribute = xmlDoc.CreateAttribute("src");
                    srcAttribute.Value = filename;
                    n.Attributes.Append(srcAttribute);
                }
                var htmlWithData = xmlDoc.OuterXml;
                return htmlWithData;
            } catch (Exception ex) {
                return $"<div class='warning'>Error rendering section chart contents: {HttpUtility.HtmlEncode(ex)}</div>";
            }
        }

        /// <summary>
        /// Retrieve the section's charts and render these inline in the generated html.
        /// </summary>
        /// <param name="sectionHtml">html string complying to XHTML standard</param>
        /// <param name="tempPath"></param>
        /// <returns>html string with table data</returns>
        private string inlineChartContents(string sectionHtml, string tempPath) {
            try {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sectionHtml);
                //create inline PNG representation
                var chartNodes = xmlDoc.SelectNodes("//img[@class='chart-png dummy' and @chart-id]");
                foreach (XmlNode n in chartNodes) {
                    var chartId = new Guid(n.Attributes["chart-id"].Value);
                    var filename = Path.Combine(tempPath, saveTempImageFile(chartId, tempPath));
                    var imgBytes = File.ReadAllBytes(filename);
                    var srcAttribute = xmlDoc.CreateAttribute("src");
                    srcAttribute.Value = $"data:image/png;base64,{Convert.ToBase64String(imgBytes)}";
                    n.Attributes.Append(srcAttribute);
                }

                //create inline svg tags for the SVG images
                chartNodes = xmlDoc.SelectNodes("//img[@class='chart-svg dummy' and @chart-id]");
                foreach (XmlNode n in chartNodes) {
                    //get the chart data
                    var chartId = new Guid(n.Attributes["chart-id"].Value);
                    var filename = Path.Combine(tempPath, saveTempImageFile(chartId, tempPath));
                    var svgLines = File.ReadAllLines(filename);
                    //skip first 2 lines (xml element and doctype element)
                    //paste all lines in one line string
                    var svgString = string.Join("", svgLines.Skip(2).Select(s => s.Trim()));
                    //create a new svg tag from the full svg string,
                    //this creates a new <svg> element
                    var svgNode = xmlDoc.CreateDocumentFragment();
                    svgNode.InnerXml = svgString;

                    var figureNode = n.ParentNode;
                    //append the <svg> to the figure node
                    figureNode.AppendChild(svgNode);
                    //remove the dummy <img> tag
                    figureNode.RemoveChild(n);
                }

                var htmlWithData = xmlDoc.OuterXml;
                return htmlWithData;
            } catch (Exception ex) {
                return $"<div class='warning'>Error rendering section chart contents: {HttpUtility.HtmlEncode(ex)}</div>";
            }
        }

        /// <summary>
        /// Tries to fetch the image file from the chart header and save it
        /// locally in the application temp path so that it can be included
        /// in a (temporary) html file that can be printed to pdf.
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="tempPath"></param>
        /// <returns></returns>
        private string saveTempImageFile(Guid dataId, string tempPath) {
            var chartHeader = _summaryToc?.GetChartHeader(dataId);
            if (chartHeader == null) {
                return string.Empty;
            }
            var fileName = $"Chart{dataId:N}.{chartHeader.FileExtension}";
            var chartFilePath = Path.Combine(tempPath, fileName);
            chartHeader.SaveChartFile(_summaryToc.SectionManager, chartFilePath);
            return fileName;
        }

        private static string loadCss() {
            var location = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);
            var assemblyFolder = (new FileInfo(location.LocalPath).Directory).FullName;
            var cssFile = Path.Combine(assemblyFolder, "Resources/ReportTemplate/print.css");
            var css = File.ReadAllText(cssFile);
            return css;
        }
    }
}
