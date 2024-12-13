using MCRA.Utils.Charting;
using Microsoft.AspNetCore.Html;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Helpers {

    public enum ChartFileType {
        Svg,
        Png
    };

    /// <summary>
    /// Utility methods for rendering charts.
    /// </summary>
    public static class ChartHelpers {

        /// <summary>
        /// Generic chart rendering method. Chart can be embedded in html or a
        /// separate chart section can be generated.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="section"></param>
        /// <param name="viewBag"></param>
        /// <param name="chartCreator"></param>
        /// <param name="fileType"></param>
        /// <param name="saveChartFile"></param>
        /// <param name="caption"></param>
        /// <param name="altText"></param>
        /// <param name="chartData"></param>
        /// <returns></returns>
        public static HtmlString Chart(
            string name,
            SummarySection section,
            ViewParameters viewBag,
            IReportChartCreator chartCreator,
            ChartFileType fileType,
            bool saveChartFile = false,
            string caption = null,
            string altText = null,
            CsvDataSummarySection chartData = null,
            string sectionLabel = null
        ) {
            var sb = new StringBuilder();
            sb.Append("<figure>");
            if (saveChartFile && !string.IsNullOrEmpty(viewBag?.TempPath)) {
                //Create chart section for this file
                var fileExtension = getFileExtension(fileType);
                var chartTempFile = $"{name}-{chartCreator.ChartId}.{fileExtension}";
                var chartFileName = Path.Combine(viewBag.TempPath ?? Path.GetTempPath(), chartTempFile);
                var chartSection = new ChartSummarySection(
                    name,
                    chartFileName,
                    viewBag?.TitlePath,
                    caption,
                    sectionLabel: sectionLabel
                );
                section.ChartSections.Add(chartSection);
                var sectionGuid = chartSection.SectionGuid;
                //Write the chart to the temp file
                if (fileType == ChartFileType.Png) {
                    chartCreator.CreateToPng(chartSection.TempFileName);
                } else if (fileType == ChartFileType.Svg) {
                    chartCreator.CreateToSvg(chartSection.TempFileName);
                }
                sb.AppendChartImageElement(name, fileExtension, sectionGuid, caption, altText, chartData);
            } else {
                if (!string.IsNullOrEmpty(caption)) {
                    sb.Append($"<figcaption>{caption}</figcaption>");
                }
                if (fileType == ChartFileType.Png) {
                    sb.Append($"<img ");
                    sb.Append($"src='data:image/png;base64,{RenderBase64Image(chartCreator)}'");
                    if (!string.IsNullOrEmpty(altText)) {
                        sb.Append($" alt='{altText}'");
                    }
                    if (chartData != null) {
                        sb.Append($" csv-download-id='{chartData.SectionId:N}' csv-download-name='{chartData.TableName}'");
                    }
                    sb.Append(" />");
                } else if (fileType == ChartFileType.Svg) {
                    sb.Append(RenderSvg(chartCreator, chartData));
                }
            }
            sb.Append("</figure>");
            return new HtmlString(sb.ToString());
        }

        /// <summary>
        /// Creates a base 64 chart.
        /// </summary>
        /// <param name="chartCreator"></param>
        /// <returns></returns>
        public static string RenderBase64Image(IChartCreator chartCreator) {
            using (var ms = new MemoryStream()) {
                chartCreator.WritePngToStream(ms);
                var imgBytes = ms.ToArray();
                var result = Convert.ToBase64String(imgBytes);
                return result;
            }
        }

        /// <summary>
        /// Renders the chart as svg string.
        /// </summary>
        /// <param name="chartCreator">The chart creator creating the chart.</param>
        /// <param name="chartData">If specified, the data section that should
        /// be linked to the chart (by means of a csv download attribute).</param>
        /// <returns></returns>
        public static string RenderSvg(
            IReportChartCreator chartCreator,
            CsvDataSummarySection chartData = null
        ) {
            try {
                var str = chartCreator.ToSvgString(chartCreator.Width, chartCreator.Height);
                //remove everything before <svg and after </svg>
                var posSvg = str.IndexOf("<svg");
                var posEnd = str.IndexOf("</svg>");
                var len = posEnd + 6 - posSvg;
                var svgOnly = str.Substring(posSvg, len);
                if (chartData != null) {
                    posSvg = svgOnly.IndexOf("<svg");
                    var csvDownloadAttributes = $" csv-download-id='{chartData.SectionId:N}' csv-download-name='{chartData.TableName}'";
                    svgOnly = svgOnly.Insert(posSvg + 4, csvDownloadAttributes);
                }
                return svgOnly;
            } catch (Exception ex) {
                var sb = new StringBuilder();
                sb.AppendParagraph($"Failed to render chart {chartCreator.ChartId}.");
                if (System.Diagnostics.Debugger.IsAttached) {
                    sb.AppendParagraph($"Error: {ex.Message}");
                    sb.AppendParagraph($"Error: {ex.StackTrace}");
                }
                return sb.ToString();
            }
        }

        private static string getFileExtension(ChartFileType fileType) {
            return fileType switch {
                ChartFileType.Svg => "svg",
                ChartFileType.Png => "png",
                _ => throw new Exception($"Unknown chart file type {fileType}."),
            };
        }

        #region StringBuilderExtensions

        public static StringBuilder AppendBase64Image(
            this StringBuilder sb,
            IChartCreator chartCreator,
            string altText = ""
        ) {
            return sb.Append($"<img src='data:image/png;base64,{RenderBase64Image(chartCreator)}' alt='{altText}' />");
        }

        public static StringBuilder AppendChart(
            this StringBuilder sb,
            string name,
            IReportChartCreator chartCreator,
            ChartFileType fileType,
            SummarySection section,
            ViewParameters viewBag,
            string caption = null,
            bool saveChartFile = false,
            string altText = null,
            CsvDataSummarySection chartData = null,
            string sectionLabel = null
        ) {
            return sb.Append(Chart(
                name: name,
                section: section,
                viewBag: viewBag,
                chartCreator: chartCreator,
                fileType: fileType,
                saveChartFile: saveChartFile,
                caption: caption,
                altText: altText,
                chartData: chartData,
                sectionLabel: sectionLabel
            ));
        }

        public static StringBuilder AppendChartImageElement(
            this StringBuilder sb,
            string name,
            string fileExtension,
            Guid sectionGuid,
            string caption = null,
            string altText = null,
            CsvDataSummarySection chartData = null
        ) {
            if (!string.IsNullOrEmpty(caption)) {
                sb.Append($"<figcaption>{caption}</figcaption>");
            }
            sb.Append($"<img ");
            sb.Append($" class='chart-{fileExtension} dummy'");
            sb.Append($" chart-id='{sectionGuid:N}'");
            sb.Append($" chart-name='{name}'");
            if (chartData != null) {
                sb.Append($" csv-download-id='{chartData.SectionId:N}' csv-download-name='{chartData.TableName}'");
            }
            if (!string.IsNullOrEmpty(altText)) {
                sb.Append($" alt='{altText}'");
            }
            sb.Append(" />");
            return sb;
        }

        #endregion
    }
}
