using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.IntraSpeciesConversion {
    public sealed class IntraSpeciesFactorModel {

        public Effect Effect { get; set; }

        public Compound Substance { get; set; }

        public double Factor { get; set; }

        public double GeometricStandardDeviation { get; set; } = double.NaN;

        public double DegreesOfFreedom { get; set; }

        /// <summary>
        /// The intra-species factor used for creating this model.
        /// </summary>
        public IntraSpeciesFactor IntraSpeciesFactor { get; set; }
    }
}
