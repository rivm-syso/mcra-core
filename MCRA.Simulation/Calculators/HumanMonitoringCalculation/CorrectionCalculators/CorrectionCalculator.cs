using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators {
    public abstract class CorrectionCalculator : ICorrectionCalculator {

        public CorrectionCalculator(List<string> substancesExcludedFromStandardisation) {
            SubstancesExcludedFromStandardisation = substancesExcludedFromStandardisation;
        }

        protected List<string> SubstancesExcludedFromStandardisation { get; private set; }

        public abstract List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
          ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
        );

        /// <summary>
        /// Calculates the factor to convert from a source concentration unit, as expressed per volume, 
        /// to a target unit that is expressed per mass.
        /// </summary>
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
