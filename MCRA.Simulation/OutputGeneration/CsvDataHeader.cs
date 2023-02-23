using MCRA.Simulation.OutputManagement;
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
            new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
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
