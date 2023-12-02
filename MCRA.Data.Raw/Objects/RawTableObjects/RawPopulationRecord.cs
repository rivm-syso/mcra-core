using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.Populations)]
    public sealed class RawPopulationRecord : IRawDataTableRecord {
        public string idPopulation { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? NominalBodyWeight { get; set; }
    }
}
