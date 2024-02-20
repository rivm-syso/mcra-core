using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.BloodCorrectionCalculation {
    public abstract class BloodCorrectionCalculatorBase : CorrectionCalculator {

        public BloodCorrectionCalculatorBase(List<string> substancesExcludedFromStandardisation)
            : base(substancesExcludedFromStandardisation) {
        }

        /// <summary>
        /// Gets the collection (HashSet) of substances for which lipid correction
        /// is possible and desirable.
        /// </summary>
        protected override HashSet<Compound> getSubstancesForCorrection(
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
    }
}
