using MCRA.Utils;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation {
    public sealed class IndividualMatrix {
        public GeneralMatrix VMatrix { get; set; }
        public ICollection<SimulatedIndividual> SimulatedIndividuals { get; set; }
        public ClusterResult ClusterResult { get; set; }
        public int NumberOfComponents {
            get { return VMatrix.RowDimension; }
        }
    }
}
