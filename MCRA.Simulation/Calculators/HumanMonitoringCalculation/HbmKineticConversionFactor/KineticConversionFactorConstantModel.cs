using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public sealed class KineticConversionFactorConstantModel : KineticConversionFactorModel {

        internal class ConstantModelParametrisation : KineticConversionFactorModelParametrisationBase {
        }

        public KineticConversionFactorConstantModel(
            KineticConversionFactor conversion,
            bool useSubgroups
        ) : base(conversion, useSubgroups) {
        }

        public override void CalculateParameters() {
            //First, check whether to use subgroups and if subgroups are available and use individual properties as keys for lookup
            if (UseSubgroups && ConversionRule.KCFSubgroups.Any()) {
                foreach (var sg in ConversionRule.KCFSubgroups) {
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

        public override void ResampleModelParameters(IRandom random) {
            // No action: no uncertainty
        }
    }
}
