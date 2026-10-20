using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;
using NCalc;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.ExposureResponseSpecificationModels {

    public abstract class ExposureResponseFactorModelBase : IExposureResponseFunctionModel {

        public double Nominal { get; set; }

        public ExposureResponseSpecificationDistributionType DistributionType { get; set; }
        public EffectMetric EffectMetric { get; set; }

        public double Lower { get; set; }

        public double Upper { get; set; }

        protected double _draw { get; set; }

        public ExposureResponseFactorModelBase(
            double nominal,
            double lower,
            double upper,
            ExposureResponseSpecificationDistributionType distributionType,
            EffectMetric effectMetric
        ) {
            DistributionType = distributionType;
            EffectMetric = effectMetric;
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
                  erf.ERFUncertaintyDistribution,
                  erf.EffectMetric
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
            if (_draw < 0D && 
                (EffectMetric == EffectMetric.RelativeRisk || EffectMetric == EffectMetric.OddsRatio)) {
                    throw new Exception("Resampling produced a negative exposure response function outcome.");
            }
        }

        private static double getValue(Expression expression) {
            return Convert.ToDouble(expression.Evaluate());
        }
    }
}
