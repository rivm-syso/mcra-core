using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Units;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmMainIndividualDayConcentrationsCalculator : HbmIndividualDayConcentrationBaseCalculator {


        public HbmMainIndividualDayConcentrationsCalculator(BiologicalMatrix biologicalMatrix) {
            BiologicalMatrixConversionCalculator = new SimpleTargetMatrixConversionCalculator(1d, biologicalMatrix);
        }

        /// <summary>
        /// Computes the HBM individual day concentrations for the specified
        /// target biological matrix based on the HBM data in the sample 
        /// substance collection.
        /// </summary>
        public ICollection<BiologicalMatrixExpressionTypeHBMCollections> Calculate(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ICollection<IndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<TargetUnit> targetUnits
        ) {
            // Compute HBM individual concentrations for the sample substance
            // collection matching the target biological matrix.
            // TODO: account for the cases when the same matrix is measured with
            // multiple sampling methods (e.g., 24h and spot urine).
            // Currently, only 1 biological matrix is allowed but expressionTypes can be > 1
            var targetBiologicalMatrix = targetUnits.FirstOrDefault().BiologicalMatrix;
            var targetHbmSampleSubstanceCollections = hbmSampleSubstanceCollections
                .Where(x => x.SamplingMethod.BiologicalMatrix == targetBiologicalMatrix)
                .ToList();
            var biologicalMatrixExpressionTypeHbmCollections = new List<BiologicalMatrixExpressionTypeHBMCollections>();

            foreach (var hbmSampleSubstanceCollection in targetHbmSampleSubstanceCollections) {
                var targetUnit = targetUnits.Single(c => c.BiologicalMatrix == targetBiologicalMatrix
                    && c.ExpressionType == hbmSampleSubstanceCollection.ExpressionType
                );
                var individualDayConcentrations = Compute(
                    hbmSampleSubstanceCollection,
                    individualDays,
                    substances,
                    targetUnit
                );
                biologicalMatrixExpressionTypeHbmCollections.Add(new BiologicalMatrixExpressionTypeHBMCollections() {
                    BiologicalMatrix = targetBiologicalMatrix,
                    ExpressionType = hbmSampleSubstanceCollection.ExpressionType,
                    HbmIndividualDayConcentrationCollections = individualDayConcentrations
                });
            }
            return biologicalMatrixExpressionTypeHbmCollections;
        }
    }
}
