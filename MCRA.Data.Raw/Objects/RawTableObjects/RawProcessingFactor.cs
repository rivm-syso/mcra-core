using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.ProcessingFactors)]
    public sealed class RawProcessingFactor : IRawDataTableRecord {
        public string idProcessingType { get; set; }
        public string idCompound { get; set; }
        public string idFoodProcessed { get; set; }
        public string idFoodUnprocessed { get; set; }
        public double Nominal { get; set; }
        public double? Upper { get; set; }
        public double? NominalUncertaintyUpper { get; set; }
        public double? UpperUncertaintyUpper { get; set; }
    }
}
