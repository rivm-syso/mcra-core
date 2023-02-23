using MCRA.Utils;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation {
    public sealed class IndividualMatrix {
        public GeneralMatrix VMatrix { get; set; }
        public ICollection<Individual> Individuals { get; set; }
        public ClusterResult ClusterResult { get; set; } 
        public int NumberOfComponents {
            get { return VMatrix.RowDimension; }
        }
    }
}
