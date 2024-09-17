using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion.ExposureBiomarkerConversionModels {

    public sealed class ExposureBiomarkerConversionLogNormalModel : ExposureBiomarkerConversionConstantModel {

        internal class LogNormalModelParametrisation : ExposureBiomarkerConversionModelParametrisation {
            public double Mu { get; set; }
            public double Sigma { get; set; }
        }

        public ExposureBiomarkerConversionLogNormalModel(
            ExposureBiomarkerConversion conversion,
            bool useSubgroups
        ) : base(conversion, useSubgroups) {
        }

        protected override ExposureBiomarkerConversionModelParametrisation getParametrisation(
            double factor,
            double upper,
            GenderType gender = GenderType.Undefined,
            double? age = null
        ) {
            var distribution = LogNormalDistribution.FromMeanAndUpper(factor, upper);
            return new LogNormalModelParametrisation() {
                Age = age,
                Gender = gender,
                Mu = distribution.Mu,
                Sigma = distribution.Sigma
            };
        }

        protected override double drawFunction(ExposureBiomarkerConversionModelParametrisation param, IRandom random) {
            var lnParams = param as LogNormalModelParametrisation;
            return LogNormalDistribution.Draw(random, lnParams.Mu, lnParams.Sigma);
        }
    }
}
