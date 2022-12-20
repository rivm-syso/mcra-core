using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class InterSpeciesFactor {
        public Compound Compound { get; set; }
        public Effect Effect { get; set; }

        public double StandardHumanBodyWeight { get; set; }
        public double StandardAnimalBodyWeight { get; set; }
        public string Species { get; set; }
        public double InterSpeciesFactorGeometricMean { get; set; }
        public double InterSpeciesFactorGeometricStandardDeviation { get; set; }
        public string HumanBodyWeightUnitString { get; set; }
        public string AnimalBodyWeightUnitString { get; set; }

        public bool IsDefault { get; set; }

        public BodyWeightUnit HumanBodyWeightUnit {
            get {
                return BodyWeightUnitConverter.FromString(HumanBodyWeightUnitString, BodyWeightUnit.kg);
            }
        }

        public BodyWeightUnit AnimalBodyWeightUnit {
            get {
                return BodyWeightUnitConverter.FromString(AnimalBodyWeightUnitString, BodyWeightUnit.kg);
            }
        }
    }
}
