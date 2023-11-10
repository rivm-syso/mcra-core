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
        private readonly ILookup<(Compound, ExposureTarget), KineticConversionFactor> _kineticConversionModels;

        private readonly TargetUnit _targetUnit;

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
            _targetUnit = targetUnit;
            _kineticConversionModels = kineticConversionFactors?
                .Where(c => c.TargetTo == targetUnit.Target)
                .ToLookup(c => (
                    c.SubstanceFrom,
                    c.TargetFrom
                ));
        }

        public ICollection<HbmSubstanceTargetExposure> GetTargetSubstanceExposure(
            HbmSubstanceTargetExposure sourceExposure,
            TargetUnit sourceExposureUnit
        ) {
            var result = new List<HbmSubstanceTargetExposure>();
            var substance = sourceExposure.Substance;
            if (sourceExposureUnit.Target == _targetUnit.Target) {
                // If source equals target, then no matrix conversion

                // Alignment factor for source-unit of concentration to target unit
                var unitAlignmentFactor = sourceExposureUnit.GetAlignmentFactor(
                    _targetUnit,
                    substance.MolecularMass,
                    double.NaN
                );
                var record = new HbmSubstanceTargetExposure() {
                    SourceSamplingMethods = sourceExposure.SourceSamplingMethods,
                    Concentration = unitAlignmentFactor * sourceExposure.Concentration,
                    IsAggregateOfMultipleSamplingMethods = sourceExposure.IsAggregateOfMultipleSamplingMethods,
                    Substance = sourceExposure.Substance,
                    Target = _targetUnit.Target
                };
                result.Add(record);
            } else if (_kineticConversionModels.Contains((substance, sourceExposureUnit.Target))) {
                var conversions = _kineticConversionModels[(substance, sourceExposureUnit.Target)];
                var resultRecords = conversions
                    .Select(c => new HbmSubstanceTargetExposure() {
                        SourceSamplingMethods = sourceExposure.SourceSamplingMethods,
                        Concentration = convertMatrixConcentration(
                            sourceExposure.Concentration,
                            sourceExposureUnit.ExposureUnit,
                            c
                        ),
                        IsAggregateOfMultipleSamplingMethods = sourceExposure.IsAggregateOfMultipleSamplingMethods,
                        Substance = c.SubstanceTo,
                        Target = _targetUnit.Target
                    })
                    .ToList();
                result.AddRange(resultRecords);
            }
            return result;
        }

        private double convertMatrixConcentration(
            double concentration,
            ExposureUnitTriple sourceExposureUnit,
            KineticConversionFactor record
        ) {
            // Alignment factor for source-unit of concentration with from-unit of conversion record
            var sourceUnitAlignmentFactor = sourceExposureUnit.GetAlignmentFactor(
                record.DoseUnitFrom,
                record.SubstanceFrom.MolecularMass,
                double.NaN
            );

            // Alignment factor for to-unit of the conversion record with the target unit
            var targetUnitAlignmentFactor = record.DoseUnitTo.GetAlignmentFactor(
                _targetUnit.ExposureUnit,
                record.SubstanceTo.MolecularMass,
                double.NaN
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
