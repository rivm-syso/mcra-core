using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConsumerProductApplicationAmountCalculation {

    public sealed class ConsumerProductApplicationAmountLogNormalModel(
        ConsumerProductApplicationAmountSGs applicationAmount
        ) : ConsumerProductApplicationAmountConstantModel(applicationAmount) {

        internal class LogNormalModelParametrisation : ConsumerProductApplicationAmountModelParametrisation {
            public double Mu { get; set; }
            public double Sigma { get; set; }
        }

        protected override ConsumerProductApplicationAmountModelParametrisation getParametrisation(
            double mean,
            double cv,
            GenderType gender = GenderType.Undefined,
            double? age = null
        ) {
            var distribution = LogNormalDistribution.FromMeanAndCv(mean, cv);
            return new LogNormalModelParametrisation() {
                Age = age,
                Gender = gender,
                Mu = distribution.Mu,
                Sigma = distribution.Sigma
            };
        }

        protected override double drawFunction(ConsumerProductApplicationAmountModelParametrisation param, IRandom random) {
            var lnParams = param as LogNormalModelParametrisation;
            return LogNormalDistribution.Draw(random, lnParams.Mu, lnParams.Sigma);
        }
    }
}
