using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public sealed class ExposureBiomarkerConversionConstantModel : ExposureBiomarkerConversionModelBase {

        internal class ConstantModelParametrisation : KineticConversionFactorModelParametrisationBase {
            public double Factor { get; set; }
        }
        public ExposureBiomarkerConversionConstantModel(
            ExposureBiomarkerConversion conversion,
            bool useSubgroups
        ) : base(conversion, useSubgroups) {
        }

        public override void CalculateParameters() {
            //First, check whether to use subgroups and if subgroups are available and use individual properties as keys for lookup
            if (UseSubgroups) {
                foreach (var sg in ConversionRule.EBCSubgroups) {
                    ModelParametrisations.Add(
                        new ConstantModelParametrisation() {
                            Age = sg.AgeLower,
                            Gender = sg.Gender,
                            Factor = sg.ConversionFactor
                        }
                    );
                }
            }
            //This is the default, no individual properties are needed.
            if (!ModelParametrisations.Any(r => r.Age == null && r.Gender == GenderType.Undefined)) {
                ModelParametrisations.Add(
                    new ConstantModelParametrisation() {
                        Age = null,
                        Gender = GenderType.Undefined,
                        Factor = ConversionRule.ConversionFactor
                    }
                );
            }
        }

        public override double Draw(IRandom random, double? age, GenderType gender) {
            Func<IKineticConversionFactorModelParametrisation, IRandom, double> drawFunction =
                (param, random) => {
                    var constParams = param as ConstantModelParametrisation;
                    return constParams.Factor;
                };
            return drawForParametrisation(random, age, gender, drawFunction);
        }
    }
}
