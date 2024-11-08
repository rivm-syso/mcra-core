using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation {
    public abstract class HbmIndividualDayConcentrationCalculatorBase {

        public ICollection<HbmIndividualDayConcentration> createHbmIndividualDayConcentrations(
            HumanMonitoringSampleSubstanceCollection sampleSubstanceCollection,
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            TargetUnit targetUnit
        ) {
            var result = new List<HbmIndividualDayConcentration>();

            var target = new ExposureTarget(
                sampleSubstanceCollection.BiologicalMatrix, 
                ExpressionType.None
            );

            // Group samples by individual day
            var samplesPerIndividualDay = sampleSubstanceCollection?
                .HumanMonitoringSampleSubstanceRecords
                .ToLookup(r => (r.Individual, r.Day));

            foreach (var simulatedIndividualDay in simulatedIndividualDays) {
                if (samplesPerIndividualDay.Contains((simulatedIndividualDay.Individual, simulatedIndividualDay.Day))) {
                    var groupedSample = samplesPerIndividualDay[(simulatedIndividualDay.Individual, simulatedIndividualDay.Day)];
                    var concentrationsBySubstance = computeConcentrationsBySubstance(
                        groupedSample.ToList(),
                        substances,
                        sampleSubstanceCollection.SamplingMethod,
                        sampleSubstanceCollection.ExpressionType,
                        sampleSubstanceCollection.ConcentrationUnit,
                        targetUnit
                    );
                    var individualDayConcentration = new HbmIndividualDayConcentration() {
                        SimulatedIndividualId = simulatedIndividualDay.SimulatedIndividualId,
                        SimulatedIndividualDayId = simulatedIndividualDay.SimulatedIndividualDayId,
                        Individual = simulatedIndividualDay.Individual,
                        IndividualSamplingWeight = simulatedIndividualDay.Individual.SamplingWeight,
                        Day = simulatedIndividualDay.Day,
                        SimulatedIndividualBodyWeight = simulatedIndividualDay.IndividualBodyWeight,
                        ConcentrationsBySubstance = concentrationsBySubstance
                            .ToDictionary(o => o.Key, o => o.Value)
                    };
                    result.Add(individualDayConcentration);
                }
            }
            return result;
        }

        private Dictionary<Compound, HbmSubstanceTargetExposure> computeConcentrationsBySubstance(
           ICollection<HumanMonitoringSampleSubstanceRecord> individualDaySamples,
           ICollection<Compound> substances,
           HumanMonitoringSamplingMethod samplingMethodSource,
           ExpressionType expressionTypeSource,
           ConcentrationUnit sourceConcentrationUnit,
           TargetUnit targetUnit
       ) {
            var result = individualDaySamples
                .SelectMany(sample => {
                    var sampleIntakesBySubstance = sample.HumanMonitoringSampleSubstances.Values
                        .SelectMany(r => getConcentrationsBySubstance(r))
                        .GroupBy(r => r.Substance)
                        .Select(g => (
                            substance: g.Key,
                            concentration: g.Any() ? g.Average(r => r.Exposure) : double.NaN
                        )
                    );
                    return sampleIntakesBySubstance;
                })
                .GroupBy(r => r.substance)
                .Where(r => substances.Contains(r.Key))
                .ToDictionary(
                    g => g.Key,
                    g => {
                        var averageConcentration = g.Any() ? g.Average(r => r.concentration) : double.NaN;
                        var concentration = getTargetConcentration(
                            samplingMethodSource, 
                            expressionTypeSource, 
                            sourceConcentrationUnit, 
                            targetUnit, 
                            g.Key,
                            averageConcentration
                        );
                        return new HbmSubstanceTargetExposure() {
                            Substance = g.Key,
                            Exposure = concentration,
                            SourceSamplingMethods = [
                                samplingMethodSource
                            ]
                        };
                    }
                );
            return result;
        }

        protected abstract double getTargetConcentration(
            HumanMonitoringSamplingMethod samplingMethodSource,
            ExpressionType expressionTypeSource,
            ConcentrationUnit sourceConcentrationUnit,
            TargetUnit targetUnit,
            Compound substance,
            double averageConcentration
        );

        private List<HbmSubstanceTargetExposure> getConcentrationsBySubstance(
            SampleCompound sampleSubstance
        ) {
            var result = new List<HbmSubstanceTargetExposure>();
            if (sampleSubstance.IsPositiveResidue || sampleSubstance.IsZeroConcentration) {
                var exposure = new HbmSubstanceTargetExposure() {
                    Substance = sampleSubstance.ActiveSubstance,
                    Exposure = sampleSubstance.Residue
                };
                result.Add(exposure);
            }
            return result;
        }
    }
}
