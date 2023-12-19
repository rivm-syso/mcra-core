using MCRA.Utils.Csv;
using MCRA.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Html;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders {

    /// <summary>
    /// Class that builds HTML tables based on a generic list of records.
    /// </summary>
    public class HtmlCsvExportLinkBuilder<TRecord> : HtmlGenericElementBuilder<TRecord> {

        /// <summary>
        /// The section for which to build the table
        /// </summary>
        public SummarySection Section { get; set; }
        /// <summary>
        /// If true, then the table header is shown. Otherwise, it is not shown.
        /// </summary>
        public bool ShowHeader { get; set; }

        /// <summary>
        /// The name of the table
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Temp save path for csv
        /// </summary>
        public string TempPath { get; set; }

        /// <summary>
        /// Builds the html table.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public HtmlString Build(IEnumerable<TRecord> items) {
            if (items == null || !items.Any()) {
                return new HtmlString(string.Empty);
            }
            var sb = new StringBuilder();
            var visibleProperties = typeof(TRecord).GetVisibleProperties(HiddenProperties).ToList();

            //Create data section for this file
            var csvTempFile = $"{TableName}-{Id}.csv";
            var dataSection = new CsvDataSummarySection(
                TableName,
                Path.Combine(TempPath ?? Path.GetTempPath(), csvTempFile),
                ViewBag?.TitlePath,
                visibleProperties,
                ViewBag?.UnitsDictionary
            );

            Section.DataSections.Add(dataSection);
            //Write the CSV to the temp file
            var csvWriter = new CsvWriter();
            csvWriter.WriteToCsvFile(items, dataSection.CsvFileName, ShowHeader, createHeaderFormatter(), visibleProperties);

            //Create a toolbar in HTML with a download link for this CSV section
            sb.Append("<div class='toolbar'>");
            //register in a data section
            sb.Append($"<a class='button icon_csv' data-csv-id='{dataSection.SectionGuid:N}' data-csv-name='{TableName}'>");
            sb.Append("<span>");
            sb.Append("</span>");
            sb.Append("</a>");
            sb.Append("</div>");

            return new HtmlString(sb.ToString());
        }
    }
}
