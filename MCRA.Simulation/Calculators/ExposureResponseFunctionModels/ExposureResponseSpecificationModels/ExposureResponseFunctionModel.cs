using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;
using NCalc;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.ExposureResponseSpecificationModels {

    public sealed class ExposureResponseFunctionModel : IExposureResponseFunctionModel {

        public Expression Nominal { get; set; }

        public ExposureResponseSpecificationDistributionType DistributionType { get; set; }

        public Expression Lower { get; set; }

        public Expression Upper { get; set; }

        public bool IsUncertain { get; set; } = false;

        public int UncertaintySeed { get; set; }

        /// <summary>
        /// Delegate function to create/fit (uncertainty) distribution from nominal, lower, and upper.
        /// </summary>
        private readonly Func<Func<double>, Func<double>, Func<double>, Distribution> _fitDistribution;

        public ExposureResponseFunctionModel(
            Expression nominal,
            Expression lower,
            Expression upper,
            ExposureResponseSpecificationDistributionType distributionType
        ) {
            Nominal = nominal;
            DistributionType = distributionType;
            Lower = lower;
            Upper = upper;
            _fitDistribution = (getNom, getLow, getUpp) => ExposureResponseUncertaintyDistributionFactory
                .GetDistribution(
                    distributionType: DistributionType,
                    getNominal: () => getNom(),
                    getLower: () => getLow(),
                    getUpper: () => getUpp()
                );
        }

        public double Compute(double exposure, double counterFactualValue) {
            if (!IsUncertain) {
                return evaluateFunction(Nominal, exposure);
            } else {
                var distribution = _fitDistribution(
                    () => evaluateFunction(Nominal, exposure),
                    () => evaluateFunction(Lower, exposure),
                    () => evaluateFunction(Upper, exposure)
                );
                // Draw from distribution using the overall seed
                return distribution.Draw(new McraRandomGenerator(UncertaintySeed));
            }
        }

        public void Resample(IRandom random) {
            if (DistributionType != ExposureResponseSpecificationDistributionType.Constant) {
                IsUncertain = true;
                UncertaintySeed = random.Next();
            }
        }

        private double evaluateFunction(Expression expression, double exposure) {
            expression.Parameters["x"] = exposure;
            return Convert.ToDouble(expression.Evaluate());
        }
    }
}
