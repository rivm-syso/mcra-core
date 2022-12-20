using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public sealed class ModelBasedIntakeResult {
        public CovariateGroup CovariateGroup { get; set; }
        public List<double> ModelBasedIntakes { get; set; }
    }
}
