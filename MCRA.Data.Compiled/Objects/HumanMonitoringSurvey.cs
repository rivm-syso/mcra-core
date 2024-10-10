using MCRA.Utils.DateTimes;
using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class HumanMonitoringSurvey: StrongEntity {

        public HumanMonitoringSurvey() {
            Individuals = [];
            Timepoints = [];
        }

        public string Location { get; set; }
        public string BodyWeightUnitString { get; set; }
        public string AgeUnitString { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberOfSurveyDays { get; set; }
        public ICollection<HumanMonitoringTimepoint> Timepoints { get; set; }
        public string IdPopulation { get; set; }
        public string LipidConcentrationUnitString { get; set; }
        public string TriglycConcentrationUnitString { get; set; }
        public string CholestConcentrationUnitString { get; set; }
        public string CreatConcentrationUnitString { get; set; }

        public ICollection<Individual> Individuals { get; set; }

        public TimeRange Period {
            get {
                if (StartDate.HasValue && EndDate.HasValue) {
                    return new TimeRange(StartDate.Value, EndDate.Value);
                } else {
                    return null;
                }
            }
        }

        public BodyWeightUnit BodyWeightUnit =>
            BodyWeightUnitConverter.FromString(BodyWeightUnitString, BodyWeightUnit.kg);

        public ConcentrationUnit LipidConcentrationUnit =>
            ConcentrationUnitConverter.FromString(LipidConcentrationUnitString, ConcentrationUnit.mgPerdL);

        public ConcentrationUnit TriglycConcentrationUnit =>
            ConcentrationUnitConverter.FromString(TriglycConcentrationUnitString, ConcentrationUnit.mgPerdL);

        public ConcentrationUnit CholestConcentrationUnit =>
            ConcentrationUnitConverter.FromString(CholestConcentrationUnitString, ConcentrationUnit.mgPerdL);

        public ConcentrationUnit CreatConcentrationUnit =>
            ConcentrationUnitConverter.FromString(CreatConcentrationUnitString, ConcentrationUnit.mgPerdL);
    }
}
