using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {

    public sealed class KineticConversionFactorConstantModel(
        KineticConversionFactor conversion,
        bool useSubgroups
    ) : KineticConversionFactorModelBase<KineticConversionFactorModelParametrisation>(conversion, useSubgroups) {
        protected override KineticConversionFactorModelParametrisation getSubgroupParametrisation(
            double? age,
            GenderType gender,
            double factor,
            double? upper
        ) {
            var result = new KineticConversionFactorModelParametrisation() {
                Age = age,
                Gender = gender,
                Factor = factor
            };
            return result;
        }
    }
}
