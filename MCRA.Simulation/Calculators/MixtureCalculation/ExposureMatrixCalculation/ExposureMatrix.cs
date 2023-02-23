using MCRA.Utils;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation {
    public sealed class ExposureMatrix {
        public GeneralMatrix Exposures { get; set; }
        public List<Compound> Substances { get; set; }
        public ICollection<Individual> Individuals { get; set; }
        public List<double> Sds { get; set; }
    }
}
