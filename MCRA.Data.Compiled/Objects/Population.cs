using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class Population : StrongEntity {

        public string Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double NominalBodyWeight { get; set; }
        public double Size { get; set; }

        public BodyWeightUnit BodyWeightUnit => BodyWeightUnit.kg;

        public Dictionary<string, PopulationIndividualPropertyValue> PopulationIndividualPropertyValues { get; set; }

        public ICollection<PopulationCharacteristic> PopulationCharacteristics { get; set; } = [];
        public bool HasPopulationCharacteristics() {
            return PopulationCharacteristics != null && PopulationCharacteristics.Count > 0;
        }

    }
}
