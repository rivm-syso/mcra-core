using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawObjects {
    [RawDataSourceTableID(RawDataSourceTableID.IndividualProperties)]
    public class RawIndividualProperty : IRawDataTableRecord {
        public string idIndividualProperty { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PropertyLevelType PropertyLevel { get; set; }
        public IndividualPropertyType Type { get; set; }
    }
}
