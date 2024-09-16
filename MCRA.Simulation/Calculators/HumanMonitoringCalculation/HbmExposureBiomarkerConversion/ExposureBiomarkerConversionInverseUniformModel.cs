using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public sealed class ExposureBiomarkerConversionInverseUniformModel : ExposureBiomarkerConversionConstantModel {

        internal class InverseUniformModelParametrisation : KineticConversionFactorModelParametrisation {
            public double Lower { get; set; }
            public double Upper { get; set; }
        }

        /// <summary>
        /// see https://en.wikipedia.org/wiki/Inverse_distribution
        /// </summary>
        /// <param name="conversion"></param>
        /// <param name="useSubgroups"></param>
        public ExposureBiomarkerConversionInverseUniformModel(
            ExposureBiomarkerConversion conversion,
            bool useSubgroups
        ) : base(conversion, useSubgroups) {
        }

        /// <summary>
        /// Determine parameters of inverseuniform from upper and median value making use of the fact that the median is 2/(a+b)
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
            var lower = 1 / upper;
            upper = 2 / factor - lower;
            if (upper < lower) {
                throw new Exception(
                    $"Incorrect exposure biomarker conversion factor distribution: " +
                    $"the computed lower bound for (nominal) factor ({factor:G3}) and upper bound ({upper:G3}) is smaller than zero."
                );
            }
            return new InverseUniformModelParametrisation {
                Lower = lower,
                Upper = upper,
                Age = age,
                Gender = gender
            };
        }

        protected override double drawFunction(IKineticConversionFactorModelParametrisation param, IRandom random) {
            var invUnifParams = param as InverseUniformModelParametrisation;
            var b = invUnifParams.Upper;
            var a = invUnifParams.Lower;
            var draw = 1 / random.NextDouble(a, b);
            return draw;
        }
    }
}
