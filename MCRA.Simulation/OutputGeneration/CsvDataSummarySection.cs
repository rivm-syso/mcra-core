using MCRA.Utils.ExtensionMethods;
using System.Reflection;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Section containing CSV data format metadata and file description
    /// of the saved CSV data file, created in a temp directory by the table
    /// summarizer
    /// </summary>
    public class CsvDataSummarySection : SummarySection {

        public CsvDataSummarySection(
            string tableName,
            string csvFileName,
            string titlePath,
            IEnumerable<PropertyInfo> propertyInfos,
            IDictionary<string, string> unitsDictionary,
            string caption = null,
            TreeTableProperties treeTableProperties = null,
            string sectionLabel = null
        ) {
            TableName = tableName;
            CsvFileName = csvFileName;
            TitlePath = titlePath;
            Caption = caption;
            SectionLabel = sectionLabel;
            TreeTableProperties = treeTableProperties;

            PropertyInfos = propertyInfos ?? new List<PropertyInfo>();

            //fill units, this is a list containing a unit for
            //every property in the PropertyInfos list
            //if there is no unit, an empty string is stored.
            PropertyUnits = new List<string>();

            foreach(var p in PropertyInfos) {
                var displayName = p.GetShortName();
                var hasUnit = false;
                //find whether there are any unit definitions in this name
                if(unitsDictionary != null) {
                    foreach (var kvp in unitsDictionary) {
                        //check whether the key is found preceded by a '(', this is a unit
                        hasUnit = displayName.Contains($"({kvp.Key}");
                        if (hasUnit) {
                            PropertyUnits.Add(kvp.Value);
                            break;
                        }
                    }
                }
                if (!hasUnit) {
                    //add empty string
                    PropertyUnits.Add(string.Empty);
                }
            }
        }

        /// <summary>
        /// File name of the CSV file that is saved in the temp folder.
        /// </summary>
        public string CsvFileName { get; private set; }
        
        /// <summary>
        /// Name of the table.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// Caption of the table
        /// </summary>
        public string Caption { get ; }

        /// <summary>
        /// Path in the output table of contents tree to the section containing the table.
        /// </summary>
        public string TitlePath { get; private set; }
        
        /// <summary>
        /// Optional properties describing column names necessary to build a hierarchical table.
        /// </summary>
        public TreeTableProperties TreeTableProperties { get; private set; }

        /// <summary>
        /// List of units for each property.
        /// </summary>
        public IList<string> PropertyUnits { get; private set; }

        /// <summary>
        /// Properties enumerable containing the property information of the columns.
        /// </summary>
        public IEnumerable<PropertyInfo> PropertyInfos { get; private set; }

    }
}
