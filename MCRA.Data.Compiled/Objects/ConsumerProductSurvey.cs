using MCRA.Data.Compiled.Interfaces;
using MCRA.General;
using MCRA.Utils.DateTimes;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConsumerProductSurvey : StrongEntity, IIndividualCollection {
        public ConsumerProductSurvey() {
            Individuals = [];
        }
        public string Country { get; set; }
        public string AgeUnitString { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string IdPopulation { get; set; }

        public ICollection<Individual> Individuals { get; set; } = [];

        public TimeRange Period =>
            StartDate.HasValue && EndDate.HasValue ? new TimeRange(StartDate.Value, EndDate.Value) : null;

        public BodyWeightUnit BodyWeightUnit { get; set; } = BodyWeightUnit.kg;
    }
}
