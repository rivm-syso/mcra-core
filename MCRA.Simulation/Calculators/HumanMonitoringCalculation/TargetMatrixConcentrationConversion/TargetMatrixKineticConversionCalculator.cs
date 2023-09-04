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
    public class TargetMatrixKineticConversionCalculator : ITargetMatrixConversionCalculator {

        /// <summary>
        /// Dictionary with relevant kinetic conversion models
        /// </summary>
        private readonly Dictionary<(Compound, ExpressionType, BiologicalMatrix, ExpressionType), KineticConversionFactor> _kineticConversionModels;

        /// <summary>
        /// Creates a new instance of a <see cref="TargetMatrixKineticConversionCalculator"/>.
        /// Select only the conversion records for the given target biological matrix
        /// </summary>
        /// <param name="kineticConversionFactors"></param>
        /// <param name="biologicalMatrix"></param>
        public TargetMatrixKineticConversionCalculator(
            ICollection<KineticConversionFactor> kineticConversionFactors,
            BiologicalMatrix biologicalMatrix
        ) {
            _kineticConversionModels = kineticConversionFactors?
                .Where(c => c.BiologicalMatrixTo == biologicalMatrix)
                .ToDictionary(c => (
                    c.SubstanceFrom,
                    c.ExpressionTypeFrom,
                    c.BiologicalMatrixFrom,
                    c.ExpressionTypeTo
                ));
            // TODO: add inverse conversion factors for the records for which the target
            // matrix is the source.
        }

        /// <summary>
        /// Gets the converted concentration for the target biological matrix
        /// based on a concentration of the source biological matrix.
        /// </summary>
        /// <param name="concentration"></param>
        /// <param name="substance"></param>
        /// <param name="sourceExpressionType"></param>
        /// <param name="sourceMatrix"></param>
        /// <param name="sourceUnit"></param>
        /// <param name="targetUnit"></param>
        /// <returns></returns>
        public double GetTargetConcentration(
            double concentration,
            Compound substance,
            ExpressionType sourceExpressionType,
            BiologicalMatrix sourceMatrix,
            ConcentrationUnit sourceUnit,
            TargetUnit targetUnit
        ) {
            if (sourceMatrix == targetUnit.BiologicalMatrix) {
                // If source equals target, then no matrix conversion
                // TODO: still unit conversion / alignment?
                return concentration;
            } else {
                // Apply conversion using the factor and update units
                if (_kineticConversionModels?.TryGetValue(
                        (substance, sourceExpressionType, sourceMatrix, targetUnit.ExpressionType), 
                        out var conversionRecord) ?? false
                ) {
                    var result = _align(
                        concentration,
                        substance,
                        sourceUnit,
                        conversionRecord, 
                        targetUnit
                    );
                    // TODO: still unit conversion / alignment?
                    // zulk soort berekeningetjes, via unit test anders is het niet te volgen.
                    // see TargetMatrixKineticConversionCalculatorTests
                    return result;
                } else {
                    return double.NaN;
                }
            }
        }

        private double _align(
            double concentration,
            Compound substance,
            ConcentrationUnit concentrationUnit,
            KineticConversionFactor record, 
            TargetUnit targetUnit
        ) {
            // Step 1: get alignment factor for source-unit of concentration with from-unit of conversion record
            var sourceUnitAlignmentFactor = ConcentrationUnitExtensions.GetConcentrationAlignmentFactor(
                concentrationUnit,
                record.DoseUnitFrom,
                substance.MolecularMass
            );

            // Step 2: get alignment factor for to-unit of the conversion record with the target unit
            var targetUnitAlignmentFactor = ConcentrationUnitExtensions.GetConcentrationAlignmentFactor(
                record.DoseUnitTo,
                targetUnit,
                substance.MolecularMass
            );

            // The factor belongs to the combination of dose-unit-from and dose-unit-to.
            // Compute the result by aligning the (source) concentration with the dose-unit-from,
            // applying the conversion factor, and then aligning the result with the alignment
            // factor of the dose-unit-to with the target unit.
            var result = concentration * sourceUnitAlignmentFactor * record.ConversionFactor * targetUnitAlignmentFactor;
            return result;
        }
    }
}
