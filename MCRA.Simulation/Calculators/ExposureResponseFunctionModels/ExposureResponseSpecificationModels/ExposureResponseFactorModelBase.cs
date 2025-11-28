using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;
using NCalc;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.ExposureResponseSpecificationModels {

    public abstract class ExposureResponseFactorModelBase : IExposureResponseFunctionModel {

        public double Nominal { get; set; }

        public ExposureResponseSpecificationDistributionType DistributionType { get; set; }

        public double Lower { get; set; }

        public double Upper { get; set; }

        protected double _draw { get; set; }

        public ExposureResponseFactorModelBase(
            double nominal,
            double lower,
            double upper,
            ExposureResponseSpecificationDistributionType distributionType
        ) {
            DistributionType = distributionType;
            Nominal = nominal;
            Lower = lower;
            Upper = upper;
            _draw = Nominal;
        }

        public ExposureResponseFactorModelBase(ExposureResponseFunction erf)
            : this(
                  getValue(erf.ExposureResponseSpecification),
                  getValue(erf.ExposureResponseSpecificationLower),
                  getValue(erf.ExposureResponseSpecificationUpper),
                  erf.ERFUncertaintyDistribution
            ) {
        }

        public abstract double Compute(double exposure, double counterFactualValue);

        public void Resample(IRandom random) {
            if (DistributionType == ExposureResponseSpecificationDistributionType.Constant) {
                _draw = Nominal;
            } else {
                var distribution = ExposureResponseUncertaintyDistributionFactory.GetDistribution(
                    DistributionType,
                    () => Nominal,
                    () => Lower,
                    () => Upper
                );
                _draw = distribution.Draw(random);
            }
        }

        private static double getValue(Expression expression) {
            return Convert.ToDouble(expression.Evaluate());
        }
    }
}
