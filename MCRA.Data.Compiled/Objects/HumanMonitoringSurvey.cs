using MCRA.Utils.DateTimes;
using MCRA.General;
using MCRA.Data.Compiled.Interfaces;

namespace MCRA.Data.Compiled.Objects {
    public sealed class HumanMonitoringSurvey: StrongEntity, IIndividualCollection {

        public HumanMonitoringSurvey() {
            Individuals = [];
            Timepoints = [];
        }

        public string Location { get; set; }
        public string AgeUnitString { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberOfSurveyDays { get; set; }
        public ICollection<HumanMonitoringTimepoint> Timepoints { get; set; } = [];
        public string IdPopulation { get; set; }

        public ICollection<Individual> Individuals { get; set; } = [];

        public TimeRange Period =>
            StartDate.HasValue && EndDate.HasValue ? new TimeRange(StartDate.Value, EndDate.Value) : null;

        public BodyWeightUnit BodyWeightUnit { get; set; } = BodyWeightUnit.kg;
        public ConcentrationUnit LipidConcentrationUnit { get; set; } = ConcentrationUnit.mgPerdL;
        public ConcentrationUnit TriglycConcentrationUnit { get; set; } = ConcentrationUnit.mgPerdL;
        public ConcentrationUnit CholestConcentrationUnit { get; set; } = ConcentrationUnit.mgPerdL;
        public ConcentrationUnit CreatConcentrationUnit { get; set; } = ConcentrationUnit.mgPerdL;
    }
}
