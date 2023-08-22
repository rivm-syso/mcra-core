using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Units;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion {

    /// <summary>
    /// Calculator class for computing biological matrix concentrations
    /// for a target matrix based on a concentration value of a sampling
    /// method of another biological matrix using a simple conversion factor
    /// that is the same for all biological matrices other than the target
    /// biological matrix.
    /// </summary>
    public class TargetMatrixKineticConversionCalculator : ITargetMatrixConversionCalculator {

        /// <summary>
        /// The target unit to which to convert the concentrations.
        /// </summary>
        private readonly TargetUnit _targetUnit;

        /// <summary>
        /// Dictionary with relevant kinetic conversion models
        /// </summary>
        private readonly Dictionary<(Compound, BiologicalMatrix), KineticConversionFactor> _kineticConversionModels;

        /// <summary>
        /// Creates a new instance of a <see cref="TargetMatrixKineticConversionCalculator"/>.
        /// Select only the conversion records for the given target biological matrix
        /// </summary>
        /// <param name="kineticConversionFactors"></param>
        /// <param name="targetUnit"></param>
        public TargetMatrixKineticConversionCalculator(
            ICollection<KineticConversionFactor> kineticConversionFactors,
            TargetUnit targetUnit
        ) {
            _kineticConversionModels = kineticConversionFactors?
                .Where(c => c.BiologicalMatrixTo == targetUnit.BiologicalMatrix)
                .ToDictionary(c => (
                    c.SubstanceFrom, 
                    c.BiologicalMatrixFrom
                ));
            // TODO: add inverse conversion factors for the records for which the target
            // matrix is the source.
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
                if (_kineticConversionModels?.TryGetValue((substance, sourceUnit.BiologicalMatrix), out var conversionRecord) ?? false) {
                    var result = conversionRecord.ConversionFactor * concentration;
                    // TODO: still unit conversion / alignment?
                    return result;
                } else {
                    return double.NaN;
                }
            }
        }
    }
}
