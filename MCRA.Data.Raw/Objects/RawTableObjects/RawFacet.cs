using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.Facets)]
    public class RawFacet : IRawDataTableRecord {
        public string idFacet { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
