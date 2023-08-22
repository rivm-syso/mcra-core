using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion {

    /// <summary>
    /// Calculator class for computing biological matrix concentrations
    /// for a target matrix based on a concentration value of a sampling
    /// method of another biological matrix using a simple conversion factor
    /// that is the same for all biological matrices other than the target
    /// biological matrix.
    /// </summary>
    public class SimpleTargetMatrixConversionCalculator : ITargetMatrixConversionCalculator {

        /// <summary>
        /// The target unit to which to convert the concentrations.
        /// </summary>
        private readonly TargetUnit _targetUnit;

        /// <summary>
        /// Conversion factor to translate a concentration of one compartment
        /// to a concentration of another compartment.
        /// </summary>
        private readonly double _conversionFactor;

        /// <summary>
        /// Creates a new instance of a <see cref="SimpleTargetMatrixConversionCalculator"/>.
        /// </summary>
        /// <param name="conversionFactor"></param>
        /// <param name="targetUnit"></param>
        public SimpleTargetMatrixConversionCalculator(
            double conversionFactor,
            TargetUnit targetUnit
        ) {
            _targetUnit = targetUnit;
            _conversionFactor = conversionFactor;
        }

        /// <summary>
        /// Gets the converted concentration for the target biological matrix
        /// based on a concentration of the source biological matrix.
        /// </summary>
        public double GetTargetConcentration(
            double concentration,
            Compound substance,
            TargetUnit sourceUnit
        ) {
            if (sourceUnit.BiologicalMatrix == _targetUnit.BiologicalMatrix) {
                // If source equals target, then no matrix conversion
                // TODO: still unit conversion / alignment?
                return concentration;
            } else {
                // Apply conversion using the factor and update units
                // TODO: still unit conversion / alignment?
                return _conversionFactor * concentration;
            }
        }
    }
}
