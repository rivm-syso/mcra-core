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

        public bool IsDefault { get; set; }

        public BodyWeightUnit HumanBodyWeightUnit { get; set; } = BodyWeightUnit.kg;
        public BodyWeightUnit AnimalBodyWeightUnit { get; set; } = BodyWeightUnit.kg;
    }
}
