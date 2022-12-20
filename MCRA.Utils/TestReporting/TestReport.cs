using System.Xml;

namespace MCRA.Utils.TestReporting {

    public class TestReport {

        public string AssemblyName { get; private set; }
        public string TestDate { get; private set; }
        public List<TestReportSection> TestReportSections { get; private set; }

        /// <summary>
        /// Creates a new (empty) test report instance.
        /// </summary>
        public TestReport() {
            AssemblyName = "";
            TestReportSections = new List<TestReportSection>();
        }

        /// <summary>
        /// Reads in the report from a documentation xml file.
        /// </summary>
        /// <param name="filename"></param>
        public void ReadDocumentationXml(string filename) {
            var document = new XmlDocument();
            document.Load(filename);
            AssemblyName = document.SelectSingleNode("//doc/assembly//name").InnerText;
            var members = document.SelectNodes("//doc//members//member");
            foreach (XmlNode member in members) {
                var fullName = member.Attributes["name"].Value;
                if (fullName.Contains("ThisAssembly", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                var memberNamespace = fullName.Substring(AssemblyName.Length + 3);
                var summary = member.SelectSingleNode("summary").InnerXml.Replace("seealso", "a");
                var index = memberNamespace.IndexOf("(");
                if (index > 0) {
                    memberNamespace = memberNamespace.Substring(0, index);
                }
                var sectionPath = memberNamespace.Split('.').ToList();
                if (fullName.StartsWith("T:")) {
                    // Class found
                    if (sectionPath.ElementAt(sectionPath.Count - 1) == "NamespaceDoc") {
                        sectionPath.RemoveAt(sectionPath.Count - 1);
                    }
                    addSection(sectionPath, summary);
                } else if (fullName.StartsWith("M:")) {
                    // Method found
                    var recordName = sectionPath.Last();
                    sectionPath.RemoveAt(sectionPath.Count - 1);
                    addSectionRecord(sectionPath, recordName, summary);
                }
            }
        }

        /// <summary>
        /// Read in the test results from a trx file and adds them to the test report.
        /// </summary>
        /// <param name="filename"></param>
        public void ReadTestResultsTrx(string filename) {
            var reader = new TrxReader();
            var testResultSummary = reader.Read(filename);
            TestDate = testResultSummary.StartDate;
            foreach (var testResult in testResultSummary.TestResultSummaryRecords) {
                var fullPath = testResult.ClassName.Substring(AssemblyName.Length + 1);
                var path = fullPath.Split('.').ToList();
                var section = getOrCreateSection(path);
                var record = section.GetOrCreateSectionRecord(testResult.TestName);
                record.Outcome = testResult.Outcome;
                record.Duration = testResult.Duration;
            }
        }

        /// <summary>
        /// Adds a section with the specified section path to the document.
        /// </summary>
        /// <param name="sectionPath">A hierarchical list of the section names in the path,
        /// where the last item should contain the section name.</param>
        /// <param name="summary">The summary of the section that is to be added.</param>
        private void addSection(List<string> sectionPath, string summary) {
            var section = getOrCreateSection(sectionPath);
            section.Summary = summary;
        }

        /// <summary>
        /// Adds a section record to the document located as specified by the record path.
        /// </summary>
        /// <param name="sectionPath">A hierarchical list of the section names in the path.</param>
        /// <param name="name">The name of the record that is to be added.</param>
        /// <param name="description">The description of the record that is to be added.</param>
        private void addSectionRecord(List<string> sectionPath, string name, string description) {
            var section = getOrCreateSection(sectionPath);
            section.AddRecord(name, description);
        }

        /// <summary>
        /// Gets or creates the section identified by the given section path.
        /// </summary>
        /// <param name="sectionPath">A hierarchical list of the section names in the path.</param>
        /// <returns></returns>
        private TestReportSection getOrCreateSection(List<string> sectionPath) {
            var sectionName = sectionPath.First();
            TestReportSection section;
            if (!TestReportSections.Select(ds => ds.Name).Contains(sectionName)) {
                section = new TestReportSection() {
                    Name = sectionName,
                    Level = 0
                };
                TestReportSections.Add(section);
            } else {
                section = TestReportSections.Single(ds => ds.Name == sectionName);
            }
            if (sectionPath.Count > 1) {
                sectionPath.RemoveAt(0);
                return section.GetOrCreateSubSection(sectionPath);
            } else {
                return section;
            }
        }

    }

}
