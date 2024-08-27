using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public sealed class ExposureBiomarkerConversionInverseUniformModel : ExposureBiomarkerConversionModelBase {

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

        public override void CalculateParameters() {
            // First, check whether to use subgroups and if subgroups are available and use individual properties as
            // keys for lookup
            if (UseSubgroups) {
                foreach (var sg in ConversionRule.EBCSubgroups) {
                    checkSubGroupUncertaintyValue(sg);
                    (var lower, var upper) = getParameters(sg.ConversionFactor, sg.VariabilityUpper.Value);
                    if (!ModelParametrisations.Any(r => r.Age == sg.AgeLower && r.Gender == sg.Gender)) {
                        ModelParametrisations.Add(
                            new InverseUniformModelParametrisation() {
                                Age = sg.AgeLower,
                                Gender = sg.Gender,
                                Lower = lower,
                                Upper = upper
                            }
                        );
                    }
                }
            }
            // This is the default, no individual properties are needed.
            if (!ModelParametrisations.Any(r => r.Age == null && r.Gender == GenderType.Undefined)) {
                if (!ConversionRule.VariabilityUpper.HasValue) {
                    throw new Exception($"Missing uncertainty upper value for exposure biomarker conversion factor {ConversionRule.IdExposureBiomarkerConversion}");
                }
                (var lower, var upper) = getParameters(ConversionRule.ConversionFactor, ConversionRule.VariabilityUpper.Value);
                ModelParametrisations.Add(
                    new InverseUniformModelParametrisation() {
                        Age = null,
                        Gender = GenderType.Undefined,
                        Lower = lower,
                        Upper = upper
                    }
                );
            }
        }
        /// <summary>
        /// Determine parameters of inverseuniform from upper and median value making use of the fact that the median is 2/(a+b)
        /// </summary>
        /// <param name="factor"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private (double lower, double upper) getParameters(double factor, double upper) {
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
            return (lower, upper);
        }

        public override double Draw(IRandom random, double? age, GenderType gender) {
            Func<IKineticConversionFactorModelParametrisation, IRandom, double> drawFunction =
                (param, random) => {
                    var invUnifParams = param as InverseUniformModelParametrisation;
                    var b = invUnifParams.Upper;
                    var a = invUnifParams.Lower;
                    var draw = 1 / random.NextDouble(a, b);
                    return draw;
                };
            return drawForParametrisation(random, age, gender, drawFunction);
        }
    }
}
