using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ParameterDistributionModels {
    public sealed class LogNormalDistributionModel : ProbabilityDistributionModel {
        public override void Initialize(double mean, double cv) {
            sigma = Math.Sqrt(Math.Log(Math.Pow(cv, 2) + 1));
            mu = Math.Log(mean) - Math.Pow(sigma, 2) / 2;
        }

        public override double Sample(IRandom random) {
            return Math.Exp(NormalDistribution.InvCDF(mu, sigma, random.NextDouble()));
        }
    }
}
