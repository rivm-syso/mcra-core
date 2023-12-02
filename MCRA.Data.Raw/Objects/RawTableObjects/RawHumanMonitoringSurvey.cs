using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.HumanMonitoringSurveys)]
    public class RawHumanMonitoringSurvey : IRawDataTableRecord {
        public string idSurvey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string BodyWeightUnit { get; set; }
        public string AgeUnit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Year { get; set; }
        public int NumberOfSurveyDays { get; set; }
        public string idPopulation { get; set; }
        public string LipidConcentrationUnit { get; set; }
        public string TriglycConcentrationUnit { get; set; }
        public string CholestConcentrationUnit { get; set; }
        public string CreatConcentrationUnit { get; set; }

    }
}
