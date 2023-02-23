using MCRA.Simulation.OutputManagement;
using System.Xml.Serialization;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Special header type which stores a zipped CSV file contents
    /// with its intended visual data representation in the output tables
    /// in HTML of PDF rendered output
    /// </summary>
    public class ChartHeader : IHeader {

        public ChartHeader() { }

        /// <summary>
        /// Section id.
        /// </summary>
        public Guid SectionId { get; set; }

        /// <summary>
        /// Hierarchical section header title path to the
        /// section containing this table
        /// </summary>
        public string TitlePath { get; set; }

        /// <summary>
        /// Chart name.
        /// </summary>
        [XmlElement("ChartName")]
        public string Name { get; set; }

        /// <summary>
        /// Chart caption
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Chart file type/extension.
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// The filename of the chart as it should be when downloaded.
        /// </summary>
        public string ChartFileName {
            get {
                return $"{Name}.{FileExtension}";
            }
        }

        public string SectionLabel { get; set; }

        /// <summary>
        /// Save section chart file to the database (zipped). If the section was saved previously, overwrites
        /// the file.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="sectionManager"></param>
        /// <returns>true of successfully saved.</returns>
        public bool SaveSummarySection(
            SummarySection section,
            ISectionManager sectionManager
        ) {
            if (section is ChartSummarySection chartSection) {
                sectionManager.SaveChartSectionChart(chartSection);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Save the section content to the chart file as specified in the parameter.
        /// </summary>
        /// <param name="sectionManager"></param>
        /// <param name="filename">File to write to.</param>
        public void SaveChartFile(
            ISectionManager sectionManager,
            string filename
        ) {
            sectionManager.WriteChartDataToFile(SectionId, filename);
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"{SectionId} - {Name} - {TitlePath}";
        }
    }
}
