using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.ParameterDistributionModels {

    public sealed class LogisticDistributionModel : ProbabilityDistributionModel {
        public override void Initialize(double mean, double cv) {
            mu = UtilityFunctions.Logit(mean);
            sigma = cv * mean;
        }

        public override double Sample(IRandom random) {
            return UtilityFunctions.ILogit(NormalDistribution.InvCDF(mu, sigma, random.NextDouble()));
        }
    }
}
