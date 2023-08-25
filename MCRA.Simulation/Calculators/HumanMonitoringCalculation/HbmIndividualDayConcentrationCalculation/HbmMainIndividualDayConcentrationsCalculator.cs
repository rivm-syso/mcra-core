using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Units;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmMainIndividualDayConcentrationsCalculator : HbmIndividualDayConcentrationBaseCalculator {

        public HbmMainIndividualDayConcentrationsCalculator(TargetUnit targetUnit) {
            BiologicalMatrixConversionCalculator = new SimpleTargetMatrixConversionCalculator(1d, targetUnit);
        }

        /// <summary>
        /// Computes the HBM individual day concentrations for the specified
        /// target biological matrix based on the HBM data in the sample 
        /// substance collection.
        /// </summary>
        public Dictionary<(Individual individual, string idDay), HbmIndividualDayConcentration> Calculate(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ICollection<IndividualDay> individualDays,
            ICollection<Compound> substances,
            BiologicalMatrix targetBiologicalMatrix,
            ConcentrationUnit concentrationUnit,
            TimeScaleUnit timeScaleUnit,
            TargetUnitsModel targetUnitsModel
        ) {
            // Compute HBM individual concentrations for the sample substance
            // collection matching the target biological matrix.
            // TODO: account for the cases when the same matrix is measured with
            // multiple sampling methods (e.g., 24h and spot urine).
            var mainSampleSubstanceCollection = hbmSampleSubstanceCollections
                .FirstOrDefault(x => x.SamplingMethod.BiologicalMatrix == targetBiologicalMatrix);

            var individualDayConcentrations = Compute(
                mainSampleSubstanceCollection,
                individualDays,
                substances,
                targetBiologicalMatrix,
                concentrationUnit,
                timeScaleUnit,
                targetUnitsModel
            );

            return individualDayConcentrations;
        }
    }
}
