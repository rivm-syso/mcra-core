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
    public class SimpleBiologicalMatrixConcentrationConversionCalculator : IBiologicalMatrixConcentrationConversionCalculator {

        /// <summary>
        /// Creates a new instance of a <see cref="SimpleBiologicalMatrixConcentrationConversionCalculator"/>.
        /// </summary>
        /// <param name="conversionFactor"></param>
        public SimpleBiologicalMatrixConcentrationConversionCalculator(double conversionFactor) {
            ConversionFactor = conversionFactor;
        }

        /// <summary>
        /// Conversion factor to translate a concentration of one compartment
        /// to a concentration of another compartment.
        /// </summary>
        public double ConversionFactor { get; set; }

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

                return ConversionFactor * concentration;
            }
        }
    }
}
