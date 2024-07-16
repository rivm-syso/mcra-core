using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration {
    public static class KineticConversionFactorExtensions {

        /// <summary>
        /// Returns whether the kinetic conversion factor is applicable for the
        /// specified combination of target and substance.
        /// </summary>
        public static bool MatchesFromTargetAndSubstance(
            this KineticConversionFactor factor,
            ExposureTarget target,
            Compound substance
        ) {
            if (factor.TargetFrom != target) {
                return false;
            }
            var result = factor.SubstanceFrom == substance
                || factor.SubstanceFrom == null
                || factor.SubstanceFrom == SimulationConstants.NullSubstance;
            return result;
        }

        /// <summary>
        /// Returns whether the kinetic conversion factor is applicable for the
        /// specified substance.
        /// </summary>
        public static bool MatchesFromSubstance(
            this KineticConversionFactor factor,
            Compound substance
        ) {
            var result = factor.SubstanceFrom == substance
                || factor.SubstanceFrom == null
                || factor.SubstanceFrom == SimulationConstants.NullSubstance;
            return result;
        }
    }
}
