using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation {
    public abstract class HbmIndividualDayConcentrationCalculatorBase {

        public ICollection<HbmIndividualDayConcentration> Compute(
            HumanMonitoringSampleSubstanceCollection sampleSubstanceCollection,
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances,
            TargetUnit targetUnit
        ) {
            var result = new List<HbmIndividualDayConcentration>();

            // Group samples by individual day
            var samplesPerIndividualDay = sampleSubstanceCollection?
                .HumanMonitoringSampleSubstanceRecords
                .ToLookup(r => (r.Individual, r.Day));

            foreach (var individualDay in individualDays) {
                if (samplesPerIndividualDay.Contains((individualDay.Individual, individualDay.Day))) {
                    var groupedSample = samplesPerIndividualDay[(individualDay.Individual, individualDay.Day)];
                    var concentrationsBySubstance = computeConcentrationsBySubstance(
                        groupedSample.ToList(),
                        substances,
                        sampleSubstanceCollection.SamplingMethod,
                        sampleSubstanceCollection.ExpressionType,
                        sampleSubstanceCollection.ConcentrationUnit,
                        targetUnit
                    );
                    var individualDayConcentration = new HbmIndividualDayConcentration() {
                        SimulatedIndividualId = individualDay.SimulatedIndividualId,
                        SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                        Individual = individualDay.Individual,
                        IndividualSamplingWeight = individualDay.Individual.SamplingWeight,
                        Day = individualDay.Day,
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
                    Concentration = sampleSubstance.Residue
                };
                result.Add(exposure);
            }
            return result;
        }
    }
}
