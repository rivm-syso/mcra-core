using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;
using MCRA.Simulation.Units;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion {

    /// <summary>
    /// Calculator class for computing biological matrix concentrations
    /// for a target matrix based on a concentration value of a sampling
    /// method of another biological matrix using a simple conversion factor
    /// that is the same for all biological matrices other than the target
    /// biological matrix.
    /// </summary>
    public class ComplexBiologicalMatrixConcentrationConversionCalculator : IBiologicalMatrixConcentrationConversionCalculator {

        /// <summary>
        /// Dictionary with relevant kinetic conversion models
        /// </summary>
        private Dictionary<(Compound, BiologicalMatrix), (KineticConversionModel kineticModel, List<TargetUnit> targetUnits)> _kineticConversionModels { get; set; }

        /// <summary>
        /// Creates a new instance of a <see cref="ComplexBiologicalMatrixConcentrationConversionCalculator"/>.
        /// Select only the conversion records for the given target biological matrix
        /// </summary>
        /// <param name="conversionFactor"></param>
        public ComplexBiologicalMatrixConcentrationConversionCalculator(
                ICollection<KineticConversionModel> kineticConversionFactors,
                BiologicalMatrix targetBiologicalMatrix,
                TargetUnitsModel targetUnitsModel
            ) {
            _kineticConversionModels = kineticConversionFactors?
             .Where(c => c.BiologicalMatrixTo == targetBiologicalMatrix)
             .Select(r => {
                 return (
                    KineticModelConversionFactor: r,
                    TargetUnits: targetUnitsModel.GetTargetUnits(r.SubstanceFrom)
                 );
             }).ToDictionary(c => (c.Item1.SubstanceFrom, c.Item1.BiologicalMatrixFrom));
        }

        /// <summary>
        /// Gets the converted concentration for the target biological matrix
        /// based on a concentration of the source biological matrix.
        /// </summary>
        public double GetTargetConcentration(
            HumanMonitoringSamplingMethod sourceSamplingMethod,
            BiologicalMatrix targetBiologicalMatrix,
            ConcentrationUnit concentrationUnit,
            TimeScaleUnit timeScaleUnit,
            TargetUnitsModel targetUnitsModel,
            Compound substance,
            double concentration
        ) {
            if (sourceSamplingMethod.BiologicalMatrix == targetBiologicalMatrix) {
                // If source equals target, then no conversion
                return concentration;
            } else {
                // Apply conversion using the factor and update units
                targetUnitsModel.Update(substance, sourceSamplingMethod.BiologicalMatrix,
                        new TargetUnit(concentrationUnit.GetSubstanceAmountUnit(), concentrationUnit.GetConcentrationMassUnit(), timeScaleUnit, targetBiologicalMatrix));
                if (_kineticConversionModels?.TryGetValue((substance, sourceSamplingMethod.BiologicalMatrix), out var conversionRecord) ?? false) {
                    return conversionRecord.kineticModel.ConversionFactor * concentration;
                } else {
                    return concentration;
                }
            }
        }

        /// <summary>
        /// Correct the kinetic model conversion factor for the possibly that the from and/or to units of the conversion factor are
        /// different than the units used for the source and target biological matrices of the HBM concentration values.
        /// </summary>
        private double getUnitAlignedConversionFactor(
            KineticConversionModel kineticConversionModel,
            TargetUnit targetUnitFrom
        ) {
            var doseUnitAlignmentFactorFrom = kineticConversionModel.DoseUnitFrom.GetDoseAlignmentFactor(targetUnitFrom);
            var doseUnitAlignmentFactorTo = kineticConversionModel.DoseUnitTo.GetDoseAlignmentFactor(targetUnitFrom);
            return (doseUnitAlignmentFactorTo / doseUnitAlignmentFactorFrom) * kineticConversionModel.ConversionFactor;
        }
    }
}
