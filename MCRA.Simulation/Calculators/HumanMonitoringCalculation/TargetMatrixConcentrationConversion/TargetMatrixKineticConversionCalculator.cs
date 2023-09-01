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
        /// <param name="targetUnit"></param>
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
        public double GetTargetConcentration(
            double concentration,
            Compound substance,
            ExpressionType sourceExpressionType,
            TargetUnit targetUnit,
            BiologicalMatrix sourceMatrix
        ) {
            if (sourceMatrix == targetUnit.BiologicalMatrix) {
                // If source equals target, then no matrix conversion
                // TODO: still unit conversion / alignment?
                return concentration;
            } else {
                // Apply conversion using the factor and update units
                if (_kineticConversionModels?.TryGetValue((substance, sourceExpressionType, sourceMatrix, targetUnit.ExpressionType), out var conversionRecord) ?? false) {
                    var result = _align(conversionRecord, targetUnit) * concentration;
                    // TODO: still unit conversion / alignment?
                    // zulk soort berekeningetjes, via unit test anders is het niet te volgen.
                    // see TargetMatrixKineticConversionCalculatorTests
                    return result;
                } else {
                    return double.NaN;
                }
            }
        }

        private double _align(KineticConversionFactor record, TargetUnit targetUnit) {
            var targetAmountUnit = targetUnit.SubstanceAmountUnit;

            var amountUnitFrom = record.DoseUnitFrom.GetSubstanceAmountUnit();
            var amountUnitTo = record.DoseUnitTo.GetSubstanceAmountUnit();

            //Align doseUnitFrom and doseUnitTo
            var multiplier2 = amountUnitFrom.GetMultiplicationFactor(amountUnitTo, 1);
            var alignedSource = record.ConversionFactor / multiplier2;

            //Bring to targetUnit scale, align on doseUnitFrom
            var multiplier4 = amountUnitFrom.GetMultiplicationFactor(targetAmountUnit, 1);
            var alignedResult = alignedSource * multiplier4;
            return alignedResult;
        }
    }
}
