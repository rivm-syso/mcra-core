﻿using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.KineticConversions {

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
        private readonly ILookup<(Compound, ExposureTarget), IKineticConversionFactorModel> _kineticConversionModels;

        private readonly TargetUnit _targetUnit;

        /// <summary>
        /// Creates a new instance of a <see cref="TargetMatrixKineticConversionCalculator"/>.
        /// Select only the conversion records for the given target biological matrix
        /// </summary>
        /// <param name="kineticConversionFactors"></param>
        /// <param name="targetUnit"></param>
        public TargetMatrixKineticConversionCalculator(
            ICollection<IKineticConversionFactorModel> kineticConversionFactors,
            TargetUnit targetUnit
        ) {
            _targetUnit = targetUnit;
            _kineticConversionModels = kineticConversionFactors?
                .Where(c => c.ConversionRule.TargetTo == targetUnit.Target)
                .ToLookup(c => (
                    c.ConversionRule.SubstanceFrom,
                    c.ConversionRule.TargetFrom
                ));
        }

        public ICollection<HbmSubstanceTargetExposure> GetSubstanceTargetExposures(
            HbmSubstanceTargetExposure sourceExposure,
            SimulatedIndividualDay individualDay,
            TargetUnit sourceExposureUnit,
            double compartmentWeight
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
                    Exposure = unitAlignmentFactor * sourceExposure.Exposure,
                    IsAggregateOfMultipleSamplingMethods = sourceExposure.IsAggregateOfMultipleSamplingMethods,
                    Substance = sourceExposure.Substance
                };
                result.Add(record);
            } else if (_kineticConversionModels.Contains((substance, sourceExposureUnit.Target))) {
                var conversions = _kineticConversionModels[(substance, sourceExposureUnit.Target)];
                var age = individualDay.SimulatedIndividual?.Age;
                var genderType = individualDay.SimulatedIndividual?.Gender ?? GenderType.Undefined;
                var resultRecords = conversions
                    .Select(c => new HbmSubstanceTargetExposure() {
                        SourceSamplingMethods = sourceExposure.SourceSamplingMethods,
                        Exposure = convertMatrixConcentration(
                            sourceExposure.Exposure,
                            sourceExposureUnit.ExposureUnit,
                            c,
                            compartmentWeight
                        ) * c.GetConversionFactor(age, genderType),
                        IsAggregateOfMultipleSamplingMethods = sourceExposure.IsAggregateOfMultipleSamplingMethods,
                        Substance = c.ConversionRule.SubstanceTo
                    })
                    .ToList();
                result.AddRange(resultRecords);
            }
            return result;
        }

        private double convertMatrixConcentration(
            double concentration,
            ExposureUnitTriple sourceExposureUnit,
            IKineticConversionFactorModel record,
            double compartmentWeight
        ) {
            // Alignment factor for source-unit of concentration with from-unit of conversion record
            var sourceUnitAlignmentFactor = sourceExposureUnit.GetAlignmentFactor(
                record.ConversionRule.DoseUnitFrom,
                record.ConversionRule.SubstanceFrom.MolecularMass,
                double.NaN
            );

            // Alignment factor for to-unit of the conversion record with the target unit
            var targetUnitAlignmentFactor = record.ConversionRule.DoseUnitTo.GetAlignmentFactor(
                _targetUnit.ExposureUnit,
                record.ConversionRule.SubstanceTo.MolecularMass,
                compartmentWeight
            );

            // The factor belongs to the combination of dose-unit-from and dose-unit-to.
            // Compute the result by aligning the (source) concentration with the dose-unit-from,
            // applying the conversion factor, and then aligning the result with the alignment
            // factor of the dose-unit-to with the target unit.
            var result = concentration * sourceUnitAlignmentFactor * targetUnitAlignmentFactor;
            return result;
        }
    }
}
