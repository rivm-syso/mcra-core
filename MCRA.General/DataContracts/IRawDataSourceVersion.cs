namespace MCRA.General {
    public interface IRawDataSourceVersion {
        int id { get; }

        bool ContainsSourceTableGroup(SourceTableGroup? tableGroup = null);

        void RegisterTableGroup(SourceTableGroup sourceTableGroup);

        string FullPath { get; }

        bool DataIsInDatabase { get; }

        string Checksum { get; }

        int VersionNumber { get; }

        string Name { get; }

        string DataSourceName { get; }
        string DataSourcePath { get; }

        DateTime? UploadTimestamp { get; }

        HashSet<SourceTableGroup> TableGroups { get; set; }
    }
}
