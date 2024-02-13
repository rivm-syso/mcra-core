using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {

    public sealed class ExposureBiomarkerConversionLogNormalModel : ExposureBiomarkerConversionModelBase {

        internal class LogNormalModelParametrisation : KineticConversionFactorModelParametrisationBase {
            public double Mu { get; set; }
            public double Sigma { get; set; }
        }

        public ExposureBiomarkerConversionLogNormalModel(
            ExposureBiomarkerConversion conversion,
            bool useSubgroups
        )
            : base(conversion, useSubgroups) {
        }

        public override void CalculateParameters() {
            //First, check whether to use subgroups and if subgroups are available and use individual properties as keys for lookup
            if (UseSubgroups && ConversionRule.EBCSubgroups.Any()) {
                foreach (var sg in ConversionRule.EBCSubgroups) {
                    checkSubGroupUncertaintyValue(sg);
                    (var mu, var sigma) = getParameters(sg.ConversionFactor, sg.VariabilityUpper.Value);
                    if (!ModelParametrisations.Any(r => r.Age == sg.AgeLower && r.Gender == sg.Gender)) {
                        ModelParametrisations.Add(
                            new LogNormalModelParametrisation() {
                                Age = sg.AgeLower,
                                Gender = sg.Gender,
                                Mu = mu,
                                Sigma = sigma
                            }
                        );
                    }
                }
            }
            //This is the default, no individual properties are needed.
            if (!ModelParametrisations.Any(r => r.Age == null && r.Gender == GenderType.Undefined)) {
                if (!ConversionRule.VariabilityUpper.HasValue) {
                    throw new Exception($"Missing uncertainty upper value for exposure biomarker conversion: {ConversionRule.IdExposureBiomarkerConversion}");
                }
                (var mu, var sigma) = getParameters(ConversionRule.ConversionFactor, ConversionRule.VariabilityUpper.Value);
                ModelParametrisations.Add(
                    new LogNormalModelParametrisation() {
                        Age = null,
                        Gender = GenderType.Undefined,
                        Mu = mu,
                        Sigma = sigma
                    }
                );
            }
        }

        private (double mu, double sigma) getParameters(double factor, double upper) {
            var mu = UtilityFunctions.LogBound(factor);
            if (factor > upper) {
                throw new Exception($"Exposure biomarker conversion: the conversion factor {factor} is higher than the upper value: {upper}.");
            }
            var sigma = (UtilityFunctions.LogBound(upper) - mu) / 1.645;
            return (mu, sigma);
        }

        public override double Draw(IRandom random, double? age, GenderType gender) {
            Func<IKineticConversionFactorModelParametrisation, IRandom, double> drawFunction =
                (param, random) => {
                    var lnParams = param as LogNormalModelParametrisation;
                    return UtilityFunctions.ExpBound(NormalDistribution.DrawInvCdf(random, lnParams.Mu, lnParams.Sigma));
                };
            return drawForParametrisation(random, age, gender, drawFunction);
        }
    }
}
