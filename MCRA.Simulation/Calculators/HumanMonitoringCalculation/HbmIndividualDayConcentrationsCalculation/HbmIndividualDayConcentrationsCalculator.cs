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
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances
        ) {
            var target = new ExposureTarget(
                hbmSampleSubstanceCollection.SamplingMethod.BiologicalMatrix,
                hbmSampleSubstanceCollection.ExpressionType
            );

            var targetUnit = new TargetUnit(
                target,
                hbmSampleSubstanceCollection.ConcentrationUnit.GetSubstanceAmountUnit(),
                hbmSampleSubstanceCollection.ConcentrationUnit.GetConcentrationMassUnit(),
                TimeScaleUnit.Unspecified
            );

            var individualDayConcentrations = createHbmIndividualDayConcentrations(
                hbmSampleSubstanceCollection,
                simulatedIndividualDays,
                substances,
                targetUnit
            );

            return new HbmIndividualDayCollection() {
                TargetUnit = targetUnit,
                HbmIndividualDayConcentrations = individualDayConcentrations
            };
        }

        public static HbmIndividualDayCollection CreateDefaultHbmIndividualDayCollection(
            List<SimulatedIndividualDay> individualDays,
            ExposureTarget target
        ) {
            var hbmIndividualDayConcentrations = new List<HbmIndividualDayConcentration>();
            foreach (var individualDay in individualDays) {
                var individualDayConcentration = new HbmIndividualDayConcentration() {
                    SimulatedIndividualId = individualDay.SimulatedIndividualId,
                    SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                    Individual = individualDay.Individual,
                    IndividualSamplingWeight = individualDay.Individual.SamplingWeight,
                    SimulatedIndividualBodyWeight = individualDay.IndividualBodyWeight, 
                    Day = individualDay.Day,
                    ConcentrationsBySubstance = new Dictionary<Compound, HbmSubstanceTargetExposure>()
                };
                hbmIndividualDayConcentrations.Add(individualDayConcentration);
            }
            var result = new HbmIndividualDayCollection() {
                TargetUnit = new TargetUnit(
                    target,
                    target.TargetLevelType == TargetLevelType.Internal
                        ? ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerL)
                        : ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay)
                ),
                HbmIndividualDayConcentrations = hbmIndividualDayConcentrations
            };
            return result;
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
                targetUnit.ExposureUnit,
                substance.MolecularMass
            );
            return averageConcentration * unitAlignmentFactor;
        }
    }
}
