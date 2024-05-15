using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public abstract class KineticConversionFactorModel {

        public bool UseSubgroups { get; set; }  

        public KineticConversionFactor ConversionRule { get; protected set; }

        protected List<IKineticConversionFactorModelParametrisation> ModelParametrisations { get; set; } = new();

        public KineticConversionFactorModel(KineticConversionFactor conversion, bool useSubgroups) {
            ConversionRule = conversion;
            UseSubgroups = useSubgroups;
        }

        public virtual void CalculateParameters() { }

        public abstract void ResampleModelParameters(IRandom random);

        public double GetConversionFactor(
            double? age,
            GenderType gender
        ) {
            var candidates = ModelParametrisations
                .Where(r => r.Gender == gender || r.Gender == GenderType.Undefined)
                .Where(r => !r.Age.HasValue || (age.HasValue && age.Value >= r.Age.Value))
                .OrderByDescending(r => r.Gender)
                .ThenByDescending(r => r.Age)
                .ToList();
            var parametrisation = candidates.FirstOrDefault();
            return parametrisation.Factor;
        }

        protected static void checkSubGroupUncertaintyValue(KineticConversionFactorSG sg) {
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
