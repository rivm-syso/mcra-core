using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public sealed class ExposureBiomarkerConversionBetaModel : ExposureBiomarkerConversionModelBase {

        internal class BetaModelParametrisation : KineticConversionFactorModelParametrisation {
            public double Alpha { get; set; }
            public double Beta { get; set; }
        }

        public ExposureBiomarkerConversionBetaModel(
            ExposureBiomarkerConversion conversion,
            bool useSubgroups
        ) : base(conversion, useSubgroups) {
        }

        public override void CalculateParameters() {//First, check whether to use subgroups and if subgroups are available and use individual properties as keys for lookup
            if (UseSubgroups) {
                foreach (var sg in ConversionRule.EBCSubgroups) {
                    checkSubGroupUncertaintyValue(sg);
                    (var alpha, var beta) = getParameters(sg.ConversionFactor, sg.VariabilityUpper.Value);
                    if (!ModelParametrisations.Any(r => r.Age == sg.AgeLower && r.Gender == sg.Gender)) {
                        ModelParametrisations.Add(
                            new BetaModelParametrisation() {
                                Age = sg.AgeLower,
                                Gender = sg.Gender,
                                Alpha = alpha,
                                Beta = beta
                            }
                        );
                    }
                }
            }
            //This is the default, no individual properties are needed.
            if (!ModelParametrisations.Any(r => r.Age == null && r.Gender == GenderType.Undefined)) {
                if (!ConversionRule.VariabilityUpper.HasValue) {
                    throw new Exception($"Missing uncertainty upper value for exposure biomarker conversion factor {ConversionRule.IdExposureBiomarkerConversion}");
                }
                (var alpha, var beta) = getParameters(ConversionRule.ConversionFactor, ConversionRule.VariabilityUpper.Value);
                ModelParametrisations.Add(
                    new BetaModelParametrisation() {
                        Age = null,
                        Gender = GenderType.Undefined,
                        Alpha = alpha,
                        Beta = beta
                    }
                );
            }
        }

        private (double alpha, double beta) getParameters(double factor, double upper) {
            if (factor < 0 || factor > 1) {
                throw new Exception($"Exposure biomarker conversion: the conversion factor ({factor}) should be between 0 and 1.");
            }
            if (upper < 0 || upper > factor * (1-factor)) {
                throw new Exception($"Exposure biomarker conversion: the variability upper value ({upper}) should be be between 0 and {factor * (1 - factor):F5} (= factor * (1 - factor)) based on a conversion factor of {factor}.");
            }
            var alpha = ((1 - factor) / upper - 1 / factor) * factor * factor;
            var beta = alpha * (1 / factor - 1);
            return (alpha, beta);
        }

        public override double Draw(IRandom random, double? age, GenderType gender) {
            Func<IKineticConversionFactorModelParametrisation, IRandom, double> drawFunction =
                (param, random) => {
                    var betaParams = param as BetaModelParametrisation;
                    return BetaDistribution.Draw(random, betaParams.Alpha, betaParams.Beta);
                };
            return drawForParametrisation(random, age, gender, drawFunction);
        }
    }
}
