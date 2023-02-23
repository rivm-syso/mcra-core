using MCRA.Simulation.OutputManagement;
using System.Xml.Serialization;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Special header type which stores a zipped CSV file contents
    /// with its intended visual data representation in the output tables
    /// in HTML of PDF rendered output
    /// </summary>
    public class XmlDataHeader : IHeader {

        public XmlDataHeader() { }

        /// <summary>
        /// Section id.
        /// </summary>
        public Guid SectionId { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [XmlElement("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Hierarchical section header title path to the
        /// section containing this table
        /// </summary>
        public string TitlePath { get; set; }

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
            if (section is XmlDataSummarySection dataSection) {
                sectionManager.SaveXmlDataSectionData(dataSection);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Save the section content to the CSV file as specified in the parameter.
        /// </summary>
        /// <param name="sectionManager"></param>
        /// <param name="filename">File to write to</param>
        public void SaveXmlFile(
            ISectionManager sectionManager,
            string filename
        ) {
            sectionManager.WriteXmlDataToFile(SectionId, filename);
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
