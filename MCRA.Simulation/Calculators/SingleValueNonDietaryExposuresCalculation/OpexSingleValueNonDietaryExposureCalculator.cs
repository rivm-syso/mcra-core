using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueInternalExposuresCalculation {
    public class OpexSingleValueNonDietaryExposureCalculator : ISingleValueInternalExposureCalculator {

        public OpexSingleValueNonDietaryExposureCalculator(
        ) {
        }

        /// <summary>
        /// Computes single value internal exposures.
        /// </summary>
        public ICollection<ISingleValueNonDietaryExposure> Compute(
            TargetUnit targetUnit
        ) {
            var results = new List<ISingleValueNonDietaryExposure>();
            return results;
        }

       
    }
}
