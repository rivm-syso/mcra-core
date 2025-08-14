using MCRA.Simulation.OutputManagement;
using MCRA.Utils.DataFileReading;
using System.Data;
using System.Xml.Serialization;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// This class has a set of properties which describe the
    /// custom columns in the csv file which are necessary to build a
    /// hierarchical table and should not be shown in the output on
    /// screen and in reports
    /// </summary>
    public class TreeTableProperties {
        public string IdFieldName { get; set; }
        public string ParentFieldName { get; set; }
        public string IsDataNodeFieldName { get; set; }
    }

    /// <summary>
    /// Special header type which stores a zipped CSV file contents
    /// with its intended visual data representation in the output tables
    /// in HTML of PDF rendered output
    /// </summary>
    public class CsvDataHeader : IHeader {

        public CsvDataHeader() { }

        /// <summary>
        /// Section id.
        /// </summary>
        public Guid SectionId { get; set; }

        /// <summary>
        /// Table name
        /// </summary>
        [XmlElement("TableName")]
        public string Name { get; set; }

        /// <summary>
        /// Table caption
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Hierarchical section header title path to the
        /// section containing this table
        /// </summary>
        public string TitlePath { get; set; }

        /// <summary>
        /// This is only non-null when the table is a hierarchical table
        /// </summary>
        public TreeTableProperties TreeTableProperties { get; set; } = null;

        /// <summary>
        /// List of system types for the columns in the output table
        /// </summary>
        [XmlArray]
        public string[] ColumnTypes { get; set; }

        /// <summary>
        /// List of display formats for each column in the output table
        /// </summary>
        [XmlArray]
        public string[] DisplayFormats { get; set; }

        /// <summary>
        /// List of string representations of the units for the columns in the output table
        /// </summary>
        [XmlArray]
        public string[] Units { get; set; }

        /// <summary>
        /// Section label that should be unique within a summary toc.
        /// </summary>
        public string SectionLabel { get; set; }

        /// <summary>
        /// Save section CSV data to the database as zipped CSV. If the section was saved previously, overwrites
        /// the file.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="sectionManager"></param>
        /// <returns>true of successfully saved.</returns>
        public bool SaveSummarySection(
            SummarySection section,
            ISectionManager sectionManager
        ) {
            if (section is CsvDataSummarySection dataSection) {
                sectionManager.SaveCsvDataSectionData(dataSection);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Save the section content to the CSV file as specified in the parameter.
        /// </summary>
        /// <param name="sectionManager"></param>
        /// <param name="filename">File to write to</param>
        public void SaveCsvFile(
            ISectionManager sectionManager,
            string filename
        ) {
            sectionManager.WriteCsvDataToFile(SectionId, filename);
        }

        /// <summary>
        /// Saves the data file in spreadsheet (Excel) format to the file name specified.
        /// </summary>
        /// <param name="sectionManager"></param>
        /// <param name="filename">Full path and name of file to write to</param>
        /// <param name="workDir">Optional working directory to create the temporary CSV file</param>
        public void SaveSpreadsheetFile(
            ISectionManager sectionManager,
            string fileName,
            string inputCsvFile = null,
            string workDir = null
        ) {
            if(string.IsNullOrEmpty(workDir) || !Directory.Exists(workDir)) {
                workDir = Path.GetTempPath();
            }
            var csvInputExists = !string.IsNullOrEmpty(inputCsvFile) && File.Exists(inputCsvFile);
            var csvFileName = csvInputExists
                ? inputCsvFile
                : Path.Combine(workDir, $"mcra-{Guid.NewGuid():N}.csv");

            if(!csvInputExists) {
                //write the data to CSV
                sectionManager.WriteCsvDataToFile(SectionId, csvFileName);
            }

            //tablename: remove ending [Data]Table from name
            //and crop to max 31 chars (excel limit)
            var tableName = Name;
            if (tableName.EndsWith("Table")) {
                tableName = tableName[..^5];
            }
            if (tableName.EndsWith("Data")) {
                tableName = tableName[..^4];
            }
            if (tableName.Length > 31) {
                tableName = tableName[..31];
            }

            var columnTypes = GetTypes();
            using var fs = File.OpenRead(csvFileName);
            using var rdr = new CsvDataReader(fs, fieldTypes: columnTypes);
            var colNames = rdr.GetColumnNames();

            //max excel tablename length
            var table = new DataTable(tableName);
            foreach (var (colName, colType) in rdr.GetColumnNames().Zip(columnTypes)) {
                var column = new DataColumn(colName, colType);
                table.Columns.Add(column);
            }
            table.Load(rdr);
            using (var xlWriter = new SpreadsheetDataSourceWriter(fileName)) {
                xlWriter.Write(table, tableName);
            }
            //delete original csv file if it was created temporarily
            if(!csvInputExists){
                File.Delete(csvFileName);
            }
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"{SectionId} - {Name} - {TitlePath}";
        }

        /// <summary>
        /// Gets the system field types of the header.
        /// </summary>
        /// <returns></returns>
        public Type[] GetTypes() {
            return ColumnTypes?
                .Select(r => _typeStringMappings.TryGetValue(r, out var val) ? val : typeof(string))
                .ToArray();
        }

        private static Dictionary<string, Type> _typeStringMappings =
            new(StringComparer.OrdinalIgnoreCase)
        {
            { "String", typeof(string) },
            { "Byte", typeof(short) },
            { "Int", typeof(int) },
            { "Int16", typeof(short) },
            { "Int32", typeof(int) },
            { "Int64", typeof(long) },
            { "Integer", typeof(int) },
            { "Double", typeof(double) },
            { "Float", typeof(float) },
            { "Decimal", typeof(decimal) },
            { "Boolean", typeof(bool) },
            { "Bool", typeof(bool) },
            { "DateTime", typeof(DateTime) }
        };
    }
}
