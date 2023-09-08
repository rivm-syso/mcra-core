using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {
    public abstract class BloodCorrectionCalculatorBase {

        protected List<string> SubstancesExcludedFromStandardisation { get; private set; }

        public BloodCorrectionCalculatorBase(List<string> substancesExcludedFromStandardisation) {
            SubstancesExcludedFromStandardisation = substancesExcludedFromStandardisation;
        }

        /// <summary>
        /// Gets the collection (HashSet) of substances for which lipid correction
        /// is possible and desirable.
        /// </summary>
        /// <param name="sampleCollection"></param>
        /// <returns></returns>
        protected HashSet<Compound> getSubstancesWithLipidCorrection(
            HumanMonitoringSampleSubstanceCollection sampleCollection
        ) {
            // Get substances for which we want to apply lipid correction
            var allSubstances = sampleCollection.HumanMonitoringSampleSubstanceRecords
                .SelectMany(r => r.HumanMonitoringSampleSubstances.Keys)
                .Distinct();
            var lipidCorrection = allSubstances
                .Where(r => r.IsLipidSoluble)
                .Where(r => !SubstancesExcludedFromStandardisation.Contains(r.Code))
                .ToHashSet();
            return lipidCorrection;
        }

        /// <summary>
        /// Express results always in gram lipids (g lipid).
        /// </summary>
        /// <param name="sourceConcentrationUnit"></param>
        /// <param name="targetMassUnit"></param>
        /// <returns></returns>
        protected double getAlignmentFactor(
            ConcentrationUnit sourceConcentrationUnit,
            ConcentrationMassUnit targetMassUnit
        ) {
            var massUnit = sourceConcentrationUnit.GetConcentrationMassUnit();
            var amountUnit = sourceConcentrationUnit.GetSubstanceAmountUnit();
            var multiplier1 = massUnit.GetMultiplicationFactor(targetMassUnit);
            var multiplier2 = amountUnit.GetMultiplicationFactor(SubstanceAmountUnit.Grams, 1);
            var multiplier = multiplier1 / multiplier2;
            return multiplier;
        }
    }
}
