using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public sealed class ExposureBiomarkerConversionUniformModel : ExposureBiomarkerConversionConstantModel {

        internal class UniformModelParametrisation : KineticConversionFactorModelParametrisation {
            public double Lower { get; set; }
            public double Upper { get; set; }
        }

        public ExposureBiomarkerConversionUniformModel(
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
            if (factor > upper) {
                throw new Exception(
                    $"Incorrect exposure biomarker conversion factor distribution: " +
                    $"the conversion factor ({factor:G3}) should be smaller than the upper value ({upper:G3})."
                );
            }
            var range = upper - factor;
            var lower = factor - range;
            if (lower < 0) {
                throw new Exception(
                    $"Incorrect exposure biomarker conversion factor distribution: " +
                    $"the computed lower bound for (nominal) factor ({factor:G3}) and upper bound ({upper:G3}) is smaller than zero."
                );
            }
            return new UniformModelParametrisation {
                Lower = lower,
                Upper = upper,
                Age = age,
                Gender = gender
            };
        }

        protected override double drawFunction(IKineticConversionFactorModelParametrisation param, IRandom random) {
            var unifParams = param as UniformModelParametrisation;
            return random.NextDouble(unifParams.Lower, unifParams.Upper);
        }
    }
}
