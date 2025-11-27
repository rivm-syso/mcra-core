using MCRA.Data.Compiled.Interfaces;
using MCRA.General;
using MCRA.Utils.DateTimes;

namespace MCRA.Data.Compiled.Objects {
    public sealed class FoodSurvey : StrongEntity, IIndividualCollection {
        public string Location { get; set; }
        public string AgeUnitString { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? NumberOfSurveyDays { get; set; }
        public string IdPopulation { get; set; }

        public ICollection<Individual> Individuals { get; set; } = new HashSet<Individual>();

        public TimeRange Period {
            get {
                if (StartDate.HasValue && EndDate.HasValue) {
                    return new TimeRange(StartDate.Value, EndDate.Value);
                } else {
                    return null;
                }
            }
        }

        public BodyWeightUnit BodyWeightUnit { get; set; } = BodyWeightUnit.kg;
        public ConsumptionUnit ConsumptionUnit { get; set; } = ConsumptionUnit.g;
    }
}
