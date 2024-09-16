using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsCollectionsGeneration;
using MCRA.Simulation.Constants;
using MCRA.Utils.Statistics;
using RDotNet;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {
    public abstract class KineticConversionFactorModel {

        public bool UseSubgroups { get; set; }

        protected List<IKineticConversionFactorModelParametrisation> ModelParametrisations { get; set; } = new();

        public KineticConversionFactor ConversionRule { get; protected set; }

        public KineticConversionFactorModel(KineticConversionFactor conversion, bool useSubgroups) {
            ConversionRule = conversion;
            UseSubgroups = useSubgroups;
        }

        public virtual void CalculateParameters() { }

        public abstract void ResampleModelParameters(IRandom random);

        /// <summary>
        /// Returns the conversion factor for the specified age and sex.
        /// </summary>
        public double GetConversionFactor(
            double? age,
            GenderType gender
        ) {
            var candidates = ModelParametrisations
                .Where(r => r.Gender == gender || r.Gender == GenderType.Undefined)
                .Where(r => !r.Age.HasValue || age.HasValue && age.Value >= r.Age.Value)
                .OrderByDescending(r => r.Gender)
                .ThenByDescending(r => r.Age)
                .ToList();
            var parametrisation = candidates.FirstOrDefault();
            return parametrisation.Factor;
        }

        /// <summary>
        /// Returns whether the kinetic conversion factor is applicable for the
        /// specified substance.
        /// </summary>
        public bool MatchesFromSubstance(Compound substance) {
            var result = ConversionRule.SubstanceFrom == substance
                || ConversionRule.SubstanceFrom == null
                || ConversionRule.SubstanceFrom == SimulationConstants.NullSubstance;
            return result;
        }

        /// <summary>
        /// Returns whether the conversion rule is specific for the from substance.
        /// </summary>
        public bool IsSubstanceFromSpecific() {
            return ConversionRule.SubstanceFrom != null
                && ConversionRule.SubstanceFrom != SimulationConstants.NullSubstance;
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
