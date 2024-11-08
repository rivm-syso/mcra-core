using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using System.Text;
using System.Web;

namespace MCRA.Simulation.OutputGeneration.Helpers {

    /// <summary>
    /// Methods to automatically generate the HTML for summarizing a section-based report.
    /// </summary>
    public static class SectionHelper {

        /// <summary>
        /// Renders the given section as html.
        /// </summary>
        /// <param name="sectionHeader"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static string RenderSingleSection(SectionHeader sectionHeader, SummarySection section) {
            var sb = new StringBuilder();
            var headerLevel = sectionHeader?.Depth + 1 ?? 1;
            sb.Append($"<div class='section' data-section-id='{sectionHeader.SectionId}'>");
            renderSectionContent(sb, sectionHeader, section, headerLevel);
            foreach (var subSectionInfo in sectionHeader.SubSectionHeaders.OrderBy(h => h.Order)) {
                renderSectionRecursive(sb, subSectionInfo, section, headerLevel + 1);
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        private static void renderSectionRecursive(StringBuilder sb, SectionHeader sectionHeader, SummarySection section, int headerLevel) {
            sb.Append($"<div class='section' data-section-id='{sectionHeader.SectionId}'>");
            //only render section header when the section itself doesn't contain data and is
            //part of a parent section containing the data
            renderSectionContent(sb, sectionHeader, section, headerLevel);
            foreach (var subHeader in sectionHeader.SubSectionHeaders.OrderBy(h => h.Order)) {
                renderSectionRecursive(sb, subHeader, section, headerLevel + 1);
            }
            sb.Append("</div>");
        }

        private static void renderSectionHeader(StringBuilder sb, string displayName, int headerLevel, int sectionHash) {
            var htmlHeaderLevel = headerLevel < 6 ? headerLevel : 6;
            sb.Append($"<h{htmlHeaderLevel} class='sectionHeader' id='{sectionHash}'>");
            sb.Append(displayName.ToHtml());
            sb.Append($"</h{htmlHeaderLevel}>");
        }

        private static void renderSectionContent(StringBuilder sb, SectionHeader header, SummarySection section, int headerLevel) {
            try {
                var renderSection = section.GetSectionRecursive(header.SectionId);
                if (renderSection != null) {
                    var toc = header.TocRoot;
                    var view = SectionViewBuilder.CreateView(renderSection, header.SummarySectionName, toc)
                            ?? SectionViewBuilder.CreateView(renderSection, header.SectionTypeName, toc);

                    if (view != null) {
                        //only render section header when the section itself doesn't contain data and is
                        //part of a parent section containing the data
                        if (!header.HasSectionData) {
                            renderSectionHeader(sb, header.Name, headerLevel, header.SectionHash);
                        }

                        var unitsDictionary = header.GetUnitsDictionary();
                        view.ViewBag.UnitsDictionary = unitsDictionary;
                        view.ViewBag.TempPath = toc.SectionManager?.GetTempDataFolder();
                        view.ViewBag.TitlePath = header.TitlePath;
                        view.RenderSectionHtml(sb);
                    } else {
                        //append comment in html when no view is defined
                        sb.Append($"<!-- No view defined for {header.SummarySectionName.ToHtml()} or {header.SectionTypeName.ToHtml()} -->");
                    }
                    //collect data sections from rendered section if it is a subsection
                    if (renderSection != section) {
                        foreach (var dataSection in renderSection.DataSections) {
                            section.DataSections.Add(dataSection);
                        }
                    }
                }

            } catch (Exception ex) {
                sb.AppendParagraph($"Failed to render the view of section {header.SummarySectionName}.");
                if (System.Diagnostics.Debugger.IsAttached) {
                    sb.AppendParagraph($"Error: {ex.Message}");
                    sb.AppendParagraph($"Error: {ex.StackTrace}");
                }
            }
        }

        #region StringBuilderExtensions

        public static StringBuilder AppendNotification(this StringBuilder sb, string content) {
            sb.AppendParagraph(content, "notification");
            return sb;
        }

        public static StringBuilder AppendWarning(this StringBuilder sb, string content) {
            sb.AppendParagraph(content, "warning");
            return sb;
        }

        public static StringBuilder AppendParagraph(this StringBuilder sb, string content, params string[] classes) {
            if (classes.Length > 0) {
                sb.Append($"<p class='{string.Join(" ", classes)}'>");
            } else {
                sb.Append("<p>");
            }
            sb.Append(content.ToHtml());
            sb.Append("</p>");
            return sb;
        }

        public static StringBuilder AppendSettingsReference(this StringBuilder sb, SummaryToc toc, ActionType actionType, string title = null) {
            var header = toc?.GetSubSectionHeaderFromTitleString<SettingsSummarySection>(actionType.GetDisplayName());
            if (header != null) {
                sb.Append($"<p class='settings-reference'>");
                sb.Append($"See <span class='section-link' data-section-id='{header.SectionId}'>{title ?? "module settings."}</span>");
                return sb.Append("</p>");
            }
            return sb;
        }

        public static StringBuilder AppendDescriptionParagraph(this StringBuilder sb, string content, params SectionReference[] refs) {
            sb.Append($"<p class='description'>");
            if (refs?.Length > 0) {
                var hyperlinks = refs
                    .Select(r => $"<span class='section-link' data-section-id='{r.SectionId}'>{r.Title}</span>")
                    .ToArray();
                //use string.Format to insert the hyperlinks in the designated positions in the html string
                var htmlText = string.Format(content.ToHtml(), hyperlinks);
                sb.Append(htmlText);
            } else {
                sb.Append(content.ToHtml());
            }

            return sb.Append("</p>");
        }

        public static StringBuilder AppendDescriptionTable(this StringBuilder sb, List<(string, string)> descriptions) {
            if (descriptions?.Count > 0) {
                sb.Append($"<table class='description-table'>");
                sb.Append($"<body>");
                foreach (var d in descriptions) {
                    sb.Append($"<tr>");
                    sb.Append($"<td>{d.Item1}</td>");
                    sb.Append($"<td>{d.Item2}</td>");
                    sb.Append($"</tr>");
                }
                sb.Append($"</body>");
                sb.Append($"</table>");
            }
            return sb;
        }

        public static StringBuilder AppendDescriptionList(this StringBuilder sb, List<string> descriptions) {
            if (descriptions?.Count > 0) {
                sb.Append($"<ul class='description-list'>");
                foreach (var d in descriptions) {
                    sb.Append($"<li>{d}</li>");
                }
                sb.Append($"</ul>");
            }
            return sb;
        }

        public static StringBuilder AppendTableRow(this StringBuilder sb, params object[] fields) {
            sb.Append($"<tr>");
            foreach (var s in fields) {
                sb.Append($"<td>{s.ToHtml()}</td>");
            }
            return sb.Append("</tr>");
        }

        public static StringBuilder AppendRawTableRow(this StringBuilder sb, params object[] fields) {
            sb.Append($"<tr>");
            foreach (var s in fields) {
                sb.Append($"<td>{s}</td>");
            }
            return sb.Append("</tr>");
        }

        public static StringBuilder AppendHeaderRow(this StringBuilder sb, params object[] fields) {
            sb.Append($"<tr>");
            foreach (var s in fields) {
                sb.Append($"<th>{s.ToHtml()}</th>");
            }
            return sb.Append("</tr>");
        }

        public static StringBuilder AppendRawHeaderRow(this StringBuilder sb, params object[] fields) {
            sb.Append($"<tr>");
            foreach (var s in fields) {
                sb.Append($"<th>{s}</th>");
            }
            return sb.Append("</tr>");
        }

        public static IList<string> AddDescriptionItem(this IList<string> list, string content, params SectionReference[] refs) {
            if (!string.IsNullOrWhiteSpace(content)) {
                string htmlText = FormatWithSectionLinks(content, refs);
                list.Add(htmlText);
            }
            return list;
        }

        public static string FormatWithSectionLinks(string content, params SectionReference[] refs) {
            if (!string.IsNullOrWhiteSpace(content) && (refs?.Length > 0)) {
                var hyperlinks = refs
                .Select(r => $"<span class='section-link' data-section-id='{r.SectionId}'>{r.Title}</span>")
                .ToArray();
                //use string.Format to insert the hyperlinks in the designated positions in the html string
                var htmlText = string.Format(content.ToHtml(), hyperlinks);
                return htmlText;
            } else {
                return content;
            }
        }

        public static string ToHtml(this object s) {
            return s == null
                ? string.Empty
                : HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(s.ToString()));
        }

        public static string ToHtml(this string s) {
            return HttpUtility.HtmlEncode(HttpUtility.HtmlDecode(s));
        }

        #endregion
    }
}
