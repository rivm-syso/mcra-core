using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.PbkModelParameterDistributionModels {
    class PbkModelParameterNormalDistributionModel : PbkModelParameterDistributionModel {
        public override void Initialize(double mean, double? sd) {
            mu = mean;
            if (!sd.HasValue) {
                throw new Exception("Incorrect parametrisation for normal PBK model parameter distribution model: SD not specified.");
            }
            sigma = sd.Value * mu;
            sigma = sigma < 0 ? 0 : sigma;
        }

        public override double Sample(IRandom random) {
            return NormalDistribution.InvCDF(mu, sigma, random.NextDouble());
        }
    }
}
