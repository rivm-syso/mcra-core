using MCRA.Utils.Csv;
using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Helpers {
    public class DataSectionBuilder<TRecord> : HtmlGenericElementBuilder<TRecord> {

        public DataSectionBuilder(
            SummarySection section,
            bool showHeader,
            ViewParameters viewBag,
            IList<string> hiddenProperties
        ) {
            Section = section;
            ViewBag = viewBag;
            TempPath = viewBag.TempPath;
            ShowHeader = showHeader;
            HiddenProperties = hiddenProperties;
        }

        /// <summary>
        /// The section for which to build the table
        /// </summary>
        public SummarySection Section { get; set; }

        /// <summary>
        /// Temporary path for saving intermediate CSV files
        /// </summary>
        public string TempPath { get; set; }

        /// <summary>
        /// If true, then the table header is shown. Otherwise, it is not shown.
        /// </summary>
        public bool ShowHeader { get; set; }

        /// <summary>
        /// Builds the data section.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public CsvDataSummarySection Build(IList<TRecord> items, string tableName, string id) {
            //Create data section for this file
            var visibleProperties = typeof(TRecord).GetVisibleProperties(HiddenProperties).ToList();
            var csvTempFile = $"{id}.csv";
            var filename = Path.Combine(TempPath ?? Path.GetTempPath(), csvTempFile);
            var dataSection = new CsvDataSummarySection(
                tableName,
                filename,
                ViewBag?.TitlePath,
                visibleProperties,
                ViewBag?.UnitsDictionary
            );
            Section.DataSections.Add(dataSection);
            //Write the CSV to the temp file
            var csvWriter = CsvOutputWriterFactory.Create();
            csvWriter.WriteToCsvFile(items, dataSection.CsvFileName, ShowHeader, createHeaderFormatter(), visibleProperties);

            return dataSection;
        }
    }
}
