using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation {
    public class HbmIndividualDayConcentrationBaseCalculator {

        public ITargetMatrixConversionCalculator BiologicalMatrixConversionCalculator { get; set; }

        public ICollection<HbmIndividualDayConcentration> Compute(
            HumanMonitoringSampleSubstanceCollection sampleSubstanceCollection,
            ICollection<IndividualDay> individualDays,
            ICollection<Compound> substances,
            TargetUnit targetUnit
        ) {
            var result = new List<HbmIndividualDayConcentration>();

            // Group samples by individual day
            var samplesPerIndividualDay = sampleSubstanceCollection?
                .HumanMonitoringSampleSubstanceRecords
                .ToLookup(r => (Individual: r.Individual, IdDay: r.Day));

            var individualDayIdCounter = 0;
            foreach (var individualDay in individualDays) {
                // TODO: refactor this; we should not regenerate simulated individual ids
                // instead, better to pass a collection of simulated individual days.
                var simulatedIndividualDayId = individualDayIdCounter++;
                if (samplesPerIndividualDay.Contains((individualDay.Individual, individualDay.IdDay))) {
                    var groupedSample = samplesPerIndividualDay[(individualDay.Individual, individualDay.IdDay)];
                    var concentrationsBySubstance = computeConcentrationsBySubstance(
                        groupedSample.ToList(),
                        substances,
                        sampleSubstanceCollection.SamplingMethod,
                        sampleSubstanceCollection.ExpressionType,
                        sampleSubstanceCollection.ConcentrationUnit,
                        targetUnit
                    );
                    var individualDayConcentration = new HbmIndividualDayConcentration() {
                        SimulatedIndividualId = individualDay.Individual.Id,
                        SimulatedIndividualDayId = simulatedIndividualDayId,
                        Individual = individualDay.Individual,
                        IndividualSamplingWeight = individualDay.Individual.SamplingWeight,
                        Day = individualDay.IdDay,
                        ConcentrationsBySubstance = concentrationsBySubstance
                            .ToDictionary(o => o.Key, o => (IHbmSubstanceTargetExposure)o.Value)
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
                            concentration: g.Any() ? g.Average(r => r.Concentration) : double.NaN
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
                        var concentration = BiologicalMatrixConversionCalculator
                            .GetTargetConcentration(
                                averageConcentration,
                                g.Key,
                                expressionTypeSource,
                                samplingMethodSource.BiologicalMatrix,
                                sourceConcentrationUnit,
                                targetUnit
                            );
                        return new HbmSubstanceTargetExposure() {
                            Substance = g.Key,
                            Concentration = concentration,
                            Unit = targetUnit,
                            SourceSamplingMethods = new List<HumanMonitoringSamplingMethod>() {
                                samplingMethodSource
                            },
                            BiologicalMatrix = targetUnit.BiologicalMatrix
                        };
                    }
                );
            return result;
        }

        private List<HbmSubstanceTargetExposure> getConcentrationsBySubstance(
            SampleCompound sampleSubstance
        ) {
            var result = new List<HbmSubstanceTargetExposure>();
            if (sampleSubstance.IsPositiveResidue || sampleSubstance.IsZeroConcentration) {
                var exposure = new HbmSubstanceTargetExposure() {
                    Substance = sampleSubstance.ActiveSubstance,
                    Concentration = sampleSubstance.Residue
                };
                result.Add(exposure);
            }
            return result;
        }
    }
}
