using MCRA.Utils.ProgressReporting;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputManagement;
using System.Text.Json.Serialization;
using System.Reflection;
using System.Xml.Serialization;

namespace MCRA.Simulation.OutputGeneration {
    public class SectionHeader : IHeader {

        [XmlIgnore, JsonIgnore]
        protected int _outputId;
        private string _sectionTypeName;
        private Type _sectionType = typeof(SectionHeader);
        private SectionHeader _parentHeader;
        private SummarySection _summarySection;
        private string _html;
        /// <summary>
        /// Flag to signal whether this header represents an empty section header.
        /// </summary>
        private bool _isEmptySectionHeader = false;

        /// <summary>
        /// Save as temporary file.
        /// </summary>
        private bool _saveTemporaryData = false;

        /// <summary>
        /// Section id.
        /// </summary>
        public Guid SectionId { get; set; }

        /// <summary>
        /// Reference to the settings section,
        /// only applies to action type module headers
        /// </summary>
        public string SettingsSectionId { get; set; }

        /// <summary>
        /// The section display order number.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// The section title.
        /// </summary>
        [XmlElement("Title"), JsonPropertyName("title")]
        public string Name { get; set; }

        /// <summary>
        /// Section label that should be unique within a summary toc.
        /// </summary>
        public string SectionLabel { get; set; }

        /// <summary>
        /// The section title including any parent(s)
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public string TitlePath {
            get {
                var parentTitle = _parentHeader?.TitlePath;
                return string.IsNullOrWhiteSpace(parentTitle) ? Name : $"{parentTitle} | {Name}";
            }
            set { }
        }

        /// <summary>
        /// The name for the summary section this header points to, defaults to the
        /// type name, is used to find the partial view for the summary section.
        /// </summary>
        public string SummarySectionName { get; set; }

        /// <summary>
        /// A list of the section headers of the sub sections.
        /// </summary>
        public List<SectionHeader> SubSectionHeaders { get; set; } = new List<SectionHeader>();

        /// <summary>
        /// Retrieves a subsection header based on a path given as a stack
        /// </summary>
        /// <param name="path">the path to find as a stack</param>
        /// <returns>The found section header or null if the path does not match a header</returns>
        public SectionHeader GetSubSectionHeaderByTitlePath(params string[] path) {
            if (path != null) {
                return getSubSectionHeaderByPathQueue(new Queue<string>(path));
            }
            return null;
        }

        /// <summary>
        /// Flag to signal whether this header's section is serialized as a separate object
        /// if false it is a part (subsection) of a composite section.
        /// </summary>
        public bool HasSectionData { get; set; }

        /// <summary>
        /// Ids of data sections (if any) for this header's section.
        /// </summary>
        [JsonIgnore]
        public HashSet<Guid> DataSectionIds { get; set; }

        /// <summary>
        /// Ids of xml data sections (if any) for this header's section.
        /// </summary>
        [JsonIgnore]
        public HashSet<Guid> XmlDataSectionIds { get; set; }

        /// <summary>
        /// Ids of chart sections (if any) for this header's section.
        /// </summary>
        [JsonIgnore]
        public HashSet<Guid> ChartSectionIds { get; set; }

        /// <summary>
        /// Return the root object, the summary table of contents.
        /// </summary>
        /// <returns></returns>
        [XmlIgnore, JsonIgnore]
        public virtual SummaryToc TocRoot => _parentHeader?.TocRoot;

        [XmlIgnore, JsonIgnore]
        public int? Depth { get; set; }

        [XmlIgnore, JsonIgnore]
        public int SectionHash { get; private set; }

        [XmlIgnore, JsonIgnore]
        public List<ActionSummaryUnitRecord> Units { get; set; }

        /// <summary>
        /// The name of the section type.
        /// </summary>
        [JsonIgnore]
        public string SectionTypeName {
            get {
                return _sectionTypeName;
            }
            set {
                _sectionTypeName = value;
                _sectionType = Assembly.GetExecutingAssembly().GetType(_sectionTypeName);
            }
        }

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public SectionHeader() { }

        #endregion

        public SectionHeader AddEmptySubSectionHeader(
            string title,
            int order,
            string sectionLabel = null,
            Guid? sectionId = null
        ) {
            var id = sectionId ?? Guid.NewGuid();
            var subHeader = new SectionHeader {
                SectionId = id,
                Name = title,
                SectionLabel = sectionLabel,
                Order = order,
                HasSectionData = false,
                SectionHash = SectionHash + 19 * (title?.GetHashCode() ?? 1),
                _parentHeader = this,
                _isEmptySectionHeader = true
            };

            SubSectionHeaders.Add(subHeader);
            return subHeader;
        }

        public SectionHeader AddSubSectionHeaderFor(
            SummarySection section,
            string title,
            int order
        ) {
            if (section == null) {
                return null;
            }
            var subHeader = new SectionHeader {
                SectionId = new Guid(section.SectionId),
                Name = title,
                SectionLabel = section.SectionLabel,
                Order = order,
                SectionTypeName = section.GetType().FullName,
                SummarySectionName = section.GetType().Name,
                SectionHash = SectionHash + 19 * (title?.GetHashCode() ?? 1),
                _parentHeader = this
            };
            if (TocRoot?.SectionManager == null) {
                // keep object reference to section when there is no
                // section manager or no root Table of contents
                subHeader._summarySection = section;
            }

            SubSectionHeaders.Add(subHeader);
            return subHeader;
        }

        /// <summary>
        /// Recursively tries to get the sub-section header with the specified section id.
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public SectionHeader GetSubSectionHeader(Guid sectionId) {
            if (SectionId == sectionId) {
                return this;
            }

            Func<List<SectionHeader>, SectionHeader> getSubHeader = null;
            getSubHeader = (headers) => {
                var header = headers.FirstOrDefault(sh => sh.SectionId == sectionId);

                if (header == null) {
                    foreach (var hdr in headers.OrderBy(h => h.Order)) {
                        header = getSubHeader(hdr.SubSectionHeaders);
                        if (header != null) {
                            return header;
                        }
                    }
                }

                return header;
            };

            // Retrieve the subsectionheader from anywhere in the tree (recursive)
            var subHeader = getSubHeader(SubSectionHeaders);
            return subHeader;
        }

        /// <summary>
        /// Recursively tries to get the sub-section header with the specified section id.
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public SectionHeader GetSubSectionHeader(string sectionId) => GetSubSectionHeader(new Guid(sectionId));



        /// <summary>
        /// Recursively tries to get the sub-section header with the specified section label.
        /// </summary>
        /// <param name="sectionLabel"></param>
        /// <returns></returns>
        public SectionHeader GetSubSectionHeaderBySectionLabel(string sectionLabel) {
            if (SectionLabel == sectionLabel) {
                return this;
            }

            Func<List<SectionHeader>, SectionHeader> getSubHeader = null;
            getSubHeader = (headers) => {
                var header = headers.FirstOrDefault(sh => sh.SectionLabel == sectionLabel);

                if (header == null) {
                    foreach (var hdr in headers.OrderBy(h => h.Order)) {
                        header = getSubHeader(hdr.SubSectionHeaders);
                        if (header != null) {
                            return header;
                        }
                    }
                }

                return header;
            };

            // Retrieve the subsectionheader from anywhere in the tree (recursive)
            var subHeader = getSubHeader(SubSectionHeaders);
            return subHeader;
        }



        /// <summary>
        /// Gets a sub-section header with the generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public SectionHeader GetSubSectionHeader<T>() where T : SummarySection {
            if (typeof(T).IsAssignableFrom(_sectionType)) {
                return this;
            }

            Func<List<SectionHeader>, SectionHeader> getSubHeader = null;
            getSubHeader = (headers) => {
                var header = headers.FirstOrDefault(sh => typeof(T).IsAssignableFrom(sh._sectionType));
                if (header == null) {
                    foreach (var hdr in headers.OrderBy(h => h.Order)) {
                        header = getSubHeader(hdr.SubSectionHeaders);
                        if (header != null) {
                            return header;
                        }
                    }
                }
                return header;
            };

            // Retrieve the subsectionheader from anywhere in the tree (recursive)
            var subHeader = getSubHeader(SubSectionHeaders);
            return subHeader;
        }

        /// <summary>
        /// Get a sub-section header with the specified title.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <returns></returns>
        public SectionHeader GetSubSectionHeaderByLabel<T>(string title) where T : SummarySection {
            if (typeof(T).IsAssignableFrom(_sectionType)
                && SectionLabel.Equals(title, StringComparison.OrdinalIgnoreCase)
            ) {
                return this;
            }

            Func<List<SectionHeader>, SectionHeader> getSubHeader = null;
            getSubHeader = (headers) => {
                var header = headers
                .FirstOrDefault(sh => typeof(T).IsAssignableFrom(sh._sectionType)
                    && sh.SectionLabel.Equals(title, StringComparison.OrdinalIgnoreCase));
                if (header == null) {
                    foreach (var hdr in headers.OrderBy(h => h.Order)) {
                        header = getSubHeader(hdr.SubSectionHeaders);
                        if (header != null) {
                            return header;
                        }
                    }
                }
                return header;
            };

            // Retrieve the subsectionheader from anywhere in the tree (recursive)
            var subHeader = getSubHeader(SubSectionHeaders);
            return subHeader;
        }

        /// <summary>
        /// Get a sub-section header with the specified title.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <returns></returns>
        public SectionHeader GetSubSectionHeaderFromTitleString<T>(string title) where T : SummarySection {
            if (typeof(T).IsAssignableFrom(_sectionType) && Name.Contains(title)) {
                return this;
            }

            Func<List<SectionHeader>, SectionHeader> getSubHeader = null;
            getSubHeader = (headers) => {
                var header = headers.FirstOrDefault(sh => typeof(T).IsAssignableFrom(sh._sectionType)
                    && sh.Name.Contains(title));
                if (header == null) {
                    foreach (var hdr in headers.OrderBy(h => h.Order)) {
                        header = getSubHeader(hdr.SubSectionHeaders);
                        if (header != null) {
                            return header;
                        }
                    }
                }
                return header;
            };

            // Retrieve the subsectionheader from anywhere in the tree (recursive)
            var subHeader = getSubHeader(SubSectionHeaders);
            return subHeader;
        }

        /// <summary>
        /// Gets an enumerator for a flattened collection of all subsection headers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SectionHeader> EnumerateAll() {
            var stack = new Stack<SectionHeader>();
            stack.Push(this);
            while (stack.Count > 0) {
                var current = stack.Pop();
                yield return current;
                foreach (var h in current.SubSectionHeaders.OrderBy(h => h.Order)) {
                    stack.Push(h);
                }
            }
        }

        /// <summary>
        /// Retrieve typed section from the database.
        /// </summary>
        /// <returns>The specific section this header refers to.</returns>
        public SummarySection GetSummarySection() {
            try {
                if (_summarySection == null) {
                    if (HasSectionData) {
                        _summarySection = TocRoot?.SectionManager?.LoadSection(SectionId.ToString(), _sectionType);
                    } else if (!_isEmptySectionHeader) {
                        //this section does not contain section data, so retrieve the parent section that does.
                        var parentSection = _parentHeader?.GetSummarySection();
                        //get the specific section by recursively searching for the subsection by id
                        _summarySection = parentSection?.GetSectionRecursive(SectionId);
                    }
                }
                return _summarySection;
            } catch (Exception) {
                throw new Exception($"Xml source data type '{SectionTypeName}' could not be deserialized.");
            }
        }

        /// <summary>
        /// Retrieve typed subsection from this section from the database.
        /// </summary>
        /// <returns>The specific section this header refers to.</returns>
        public T GetSubSummarySection<T>() where T : SummarySection {
            try {
                var subHeader = GetSubSectionHeader<T>();
                return (T)subHeader?.GetSummarySection();
            } catch (Exception) {
                throw new Exception($"Xml source data type '{SectionTypeName}' could not be deserialized.");
            }
        }

        /// <summary>
        /// Retrieve subsection from this section from the database based on sectionid
        /// </summary>
        /// <param name="subSectionId"></param>
        /// <returns>The specific section this header refers to.</returns>
        public SummarySection GetSubSummarySection(string subSectionId) {
            try {
                var subHeader = GetSubSectionHeader(subSectionId);
                return subHeader?.GetSummarySection();
            } catch (Exception) {
                throw new Exception($"Xml source data type '{SectionTypeName}' could not be deserialized.");
            }
        }

        /// <summary>
        /// Save section data as compressed XML to the temp folder.
        /// If the section was saved previously, overwrites the file.
        /// </summary>
        /// <param name="section"></param>
        /// <returns>true of successfully saved.</returns>
        public virtual bool SaveSummarySection(SummarySection section) {
            if (section == null || !section.GetType().FullName.Equals(SectionTypeName)) {
                return false;
            }

            HasSectionData = true;
            _saveTemporaryData = section.SaveTemporaryData;

            var manager = TocRoot?.SectionManager;
            if (manager == null) {
                _summarySection = section;
            } else {
                if (_saveTemporaryData) {
                    manager.SaveSection(section);
                } else {
                    saveRenderedHtml(section, manager);
                }
            }

            return true;
        }

        /// <summary>
        /// Retrieve section html from the database.
        /// </summary>
        /// <returns>The specific section this header refers to.</returns>
        public string GetSummarySectionHtml() {
            try {
                if (HasSectionData && _html == null) {
                    var sectionManager = TocRoot?.SectionManager;
                    //get the section html from the section manager
                    _html = sectionManager?.GetSectionHtml(SectionId);
                    //if it is empty try rendering it from the saved section data (if available)
                    if (string.IsNullOrEmpty(_html)) {
                        var section = sectionManager?.LoadSection(SectionId.ToString(), _sectionType);
                        if (section != null) {
                            _html = SectionHelper.RenderSingleSection(this, section);
                        }
                    }
                }
                return _html;
            } catch (Exception) {
                throw new Exception($"Html for section '{SectionTypeName}' could not be constructed.");
            }
        }

        /// <summary>
        /// Save all output recursively as zipped html in the database.
        /// </summary>
        public virtual void SaveHtmlRecursive(ISectionManager sectionManager, CompositeProgressState state) {
            var localProgress = state.NewProgressState(SubSectionHeaders.Any() ? 1 : 100);
            localProgress.Update($"Rendering section '{Name}'");
            if (_saveTemporaryData) {
                var section = GetSummarySection();
                saveRenderedHtml(section, sectionManager);
            }
            localProgress.Update(100);
            if (SubSectionHeaders.Any()) {
                foreach (var header in SubSectionHeaders) {
                    var subState = state.NewCompositeState(99D / SubSectionHeaders.Count);
                    header.SaveHtmlRecursive(sectionManager, subState);
                    subState.MarkCompleted();
                }
            }
        }

        /// <summary>
        /// Get all data section ids of this header and all subsectionheaders, and add them to
        /// the input HashSet.
        /// </summary>
        /// <param name="dataSectionIds"></param>
        public void GetDataSectionIdsRecursive(HashSet<Guid> dataSectionIds) {
            if (dataSectionIds == null) {
                return;
            }

            foreach (var id in DataSectionIds) {
                dataSectionIds.Add(id);
            }
            foreach (var h in SubSectionHeaders) {
                h.GetDataSectionIdsRecursive(dataSectionIds);
            }
        }

        /// <summary>
        /// Get all xml data section ids of this header and all subsectionheaders, and add them to
        /// the input HashSet.
        /// </summary>
        /// <param name="xmlDataSectionIds"></param>
        public void GetXmlDataSectionIdsRecursive(HashSet<Guid> xmlDataSectionIds) {
            if (xmlDataSectionIds == null) {
                return;
            }

            foreach (var id in DataSectionIds) {
                xmlDataSectionIds.Add(id);
            }
            foreach (var hdr in SubSectionHeaders) {
                hdr.GetXmlDataSectionIdsRecursive(xmlDataSectionIds);
            }
        }

        /// <summary>
        /// Get all chart section ids of this header and all subsectionheaders
        /// and add them to the input HashSet.
        /// </summary>
        /// <param name="chartSectionIds"></param>
        public void GetChartSectionIdsRecursive(HashSet<Guid> chartSectionIds) {
            if (chartSectionIds == null) {
                return;
            }

            foreach (var id in ChartSectionIds) {
                chartSectionIds.Add(id);
            }
            foreach (var h in SubSectionHeaders) {
                h.GetChartSectionIdsRecursive(chartSectionIds);
            }
        }

        public Dictionary<string, string> GetUnitsDictionary() {
            if (Units?.Any() ?? false) {
                return Units.ToDictionary(u => u.Type, u => u.Unit);
            }
            return _parentHeader?.GetUnitsDictionary() ?? new Dictionary<string, string>();
        }

        public override string ToString() {
            return $"{SummarySectionName}: {Name}";
        }

        /// <summary>
        /// Set the parent of the subsectionheaders to this instance, also set the hash codes for the 
        /// section headers: these are used to identify a specific header in the html-rendered tree
        /// These hash codes should be identical for similar trees (side-by-side display)
        /// </summary>
        protected void SetParentRecursive() {
            // Set the section hash code: use the parent hash also, if available
            SectionHash = (_parentHeader?.SectionHash ?? 0) + 19 * (Name?.GetHashCode() ?? 1);
            if (SubSectionHeaders != null) {
                foreach (var h in SubSectionHeaders) {
                    h._parentHeader = this;
                    h._outputId = _outputId;
                    h.SetParentRecursive();
                }
            }
        }

        private SectionHeader getSubSectionHeaderByPathQueue(Queue<string> path) {
            SectionHeader header = null;
            if (path.Count > 0) {
                var title = path.Dequeue();
                header = SubSectionHeaders
                    .OrderBy(h => h.Order)
                    .FirstOrDefault(h => h.Name.Equals(title, StringComparison.OrdinalIgnoreCase));

                if (header != null && path.Count > 0) {
                    header = header.getSubSectionHeaderByPathQueue(path);
                }
            }
            return header;
        }

        private void saveRenderedHtml(SummarySection section, ISectionManager sectionManager) {
            var sectionHtml = SectionHelper.RenderSingleSection(this, section);

            // Write section html
            sectionManager.SaveSummarySectionHtml(section, sectionHtml);

            // Also save the temporary CSV data from the data sections (if any)
            // in the summary toc (root of the header tree)
            if (TocRoot is SummaryToc toc) {
                // Save csv data sections
                if (section.DataSections.Any()) {
                    DataSectionIds = section.DataSections.Select(s => s.SectionGuid).ToHashSet();
                    toc.SaveCsvDataSection(section.DataSections, sectionManager);
                }
                // Save xml data sections
                if (section.XmlDataSections.Any()) {
                    XmlDataSectionIds = section.XmlDataSections.Select(s => s.SectionGuid).ToHashSet();
                    toc.SaveXmlDataSection(section.XmlDataSections, sectionManager);
                }
                // Save chart sections
                if (section.ChartSections.Any()) {
                    ChartSectionIds = section.ChartSections.Select(s => s.SectionGuid).ToHashSet();
                    toc.SaveChartSection(section.ChartSections, sectionManager);
                }
            }
        }

        protected int countRecursive() {
            return 1 + SubSectionHeaders.Sum(h => h.countRecursive());
        }
    }
}
