using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.Consumptions)]
    public sealed class RawConsumption : IRawDataTableRecord {
        public string idIndividual { get; set; }
        public string idFood { get; set; }
        public string Facets { get; set; }
        public string idUnit { get; set; }
        public string idDay { get; set; }
        public string idMeal { get; set; }
        public double Amount { get; set; }
    }
}
