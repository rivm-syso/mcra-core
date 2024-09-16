using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ParameterDistributionModels {
    public sealed class DeterministicDistributionModel : ProbabilityDistributionModel {
        public override void Initialize(double mean, double sd) {
            mu = mean;
        }

        public override double Sample(IRandom random) {
            return mu;
        }
    }
}
