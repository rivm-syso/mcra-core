using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {

    public class KineticConversionFactorDistributionModelParametrisation<T> 
        : KineticConversionFactorModelParametrisation where T : Distribution {
        public T Distribution { get; set; }
    }

    public abstract class KineticConversionFactorDistributionModel<T>(
        KineticConversionFactor conversion,
        bool useSubgroups
    ) : KineticConversionFactorModelBase<KineticConversionFactorDistributionModelParametrisation<T>>(
        conversion, 
        useSubgroups
    ) where T : Distribution {

        public override void ResampleModelParameters(IRandom random) {
            var rnd = new McraRandomGenerator(random.Next());
            foreach (var parametrisation in ModelParametrisations) {
                // Correlated draw for all parametrisations
                parametrisation.Factor = parametrisation.Distribution.Draw(rnd);
                rnd.Reset();
            }
        }

        protected abstract T getDistributionFromNominalAndUpper(double factor, double upper);

        protected override KineticConversionFactorDistributionModelParametrisation<T> getSubgroupParametrisation(
            double? age,
            GenderType gender,
            double factor,
            double? upper
        ) {
            if (!upper.HasValue) {
                var msg = $"Missing uncertainty upper value for kinetic conversion factor {ConversionRule.IdKineticConversionFactor}";
                throw new Exception(msg);
            }
            try {
                var distribution = getDistributionFromNominalAndUpper(factor, upper.Value);
                var result = new KineticConversionFactorDistributionModelParametrisation<T>() {
                    Age = age,
                    Gender = gender,
                    Distribution = distribution,
                    Factor = factor
                };
                return result;
            } catch (Exception ex) {
                var msg = $"Incorrect specification of kinetic conversion factor uncertainty distribution: {ex.Message}";
                throw new Exception(msg);
            }
        }

        protected override void checkSubGroupUncertaintyValue(KineticConversionFactorSG sg) {
            if (!sg.UncertaintyUpper.HasValue) {
                var sgStrings = new List<string>();
                if (sg.AgeLower.HasValue) {
                    sgStrings.Add(sg.AgeLower.Value.ToString());
                }
                if (sg.Gender != GenderType.Undefined) {
                    sgStrings.Add(sg.Gender.ToString());
                }
                var sgString = string.Join(",", sgStrings);
                throw new Exception($"Missing uncertainty upper value for subgroup [{sgString}] of kinetic conversion factor [{sg.IdKineticConversionFactor}].");
            }
        }
    }
}
