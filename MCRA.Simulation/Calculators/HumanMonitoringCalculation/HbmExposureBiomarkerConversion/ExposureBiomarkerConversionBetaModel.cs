using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public sealed class ExposureBiomarkerConversionBetaModel : ExposureBiomarkerConversionConstantModel {

        internal class BetaModelParametrisation : KineticConversionFactorModelParametrisation {
            public double Alpha { get; set; }
            public double Beta { get; set; }
        }

        public ExposureBiomarkerConversionBetaModel(
            ExposureBiomarkerConversion conversion,
            bool useSubgroups
        ) : base(conversion, useSubgroups) {
        }

        protected override IKineticConversionFactorModelParametrisation getParametrisation(
            double factor,
            double upper,
            GenderType gender = GenderType.Undefined,
            double? age = null
        ) {
            if (factor < 0 || factor > 1) {
                throw new Exception($"Exposure biomarker conversion: the conversion factor ({factor}) should be between 0 and 1.");
            }
            if (upper < 0 || upper > factor * (1 - factor)) {
                throw new Exception($"Exposure biomarker conversion: the variability upper value ({upper}) should be be between 0 and {factor * (1 - factor):F5} (= factor * (1 - factor)) based on a conversion factor of {factor}.");
            }
            var alpha = ((1 - factor) / upper - 1 / factor) * factor * factor;
            var beta = alpha * (1 / factor - 1);
            return new BetaModelParametrisation() {
                Age = null,
                Gender = GenderType.Undefined,
                Alpha = alpha,
                Beta = beta
            };
        }

        protected override double drawFunction(IKineticConversionFactorModelParametrisation param, IRandom random) {
            var betaParams = param as BetaModelParametrisation;
            return BetaDistribution.Draw(random, betaParams.Alpha, betaParams.Beta);
        }
    }
}
