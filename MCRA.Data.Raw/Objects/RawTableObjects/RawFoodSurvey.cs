using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.FoodSurveys)]
    public sealed class RawFoodSurvey : IRawDataTableRecord {
        public string idFoodSurvey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Year { get; set; }
        public string Location { get; set; }
        public string BodyWeightUnit { get; set; }
        public string AgeUnit { get; set; }
        public string ConsumptionUnit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberOfSurveyDays { get; set; }
        public string idPopulation { get; set; }
    }
}
