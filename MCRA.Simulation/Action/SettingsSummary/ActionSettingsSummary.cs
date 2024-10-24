using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.OutputGeneration;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Collections;

namespace MCRA.Simulation.Action {

    [Serializable]
    public sealed class ActionSettingsSummary {

        public string Title { get; set; }
        public List<IActionSettingSummaryRecord> SummaryRecords { get; set; }
        public List<ActionSettingsSummary> SubSections { get; set; }

        public List<ActionSettingsSummary> ScopeSubSections { get; set; }
        public List<ActionSettingsSummary> SubActionSubSections { get; set; }

        public bool IsActionRoot { get; set; }

        public ActionSettingsSummary(string title) {
            IsActionRoot = false;
            Title = title;
            SummaryRecords = new List<IActionSettingSummaryRecord>();
            SubSections = new List<ActionSettingsSummary>();
            ScopeSubSections = new List<ActionSettingsSummary>();
            SubActionSubSections = new List<ActionSettingsSummary>();
        }

        public bool IsValid {
            get {
                return SummaryRecords.All(r => r.IsValid)
                    && SubSections.All(r => r.IsValid)
                    && SubActionSubSections.All(r => r.IsValid)
                    && ScopeSubSections.All(r => r.IsValid);
            }
        }

        public bool HasOwnSummary {
            get {
                return SubSections.Any() || SummaryRecords.Any();
            }
        }

        public void SummarizeSetting<T>(SettingsItemType settingsItemType, T value, bool isValid = true, bool isVisible = true) {
            if (isVisible) {
                var settingsDefinition = McraSettingsDefinitions.Instance.SettingsDefinitions[settingsItemType];
                SummaryRecords.Add(new ActionSettingSummaryRecord() {
                    SettingsItemType = settingsItemType,
                    Option = settingsDefinition.Name,
                    Value = value.PrintValue(),
                    RawValue = value,
                    IsValid = isValid
                });
            }
        }

        public void SummarizeSetting<T>(string name, T value, bool isValid = true, bool isVisible = true) {
            if (isVisible) {
                SummaryRecords.Add(new ActionSettingSummaryRecord() {
                    SettingsItemType = SettingsItemType.Undefined,
                    Option = name,
                    Value = value.PrintValue(),
                    RawValue = value,
                    IsValid = isValid
                });
            }
        }

        public void SummarizeDataSource(SourceTableGroup tableGroup, IRawDataSourceVersion version) {
            var record = new ActionDataSummaryRecord() {
                SourceTableGroup = tableGroup,
                DataSourceName = version?.DataSourceName,
                DataSourcePath = version?.DataSourcePath,
                Checksum = version?.Checksum,
                IdDataSourceVersion = version?.id ?? -1,
                Version = version?.VersionNumber ?? -1,
                VersionName = version?.Name,
                VersionDate = version?.UploadTimestamp,
                IsValid = version?.DataSourceName != null
            };
            SummaryRecords.Add(record);
        }

        public List<IActionSettingSummaryRecord> GetSettingsSummaryRecordsRecursive() {
            var result = new List<IActionSettingSummaryRecord>();
            result.AddRange(SummaryRecords);
            if (SubSections?.Any() ?? false) {
                foreach (var subSection in SubSections) {
                    result.AddRange(subSection.GetSettingsSummaryRecordsRecursive());
                }
            }
            if (ScopeSubSections?.Any() ?? false) {
                foreach (var subSection in ScopeSubSections) {
                    result.AddRange(subSection.GetSettingsSummaryRecordsRecursive());
                }
            }
            if (SubActionSubSections?.Any() ?? false) {
                foreach (var subSection in SubActionSubSections) {
                    result.AddRange(subSection.GetSettingsSummaryRecordsRecursive());
                }
            }
            return result;
        }

        public void WriteToOutputSummaryRecursive(
            SectionHeader header,
            ref int order,
            Guid? sectionId = null,
            bool nestedScope = false,
            bool nestedInputActions = false,
            HashSet<ActionSettingsSummary> tabuList = null
        ) {
            SectionHeader subHeader = null;
            if (HasOwnSummary) {
                // Section content: settings recrods
                var summarySection = (sectionId != null) ? new SettingsSummarySection(sectionId.Value) : new SettingsSummarySection();
                subHeader = header.AddSubSectionHeaderFor(summarySection, Title, order++);

                // Section settings
                var summaryRecords = SummaryRecords.Where(r => r is ActionSettingSummaryRecord).Cast<ActionSettingSummaryRecord>();
                foreach (var item in summaryRecords) {
                    summarySection.SummarizeSetting(item.Option, item.Value);
                }

                // Data source records
                var dataSourceRecords = SummaryRecords.Where(r => r is ActionDataSummaryRecord).Cast<ActionDataSummaryRecord>();
                foreach (var item in dataSourceRecords) {
                    summarySection.SummarizeDataSource(item);
                }

                subHeader.SaveSummarySection(summarySection);
            }

            tabuList ??= new HashSet<ActionSettingsSummary>();
            tabuList.Add(this);

            int subOrder = 0;

            // Settings sub-sections
            foreach (var subSection in SubSections) {
                subSection.WriteToOutputSummaryRecursive(subHeader, ref subOrder, null, nestedScope, nestedInputActions, tabuList);
            }

            // Scope summaries
            if (nestedScope) {
                foreach (var subSection in ScopeSubSections) {
                    subSection.WriteToOutputSummaryRecursive(header, ref order, null, nestedScope, nestedInputActions, tabuList);
                }
            }

            // Input-action sub-sections
            if (nestedInputActions) {
                foreach (var subSection in SubActionSubSections) {
                    if (!tabuList.Contains(subSection)) {
                        subSection.WriteToOutputSummaryRecursive(header, ref order, null, nestedScope, nestedInputActions, tabuList);
                    }
                }
            }
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                foreach (var record in this.ScopeSubSections) {
                    hash = hash * 23 + record.GetHashCode();
                }
                foreach (var record in this.SubActionSubSections) {
                    hash = hash * 29 + record.GetHashCode();
                }
                foreach (var record in this.SubSections) {
                    hash = hash * 31 + record.GetHashCode();
                }
                foreach (var record in this.SummaryRecords) {
                    hash = hash * 37 + record.GetHashCode();
                }
                return hash;
            }
        }

        public string GetHash() {
            var bytes = Encoding.UTF8.GetBytes(this.GetHashCode().ToString());
            var sha1 = SHA1.Create();
            var hashBytes = sha1.ComputeHash(bytes);
            return hexStringFromBytes(hashBytes);
        }

        /// <summary>
        /// Convert an array of bytes to a string of hex digits
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        /// <returns>String of hex digits</returns>
        private static string hexStringFromBytes(byte[] bytes) {
            var sb = new StringBuilder();
            foreach (byte b in bytes) {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }
    }
}
