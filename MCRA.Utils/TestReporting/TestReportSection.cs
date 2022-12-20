using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRA.Utils.TestReporting {
    public sealed class TestReportSection {

        /// <summary>
        /// Gets/sets the section name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets/sets the section summary text.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets/sets the section level (paragraph depth).
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets the section's test records.
        /// </summary>
        public List<TestReportSectionRecord> SectionRecords { get; private set; }

        /// <summary>
        /// Gets the section's sub-sections.
        /// </summary>
        public List<TestReportSection> SubSections { get; private set; }

        /// <summary>
        /// Creates a new instance of a test report section.
        /// </summary>
        public TestReportSection() {
            SectionRecords = new List<TestReportSectionRecord>();
            SubSections = new List<TestReportSection>();
        }

        /// <summary>
        /// Adds a record to this section with the given name and description.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public void AddRecord(string name, string description) {
            var record = GetOrCreateSectionRecord(name);
            record.Description = description;
        }

        /// <summary>
        /// Gets the sub-section (recursively) with the given section path or creates
        /// the subsection (possibly a tree of subsections at multiple depths) if it
        /// doesn't exist.
        /// </summary>
        /// <param name="sectionPath"></param>
        /// <returns></returns>
        public TestReportSection GetOrCreateSubSection(List<string> sectionPath) {
            var sectionName = sectionPath.First();
            TestReportSection section;
            if (!SubSections.Select(ds => ds.Name).Contains(sectionName)) {
                section = new TestReportSection() {
                    Name = sectionName,
                    Level = this.Level + 1
                };
                SubSections.Add(section);
            } else {
                section = SubSections.Single(ds => ds.Name == sectionName);
            }
            if (sectionPath.Count > 1) {
                sectionPath.RemoveAt(0);
                return section.GetOrCreateSubSection(sectionPath);
            } else {
                return section;
            }
        }

        /// <summary>
        /// Gets the section record with the provided name, or creates this section
        /// record if it doesn't exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TestReportSectionRecord GetOrCreateSectionRecord(string name) {
            if (!SectionRecords.Select(ds => ds.Name).Contains(name)) {
                var record = new TestReportSectionRecord() {
                    Name = name
                };
                SectionRecords.Add(record);
                return record;
            } else {
                return SectionRecords.Single(ds => ds.Name == name);
            }
        }

        /// <summary>
        /// Checks whether there is any record with an outcome that is not null
        /// or empty. If recursive, then it will also check for subsections.
        /// </summary>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public bool HasAnyOutcome(bool recursive) {
            var result = SectionRecords.Any(r => !string.IsNullOrEmpty(r.Outcome));
            if (recursive) {
                result = result || SubSections.Any(r => r.HasAnyOutcome(true));
            }
            return result;
        }
    }

    public sealed class TestReportSectionRecord {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Outcome { get; set; }
        public string Duration { get; set; }
    }
}
