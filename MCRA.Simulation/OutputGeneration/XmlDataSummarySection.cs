namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Section containing CSV data format metadata and file description
    /// of the saved CSV data file, created in a temp directory by the table
    /// summarizer
    /// </summary>
    public class XmlDataSummarySection : SummarySection {

        public XmlDataSummarySection(
            string name,
            string xml,
            string titlePath,
            string sectionLabel
        ) {
            Name = name;
            Xml = xml;
            TitlePath = titlePath;
            SectionLabel = !string.IsNullOrEmpty(sectionLabel) ? $"{sectionLabel}:{name}" : null;
        }

        /// <summary>
        /// The xml.
        /// </summary>
        public string Xml { get; private set; }

        /// <summary>
        /// Name of the table.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Path in the output table of contents tree to the section containing the table.
        /// </summary>
        public string TitlePath { get; private set; }

    }
}
