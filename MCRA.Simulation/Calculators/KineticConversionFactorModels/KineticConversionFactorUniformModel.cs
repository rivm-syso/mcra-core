using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {

    public sealed class KineticConversionFactorUniformModel : KineticConversionFactorModel {

        internal class UniformModelParametrisation : KineticConversionFactorModelParametrisation {
            public double Lower { get; set; }
            public double Upper { get; set; }
        }

        public KineticConversionFactorUniformModel(
            KineticConversionFactor conversion,
            bool useSubgroups
        ) : base(conversion, useSubgroups) {
        }

        public override void CalculateParameters() {
            // First, check whether to use subgroups and if subgroups are available and use individual properties as keys for lookup
            if (UseSubgroups && ConversionRule.KCFSubgroups.Any()) {
                foreach (var sg in ConversionRule.KCFSubgroups) {
                    checkSubGroupUncertaintyValue(sg);
                    (var lower, var upper) = getParameters(sg.ConversionFactor, sg.UncertaintyUpper.Value);
                    if (!ModelParametrisations.Any(r => r.Age == sg.AgeLower && r.Gender == sg.Gender)) {
                        ModelParametrisations.Add(
                            new UniformModelParametrisation() {
                                Age = sg.AgeLower,
                                Gender = sg.Gender,
                                Lower = lower,
                                Upper = upper,
                                Factor = sg.ConversionFactor
                            }
                        );
                    }
                }
            }
            //This is the default, no individual properties are needed.
            if (!ModelParametrisations.Any(r => r.Age == null && r.Gender == GenderType.Undefined)) {
                if (!ConversionRule.UncertaintyUpper.HasValue) {
                    throw new Exception($"Missing uncertainty upper value for kinetic conversion factor {ConversionRule.IdKineticConversionFactor}");
                }
                (var lower, var upper) = getParameters(ConversionRule.ConversionFactor, ConversionRule.UncertaintyUpper.Value);
                ModelParametrisations.Add(
                    new UniformModelParametrisation() {
                        Age = null,
                        Gender = GenderType.Undefined,
                        Lower = lower,
                        Upper = upper,
                        Factor = ConversionRule.ConversionFactor
                    }
                );
            }
        }

        public override void ResampleModelParameters(IRandom random) {
            var parametrisations = ModelParametrisations
                .Cast<UniformModelParametrisation>()
                .ToList();
            // Correlated draw for all parametrisations
            var p = random.NextDouble();
            foreach (var parametrisation in parametrisations) {
                parametrisation.Factor = parametrisation.Lower + p * (parametrisation.Upper - parametrisation.Lower);
            }
        }

        private (double lower, double upper) getParameters(double factor, double upper) {
            if (factor > upper) {
                throw new Exception(
                    $"Incorrect kinetic conversion factor uncertainty distribution: " +
                    $"the (nominal) conversion factor ({factor:G3}) should be smaller than the upper value ({upper:G3})."
                );
            }
            var range = upper - factor;
            var lower = factor - range;
            if (lower < 0) {
                throw new Exception(
                    $"Incorrect kinetic conversion factor uncertainty distribution: " +
                    $"the computed lower bound for (nominal) factor ({factor:G3}) and upper bound ({upper:G3}) is smaller than zero."
                );
            }
            return (lower, upper);
        }
    }
}
