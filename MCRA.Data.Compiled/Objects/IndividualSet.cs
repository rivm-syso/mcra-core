using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class IndividualSet : StrongEntity {

        public string AgeUnitString { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string IdPopulation { get; set; }

        public BodyWeightUnit BodyWeightUnit { get; set; } = BodyWeightUnit.kg;

        public ICollection<Individual> Individuals { get; set; } = [];
    }
}
