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

        private readonly TargetUnit _targetUnit;

        /// <summary>
        /// Creates a new instance of a <see cref="SimpleTargetMatrixConversionCalculator"/>.
        /// </summary>
        /// <param name="targetUnit"></param>
        /// <param name="conversionFactor"></param>
        public SimpleTargetMatrixConversionCalculator(
            TargetUnit targetUnit,
            double conversionFactor
        ) {
            _targetUnit = targetUnit;
            _conversionFactor = conversionFactor;
        }

        public ICollection<HbmSubstanceTargetExposure> GetTargetSubstanceExposure(
            HbmSubstanceTargetExposure sourceExposure,
            TargetUnit sourceExposureUnit,
            double compartmentWeight
        ) {
            var result = new List<HbmSubstanceTargetExposure>();
            var record = new HbmSubstanceTargetExposure() {
                SourceSamplingMethods = sourceExposure.SourceSamplingMethods,
                Concentration = getTargetConcentration(
                    sourceExposure.Concentration,
                    sourceExposure.Substance,
                    sourceExposureUnit.Target,
                    sourceExposureUnit.ExposureUnit
                ),
                IsAggregateOfMultipleSamplingMethods = sourceExposure.IsAggregateOfMultipleSamplingMethods,
                Substance = sourceExposure.Substance,
                Target = _targetUnit.Target
            };
            result.Add(record);
            return result;
        }

        /// <summary>
        /// Gets the converted concentration for the target biological matrix
        /// based on a concentration of the source biological matrix.
        /// </summary>
        /// <param name="concentration"></param>
        /// <param name="substance"></param>
        /// <param name="sourceExposureTarget"></param>
        /// <param name="sourceUnit"></param>
        /// <returns></returns>
        private double getTargetConcentration(
            double concentration,
            Compound substance,
            ExposureTarget sourceExposureTarget,
            ExposureUnitTriple sourceUnit
        ) {
            if (sourceExposureTarget == _targetUnit.Target) {
                // If source equals target, then no matrix conversion

                // Alignment factor for source-unit of concentration to target unit
                var unitAlignmentFactor = sourceUnit.GetAlignmentFactor(
                    _targetUnit.ExposureUnit,
                    substance.MolecularMass,
                    double.NaN
                );

                return unitAlignmentFactor * concentration;
            } else {
                // Alignment factor for source-unit of concentration to target unit
                var unitAlignmentFactor = sourceUnit.GetAlignmentFactor(
                    _targetUnit.ExposureUnit,
                    substance.MolecularMass,
                    double.NaN
                );

                // Apply conversion using the factor
                return _conversionFactor * unitAlignmentFactor * concentration;
            }
        }
    }
}
