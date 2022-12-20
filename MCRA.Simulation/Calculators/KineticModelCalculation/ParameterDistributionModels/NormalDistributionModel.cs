using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.ParameterDistributionModels {
    class NormalDistributionModel : ProbabilityDistributionModel {
        public override void Initialize(double mean, double sd) {
            mu = mean;
            sigma = sd * mu;
            sigma = sigma < 0 ? 0 : sigma;
        }

        public override double Sample(IRandom random) {
            return NormalDistribution.InvCDF(mu, sigma, random.NextDouble());
        }
    }
}
