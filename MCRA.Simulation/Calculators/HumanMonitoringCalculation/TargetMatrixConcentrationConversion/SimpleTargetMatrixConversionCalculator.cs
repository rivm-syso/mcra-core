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
        /// Conversion factor to translate a concentration of one compartment
        /// to a concentration of another compartment.
        /// </summary>
        private readonly double _conversionFactor;

        /// <summary>
        /// Creates a new instance of a <see cref="SimpleTargetMatrixConversionCalculator"/>.
        /// </summary>
        /// <param name="conversionFactor"></param>
        public SimpleTargetMatrixConversionCalculator(
            double conversionFactor
        ) {
            _conversionFactor = conversionFactor;
        }

        /// <summary>
        /// Gets the converted concentration for the target biological matrix
        /// based on a concentration of the source biological matrix.
        /// </summary>
        public double GetTargetConcentration(
            double concentration,
            Compound substance,
            ExpressionType sourceExpressionType,
            BiologicalMatrix sourceMatrix,
            ConcentrationUnit sourceConcentrationUnit,
            TargetUnit targetUnit
        ) {
            if (sourceMatrix == targetUnit.BiologicalMatrix
                && sourceExpressionType == targetUnit.ExpressionType
            ) {
                // If source equals target, then no matrix conversion

                // Alignment factor for source-unit of concentration to target unit
                var unitAlignmentFactor = ConcentrationUnitExtensions.GetConcentrationAlignmentFactor(
                    sourceConcentrationUnit,
                    targetUnit.ExposureUnit,
                    substance.MolecularMass
                );

                return unitAlignmentFactor * concentration;
            } else {
                // Alignment factor for source-unit of concentration to target unit
                var unitAlignmentFactor = ConcentrationUnitExtensions.GetConcentrationAlignmentFactor(
                    sourceConcentrationUnit,
                    targetUnit.ExposureUnit,
                    substance.MolecularMass
                );

                // Apply conversion using the factor
                return _conversionFactor * unitAlignmentFactor * concentration;
            }
        }
    }
}
