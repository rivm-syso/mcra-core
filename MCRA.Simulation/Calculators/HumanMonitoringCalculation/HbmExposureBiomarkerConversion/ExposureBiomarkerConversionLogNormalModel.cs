using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {

    public sealed class ExposureBiomarkerConversionLogNormalModel : ExposureBiomarkerConversionConstantModel {

        internal class LogNormalModelParametrisation : KineticConversionFactorModelParametrisation {
            public double Mu { get; set; }
            public double Sigma { get; set; }
        }

        public ExposureBiomarkerConversionLogNormalModel(
            ExposureBiomarkerConversion conversion,
            bool useSubgroups
        )
            : base(conversion, useSubgroups) {
        }

        protected override IKineticConversionFactorModelParametrisation getParametrisation(
            double factor,
            double upper,
            GenderType gender = GenderType.Undefined,
            double? age = null
        ) {
            var mu = UtilityFunctions.LogBound(factor);
            if (factor > upper) {
                throw new Exception($"Exposure biomarker conversion: the conversion factor {factor} is higher than the upper value: {upper}.");
            }
            var sigma = (UtilityFunctions.LogBound(upper) - mu) / 1.645;
            return new LogNormalModelParametrisation() {
                Age = age,
                Gender = gender,
                Mu = mu,
                Sigma = sigma
            };
        }

        protected override double drawFunction(IKineticConversionFactorModelParametrisation param, IRandom random) {
            var lnParams = param as LogNormalModelParametrisation;
            return UtilityFunctions.ExpBound(NormalDistribution.DrawInvCdf(random, lnParams.Mu, lnParams.Sigma));
        }
    }
}
