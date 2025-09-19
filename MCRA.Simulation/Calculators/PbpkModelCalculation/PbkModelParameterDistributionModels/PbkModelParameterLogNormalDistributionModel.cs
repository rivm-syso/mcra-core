using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation.PbkModelParameterDistributionModels {
    public sealed class PbkModelParameterLogNormalDistributionModel : PbkModelParameterDistributionModel {
        public override void Initialize(double mean, double? cv) {
            if (!cv.HasValue) {
                throw new Exception("Incorrect parametrisation for log-normal PBK model parameter distribution model: CV not specified.");
            }
            sigma = Math.Sqrt(Math.Log(Math.Pow(cv.Value, 2) + 1));
            mu = Math.Log(mean) - Math.Pow(sigma, 2) / 2;
        }

        public override double Sample(IRandom random) {
            return Math.Exp(NormalDistribution.InvCDF(mu, sigma, random.NextDouble()));
        }
    }
}
