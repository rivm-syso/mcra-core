using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.Individuals)]
    public sealed class RawIndividual : IRawDataTableRecord {
        public string idIndividual { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string idFoodSurvey { get; set; }
        public double? BodyWeight { get; set; }
        public double? SamplingWeight { get; set; }
        public int? NumberOfSurveyDays { get; set; }
    }
}
