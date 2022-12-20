using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.General.Action.Settings {
    public class RawDataSourceVersionDto : IRawDataSourceVersion {
        public int id { get; set; }

        public string FullPath { get; set; }

        public bool DataIsInDatabase { get; set; }

        public string Checksum { get; set; }

        public int VersionNumber { get; set; }

        public string Name { get; set; }

        public string DataSourceName { get; set; }

        public string DataSourcePath { get; set; }

        public DateTime? UploadTimestamp { get; set; } = null;

        public float? FileSizeKb { get; set; }

        public HashSet<SourceTableGroup> TableGroups { get; set; } = new HashSet<SourceTableGroup>();

        public bool ContainsSourceTableGroup(SourceTableGroup? tableGroup = null) =>
            tableGroup.HasValue ? TableGroups.Contains(tableGroup.Value) : TableGroups.Any();

        public void RegisterTableGroup(SourceTableGroup sourceTableGroup) =>
            TableGroups.Add(sourceTableGroup);
    }
}
