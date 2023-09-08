using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmIndividualDayConcentrationsCalculator : HbmIndividualDayConcentrationCalculatorBase {

        /// <summary>
        /// Computes the HBM individual day concentrations from the sample substance collection.
        /// </summary>
        public HbmIndividualDayCollection Calculate(
            HumanMonitoringSampleSubstanceCollection hbmSampleSubstanceCollection,
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances
        ) {
            var targetUnit = new TargetUnit(
                hbmSampleSubstanceCollection.ConcentrationUnit.GetSubstanceAmountUnit(),
                hbmSampleSubstanceCollection.ConcentrationUnit.GetConcentrationMassUnit(),
                TimeScaleUnit.Unspecified,
                hbmSampleSubstanceCollection.SamplingMethod.BiologicalMatrix,
                hbmSampleSubstanceCollection.ExpressionType
            );

            var individualDayConcentrations = Compute(
                hbmSampleSubstanceCollection,
                individualDays,
                substances,
                targetUnit
            );

            return new HbmIndividualDayCollection() {
                TargetUnit = targetUnit,
                HbmIndividualDayConcentrations = individualDayConcentrations
            };
        }

        protected override double getTargetConcentration(
            HumanMonitoringSamplingMethod samplingMethodSource,
            ExpressionType expressionTypeSource,
            ConcentrationUnit sourceConcentrationUnit,
            TargetUnit targetUnit,
            Compound substance,
            double averageConcentration
        ) {
            var unitAlignmentFactor = ConcentrationUnitExtensions.GetConcentrationAlignmentFactor(
                sourceConcentrationUnit,
                targetUnit,
                substance.MolecularMass
            );
            return averageConcentration * unitAlignmentFactor;
        }
    }
}
