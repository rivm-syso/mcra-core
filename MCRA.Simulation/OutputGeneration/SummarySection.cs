using MCRA.Utils.ProgressReporting;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class SummarySection {

        /// <summary>
        /// The section's unique identifier
        /// </summary>
        protected Guid _sectionId = Guid.NewGuid();

        [Display(AutoGenerateField = false)]
        public List<ActionSummaryUnitRecord> Units { get; set; }

        /// <summary>
        /// Return the id of the section as Guid
        /// </summary>
        [XmlIgnore, JsonIgnore]
        [Display(AutoGenerateField = false)]
        public Guid SectionGuid {
            get {
                return _sectionId;
            }
        }

        /// <summary>
        /// A guid string representing the id of the section.
        /// </summary>
        [Display(AutoGenerateField = false)]
        public string SectionId {
            get { return _sectionId.ToString(); }
            set { _sectionId = new Guid(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore, JsonIgnore]
        [Display(AutoGenerateField = false)]
        public CompositeProgressState ProgressState { get; set; }

        [XmlIgnore, JsonIgnore]
        [Display(AutoGenerateField = false)]
        public virtual bool SaveTemporaryData => false;

        [XmlIgnore, JsonIgnore]
        [Display(AutoGenerateField = false)]
        public string SectionLabel { get; set; }

        /// <summary>
        /// Keep a list of data sections which will contain the metadata and file location
        /// of temporary saved CSV files for this section that need to be saved in the database.
        /// </summary>
        [XmlIgnore, JsonIgnore]
        [Display(AutoGenerateField = false)]
        public IList<CsvDataSummarySection> DataSections { get; } = new List<CsvDataSummarySection>();

        /// <summary>
        /// Keep a list of xml data sections which will contain the metadata and file location
        /// of temporary saved xml for this section that need to be saved in the database.
        /// </summary>
        [XmlIgnore, JsonIgnore]
        [Display(AutoGenerateField = false)]
        public IList<XmlDataSummarySection> XmlDataSections { get; } = new List<XmlDataSummarySection>();

        /// <summary>
        /// Keep a list of chart sections which will contain the metadata and file location
        /// of temporary saved chart files for this section that need to be saved in the database.
        /// </summary>
        [XmlIgnore, JsonIgnore]
        [Display(AutoGenerateField = false)]
        public IList<ChartSummarySection> ChartSections { get; } = new List<ChartSummarySection>();

        /// <summary>
        /// Finds a section recursively and returns it, or return null if none was found.
        /// </summary>
        /// <param name="sectionName">the name to search for in the subsections</param>
        /// <returns>The section with the specified Guid (including self) or null if nothing was found.</returns>
        public SummarySection GetSectionRecursive(Guid sectionId) {
            if (_sectionId == sectionId) {
                return this;
            }

            var subSections = this.GetType().GetProperties()
                .Where(p => p.PropertyType.IsSubclassOf(typeof(SummarySection)))
                .Select(p => (
                    SummarySection: (SummarySection)p.GetValue(this),
                    PropertyInfo: p
                ))
                .Where(ssn => ssn.SummarySection != null)
                .ToList();

            foreach (var ssn in subSections) {
                var ssnInfo = ssn.SummarySection.GetSectionRecursive(sectionId);
                if (ssnInfo != null) {
                    ssnInfo.ProgressState = ProgressState;
                    return ssnInfo;
                }
            }

            return null;
        }
    }
}
