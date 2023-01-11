
using System.Xml.Serialization;

namespace MCRA.General {

    /// <summary>
    /// Defines a mapping of a raw data source as group of a compiled data source.
    /// </summary>
    public sealed class DataSourceMappingRecord {
        public SourceTableGroup SourceTableGroup { get; set; }
        public int IdRawDataSourceVersion { get; set; } = -1;
        public string Name { get; set; }
        [XmlIgnore]
        public string RawDataSourcePath { get; set; }
        public string RepositoryPath { get; set; }
        public string Checksum { get; set; }
        //XML serialization: skip IdRawDataSourceVersion if < 0
        public bool ShouldSerializeIdRawDataSourceVersion() => IdRawDataSourceVersion >= 0;

        public DataSourceMappingRecord Clone() {
            return new DataSourceMappingRecord() {
                SourceTableGroup = this.SourceTableGroup,
                IdRawDataSourceVersion = this.IdRawDataSourceVersion,
                Name = this.Name,
                RawDataSourcePath = this.RawDataSourcePath,
                RepositoryPath = this.RepositoryPath,
                Checksum = this.Checksum,
            };
        }

        public override string ToString() => $"{SourceTableGroup}: {Name} (Id: {IdRawDataSourceVersion})";
    }
}
