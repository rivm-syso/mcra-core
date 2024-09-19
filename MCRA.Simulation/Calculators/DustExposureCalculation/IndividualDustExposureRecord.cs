using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public sealed class IndividualDustExposureRecord {
        public string IdIndividual { get; set; }
        public Individual Individual { get; set; }
        public Compound Substance { get; set; }
        public ExposureRoute ExposureRoute { get; set; }
        public double Exposure { get; set; }
        public ExposureUnitTriple ExposureUnit { get; set; }
    }
}


