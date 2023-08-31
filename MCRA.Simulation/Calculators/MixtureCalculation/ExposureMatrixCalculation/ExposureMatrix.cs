using MCRA.Utils;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation {
    public sealed class ExposureMatrix {
        public TargetUnit TargetUnit { get ; set; } 
        public GeneralMatrix Exposures { get; set; }
        public List<Compound> Substances { get; set; }
        public ICollection<Individual> Individuals { get; set; }
        public List<double> Sds { get; set; }
    }
}
