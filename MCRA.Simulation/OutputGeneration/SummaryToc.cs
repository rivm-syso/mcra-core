using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Xml;
using MCRA.Simulation.OutputManagement;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace MCRA.Simulation.OutputGeneration {
    public class SummaryToc : SectionHeader {

        #region Constructors

        public SummaryToc() : base() { }

        public SummaryToc(ISectionManager sectionManager) : this() {
            SectionManager = sectionManager;
        }

        /// <summary>
        /// Static construction method to create an instance from a compressed XML stream
        /// </summary>
        /// <param name="data">Byte array containing the compressed XML for a SummaryToc</param>
        /// <param name="sectionManager">The section manager to use for saving/retrieving section data</param>
        /// <returns>A new instance of SummaryToc</returns>
        public static SummaryToc FromCompressedXml(byte[] data, ISectionManager sectionManager) {
            var toc = XmlSerialization.FromCompressedXml<SummaryToc>(data);
            toc.SectionManager = sectionManager;
            toc.InitializeHierarchy();
            return toc;
        }

        #endregion

        /// <summary>
        /// Return this instance which is the root TOC of a tree of section headers
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public override SummaryToc TocRoot => this;

        /// <summary>
        /// The SectionManager for this summary which is used to retrieve
        /// and save output sections with their header.
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public ISectionManager SectionManager { get; private set; }

        //use a hashset to cache saved data header ids, because
        //a dictionary of headers can't be serialized directly
        private HashSet<Guid> _dataHeaderIds;

        //use a hashset to cache saved xml data header ids, because
        //a dictionary of headers can't be serialized directly
        private HashSet<Guid> _xmlDataHeaderIds;

        //use a hashset to cache saved data header ids, because
        //a dictionary of headers can't be serialized directly
        private HashSet<Guid> _chartHeaderIds;

        //Use a list of data headers which can be serialized as XML
        [JsonIgnore]
        public List<CsvDataHeader> DataHeaders { get; set; } = new List<CsvDataHeader>();

        [JsonIgnore]
        public List<XmlDataHeader> XmlDataHeaders { get; set; } = new List<XmlDataHeader>();

        [JsonIgnore]
        public List<ChartHeader> ChartHeaders { get; set; } = new List<ChartHeader>();


        [XmlIgnore, JsonIgnore]
        public HashSet<string> DataHeaderLabels { get; set; } = new HashSet<string>();

        [XmlIgnore, JsonIgnore]
        public HashSet<string> XmlDataHeaderLabels { get; set; } = new HashSet<string>();

        [XmlIgnore, JsonIgnore]
        public HashSet<string> ChartHeaderLabels { get; set; } = new HashSet<string>();

        /// <summary>
        /// Counts the total of all headers in the hierarchy including this TOC
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public int AllHeadersCount => countRecursive();

        /// <summary>
        /// recursively set the parent - child hierarchy of the header structure
        /// (when needed after the summary toc is loaded from XML)
        /// </summary>
        public void InitializeHierarchy() => SetParentRecursive();

        /// <summary>
        /// Get all settings headers and recursively populate SettingsSectionId property
        /// for all other headers with the same title
        /// </summary>
        public void SetModuleSettingsLinks() {
            var settingsHdr = SubSectionHeaders.FirstOrDefault(h => h.SectionId == OutputConstants.ActionSettingsSectionGuid);
            if (settingsHdr != null) {
                //create a dictionary of sectionIds by settings section title
                var settingsLinkDict = settingsHdr.EnumerateAll().ToDictionary(h => h.Name, h => h.SectionId, StringComparer.OrdinalIgnoreCase);
                foreach (var mainHeader in SubSectionHeaders.Where(h => h.SectionId != OutputConstants.ActionSettingsSectionGuid)) {
                    // only check main sections and direct sub-sections; not deeper!
                    if (settingsLinkDict.TryGetValue(mainHeader.Name, out Guid settingsId)) {
                        mainHeader.SettingsSectionId = settingsId.ToString();
                    } else {
                        foreach (var header in mainHeader.SubSectionHeaders) {
                            if (settingsLinkDict.TryGetValue(header.Name, out settingsId)) {
                                header.SettingsSectionId = settingsId.ToString();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save all output recursively as zipped html in the database
        /// </summary>
        /// <param name="sectionManager"></param>
        /// <param name="state"></param>
        public override void SaveHtmlRecursive(
            ISectionManager sectionManager,
            CompositeProgressState state
        ) {
            SetModuleSettingsLinks();
            var localProgress = state.NewProgressState(SubSectionHeaders.Any() ? 1 : 100);
            if (SubSectionHeaders.Any()) {
                foreach (var header in SubSectionHeaders.OrderBy(h => h.Order)) {
                    var subState = state.NewCompositeState(99D / SubSectionHeaders.Count);
                    header.SaveHtmlRecursive(sectionManager, subState);
                    subState.MarkCompleted();
                }
            }
            localProgress.Update(100);
        }

        public CsvDataHeader GetDataHeader(Guid id) {
            return DataHeaders.FirstOrDefault(d => d.SectionId == id);
        }

        public ChartHeader GetChartHeader(Guid id) {
            return ChartHeaders.FirstOrDefault(d => d.SectionId == id);
        }

        public XmlDataHeader GetXmlDataHeader(Guid id) {
            return XmlDataHeaders.FirstOrDefault(d => d.SectionId == id);
        }

        public CsvDataHeader GetDataHeader(string sectionLabel) {
            return DataHeaders.FirstOrDefault(d => d.SectionLabel == sectionLabel);
        }

        public ChartHeader GetChartHeader(string sectionLabel) {
            return ChartHeaders.FirstOrDefault(d => d.SectionLabel == sectionLabel);
        }

        public XmlDataHeader GetXmlDataHeader(string sectionLabel) {
            return XmlDataHeaders.FirstOrDefault(d => d.SectionLabel == sectionLabel);
        }

        public void SaveChartSection(
            IEnumerable<ChartSummarySection> chartSections,
            ISectionManager sectionManager
        ) {
            if (_chartHeaderIds == null) {
                _chartHeaderIds = new HashSet<Guid>();
            }
            foreach (var ds in chartSections) {
                if (_chartHeaderIds.Contains(ds.SectionGuid)) {
                    continue;
                }
                var chartHeader = new ChartHeader() {
                    Name = ds.ChartName,
                    FileExtension = ds.ChartFileExtension,
                    TitlePath = ds.TitlePath,
                    SectionId = ds.SectionGuid,
                    Caption = ds.Caption
                };
                _chartHeaderIds.Add(ds.SectionGuid);
                ChartHeaders.Add(chartHeader);
                if (!string.IsNullOrEmpty(ds.SectionLabel) && !ChartHeaderLabels.Add(ds.SectionLabel)) {
                    throw new Exception($"This chart: {ds.SectionLabel} is already added, path: {ds.TitlePath}");
                };
                //save to database
                chartHeader.SaveSummarySection(ds, sectionManager);
            }
        }

        public void SaveCsvDataSection(
            IEnumerable<CsvDataSummarySection> dataSections,
            ISectionManager sectionManager
        ) {
            if (_dataHeaderIds == null) {
                _dataHeaderIds = new HashSet<Guid>();
            }
            foreach (var ds in dataSections) {
                if (_dataHeaderIds.Contains(ds.SectionGuid)) {
                    continue;
                }
                var csvHeader = new CsvDataHeader() {
                    Name = ds.TableName,
                    TitlePath = ds.TitlePath,
                    SectionId = ds.SectionGuid,
                    Caption = ds.Caption,
                    SectionLabel = ds.SectionLabel,
                    TreeTableProperties = ds.TreeTableProperties,
                    //check whether there are any nullable types: if so, use the underlying type's name
                    ColumnTypes = ds.PropertyInfos
                        .Select(p => Nullable.GetUnderlyingType(p.PropertyType)?.Name ?? p.PropertyType.Name)
                        .ToArray(),
                    DisplayFormats = ds.PropertyInfos
                        .Select(p => p.GetAttribute<DisplayFormatAttribute>(false)?.DataFormatString ?? string.Empty)
                        .ToArray(),
                    Units = ds.PropertyUnits.ToArray(),
                };
                _dataHeaderIds.Add(ds.SectionGuid);
                DataHeaders.Add(csvHeader);
                if (!string.IsNullOrEmpty(ds.SectionLabel) &&  !DataHeaderLabels.Add(ds.SectionLabel)) {
                    throw new Exception($"This table: {ds.SectionLabel} is already added, path: {ds.TitlePath}");
                };

                //save to database
                csvHeader.SaveSummarySection(ds, sectionManager);
            }
        }

        public void SaveXmlDataSection(
            IEnumerable<XmlDataSummarySection> dataSections,
            ISectionManager sectionManager
        ) {
            if (_xmlDataHeaderIds == null) {
                _xmlDataHeaderIds = new HashSet<Guid>();
            }
            foreach (var ds in dataSections) {
                if (_xmlDataHeaderIds.Contains(ds.SectionGuid)) {
                    continue;
                }
                var xmlHeader = new XmlDataHeader() {
                    SectionId = ds.SectionGuid,
                    Name = ds.Name,
                    TitlePath = ds.TitlePath,

                };
                _xmlDataHeaderIds.Add(ds.SectionGuid);
                XmlDataHeaders.Add(xmlHeader);
                if (!string.IsNullOrEmpty(ds.SectionLabel) && !XmlDataHeaderLabels.Add(ds.SectionLabel)) {
                    throw new Exception($"This xml: {ds.SectionLabel} is already added, path: {ds.TitlePath}");
                };
                //save to database
                xmlHeader.SaveSummarySection(ds, sectionManager);
            }
        }

        /// <summary>
        /// Save tables in section data as CSV file(s) recursively.
        /// </summary>
        /// <param name="dataDir">DirectoryInfo instance for where the files are saved</param>
        /// <param name="sectionManager"></param>
        /// <param name="tableFiles">Dictionary of string-string where the key is the name of the table and the value
        /// <param name="dataSectionIds">Hashset of ids of data sections to save the table data for</param>
        /// the title path in the sections hierarchy</param>
        /// <returns>Dictionary of filenames and the path of header titles to the table in the output hierarchy</returns>
        public IDictionary<Guid, (string FileName, string TitlePath)> SaveTablesAsCsv(
            DirectoryInfo dataDir,
            ISectionManager sectionManager,
            IDictionary<Guid, (string, string)> tableFiles = null,
            HashSet<Guid> dataSectionIds = null
        ) {
            tableFiles ??= new Dictionary<Guid, (string, string)>();
            var csvDataHeaders = DataHeaders
                .Where(d => dataSectionIds?.Contains(d.SectionId) ?? true)
                .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(d => d.TitlePath, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var tempDataDir = dataDir.FullName;
            Directory.CreateDirectory(tempDataDir);
            foreach (var dh in csvDataHeaders) {
                var csvName = $"{dh.Name}.csv";
                var fileName = Path.Combine(tempDataDir, csvName);
                //TODO: solution for duplicate output csv file names, make them unique
                var i = 1;
                while (File.Exists(fileName)) {
                    csvName = $"{dh.Name}-{i++}.csv";
                    fileName = Path.Combine(tempDataDir, csvName);
                }
                tableFiles.Add(dh.SectionId, (csvName, dh.TitlePath));
                dh.SaveCsvFile(sectionManager, fileName);
            }
            return tableFiles;
        }

        /// <summary>
        /// Save all sections recursively using the provided SectionManager for this instance
        /// </summary>
        /// <param name="state"></param>
        public void SaveSummarySectionsRecursive(CompositeProgressState state) {
            SaveHtmlRecursive(SectionManager, state);
        }

        /// <summary>
        /// Save charts of the chart sections recursively.
        /// </summary>
        /// <param name="chartsDir">DirectoryInfo instance for where the files are saved</param>
        /// <param name="sectionManager"></param>
        /// <param name="chartFiles">Dictionary of string-string where the key is the name of the table and the value
        /// <param name="chartHeaderIds">Hashset of ids of data sections to save the table data for</param>
        /// the title path in the sections hierarchy</param>
        /// <returns>Dictionary of filenames and the path of header titles to the table in the output hierarchy</returns>
        public IDictionary<Guid, (string, string)> SaveChartFiles(
            DirectoryInfo chartsDir,
            ISectionManager sectionManager,
            IDictionary<Guid, (string, string)> chartFiles = null,
            HashSet<Guid> chartHeaderIds = null
        ) {
            chartFiles ??= new Dictionary<Guid, (string, string)>();

            var selectedChartHeaders = ChartHeaders
                .Where(d => chartHeaderIds?.Contains(d.SectionId) ?? true)
                .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(d => d.TitlePath, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var tempImageDir = chartsDir.FullName;
            Directory.CreateDirectory(tempImageDir);
            foreach (var chartHeader in selectedChartHeaders) {
                var chartFileName = $"{chartHeader.Name}.{chartHeader.FileExtension}";
                var fileName = Path.Combine(tempImageDir, chartFileName);
                //TODO: solution for duplicate chart file names, make them unique
                var i = 1;
                while (File.Exists(fileName)) {
                    chartFileName = $"{chartHeader.Name}-{i++}.{chartHeader.FileExtension}";
                    fileName = Path.Combine(tempImageDir, chartFileName);
                }
                chartFiles.Add(chartHeader.SectionId, (chartFileName, chartHeader.TitlePath));
                chartHeader.SaveChartFile(sectionManager, fileName);
            }
            return chartFiles;
        }



        /// <summary>
        /// Writes the summary toc to a comma separated text file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="ext">The extension for the file (e.g. "csv" or "txt")</param>
        public void WriteHeadersToFiles(string path, string ext = "csv") {
            //the tables consist of the following columns, comma separated
            //this is written as the first line in the exported text file
            const string header = "Title,TitlePath,SectionLabel,SectionId";

            using (var sw = new StreamWriter(Path.Combine(path, $"TOC.{ext}"))) {
                sw.WriteLine(header);
                foreach (var subHeader in SubSectionHeaders) {
                    writeToCsvRecursive(sw, subHeader);
                }
            }

            //Write the other exported files
            var entries = new[] {
                ("TOC-CsvData", DataHeaders.Cast<IHeader>()),
                ("TOC-Charts", ChartHeaders.Cast<IHeader>()),
                ("TOC-XmlData", XmlDataHeaders.Cast<IHeader>()),
            };

            foreach (var (prefix, list) in entries) {
                using (var sw = new StreamWriter(Path.Combine(path, $"{prefix}.{ext}"))) {
                    sw.WriteLine(header);
                    foreach (var dataHeader in list.OrderBy(v => v.TitlePath, StringComparer.OrdinalIgnoreCase).ThenBy(v => v.Name, StringComparer.OrdinalIgnoreCase)) {
                        var values = new[] {
                            dataHeader.Name,
                            dataHeader.TitlePath,
                            dataHeader.SectionLabel,
                            dataHeader.SectionId.ToString()
                        };
                        sw.WriteLine(string.Join(",", values.Select(r => $"\"{r}\"")));
                    }
                }
            }
        }

        private void writeToCsvRecursive(StreamWriter sw, SectionHeader header) {
            var values = new[] {
                header.Name,
                header.TitlePath,
                header.SectionLabel,
                header.SectionId.ToString()
            };
            sw.WriteLine(string.Join(",", values.Select(r => $"\"{r}\"")));
            foreach (var subHeader in header.SubSectionHeaders) {
                writeToCsvRecursive(sw, subHeader);
            }
        }
    }
}
