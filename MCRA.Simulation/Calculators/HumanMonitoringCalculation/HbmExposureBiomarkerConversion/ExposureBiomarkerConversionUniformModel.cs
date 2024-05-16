using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public sealed class ExposureBiomarkerConversionUniformModel : ExposureBiomarkerConversionModelBase {

        internal class UniformModelParametrisation : KineticConversionFactorModelParametrisationBase {
            public double Lower { get; set; }
            public double Upper { get; set; }
        }

        public ExposureBiomarkerConversionUniformModel(
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
                            new UniformModelParametrisation() {
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
                    new UniformModelParametrisation() {
                        Age = null,
                        Gender = GenderType.Undefined,
                        Lower = lower,
                        Upper = upper
                    }
                );
            }
        }

        private (double lower, double upper) getParameters(double factor, double upper) {
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
            return (lower, upper);
        }

        public override double Draw(IRandom random, double? age, GenderType gender) {
            Func<IKineticConversionFactorModelParametrisation, IRandom, double> drawFunction =
                (param, random) => {
                    var unifParams = param as UniformModelParametrisation;
                    return random.NextDouble(unifParams.Lower, unifParams.Upper);
                };
            return drawForParametrisation(random, age, gender, drawFunction);
        }
    }
}
