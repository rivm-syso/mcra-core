using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.PbkModelParameterDistributionModels {
    public sealed class PbkModelParameterDeterministicModel : PbkModelParameterDistributionModel {
        public override void Initialize(double mean, double? sd) {
            mu = mean;
        }

        public override double Sample(IRandom random) {
            return mu;
        }
    }
}
