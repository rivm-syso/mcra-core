using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation.PbkModelParameterDistributionModels {

    public sealed class PbkModelParameterLogisticDistributionModel : PbkModelParameterDistributionModel {
        public override void Initialize(double mean, double? cv) {
            mu = UtilityFunctions.Logit(mean);
            if (!cv.HasValue) {
                throw new Exception("Incorrect parametrisation for logistic normal PBK model parameter distribution model: CV not specified.");
            }
            sigma = cv.Value * mean;
        }

        public override double Sample(IRandom random) {
            return UtilityFunctions.ILogit(NormalDistribution.InvCDF(mu, sigma, random.NextDouble()));
        }
    }
}
