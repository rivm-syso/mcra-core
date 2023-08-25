using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Units;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation {
    public class HbmIndividualDayConcentrationBaseCalculator {
        public ITargetMatrixConversionCalculator BiologicalMatrixConversionCalculator { get; set; }

        public Dictionary<(Individual Individual, string IdDay), HbmIndividualDayConcentration> Compute(
            HumanMonitoringSampleSubstanceCollection sampleSubstanceCollection,
            ICollection<IndividualDay> individualDays,
            ICollection<Compound> substances,
            BiologicalMatrix targetBiologicalMatrix,
            ConcentrationUnit concentrationUnit,
            TimeScaleUnit timeScaleUnit,
            TargetUnitsModel hbmTargetUnitsModel
        ) {
            var individualDayConcentrations = new Dictionary<(Individual Individual, string IdDay), HbmIndividualDayConcentration>();

            var samplingMethod = sampleSubstanceCollection.SamplingMethod;
            var measuredBiologicalMatrix = sampleSubstanceCollection.SamplingMethod.BiologicalMatrix;
            var expressionType = sampleSubstanceCollection.ExpressionType;

            var samplesPerIndividualDay = sampleSubstanceCollection?
                .HumanMonitoringSampleSubstanceRecords
                .ToLookup(r => (Individual: r.Individual, IdDay: r.Day));

            var sourceCompartment = sampleSubstanceCollection.SamplingMethod.BiologicalMatrix;

            var individualDayIdCounter = 0;
            foreach (var individualDay in individualDays) {
                if (samplesPerIndividualDay.Contains((individualDay.Individual, individualDay.IdDay))) {
                    var groupedSample = samplesPerIndividualDay[(individualDay.Individual, individualDay.IdDay)];
                    var concentrationsBySubstance = computeConcentrationsBySubstance(
                        groupedSample.ToList(),
                        substances,
                        samplingMethod,
                        targetBiologicalMatrix,
                        expressionType,
                        concentrationUnit,
                        timeScaleUnit,
                        hbmTargetUnitsModel
                    );
                    var individualDayConcentration = new HbmIndividualDayConcentration() {
                        SimulatedIndividualId = individualDay.Individual.Id,
                        SimulatedIndividualDayId = individualDayIdCounter++,
                        Individual = individualDay.Individual,
                        IndividualSamplingWeight = individualDay.Individual.SamplingWeight,
                        Day = individualDay.IdDay,
                        ConcentrationsBySubstance = concentrationsBySubstance
                            .ToDictionary(o => o.Key, o => (IHbmSubstanceTargetExposure)o.Value)
                    };
                    individualDayConcentrations[(individualDay.Individual, individualDay.IdDay)] = individualDayConcentration;
                }
            }
            return individualDayConcentrations;
        }
        private Dictionary<Compound, HbmSubstanceTargetExposure> computeConcentrationsBySubstance(
           ICollection<HumanMonitoringSampleSubstanceRecord> individualDaySamples,
           ICollection<Compound> substances,
           HumanMonitoringSamplingMethod samplingMethod,
           BiologicalMatrix targetBiologicalMatrix,
           ExpressionType expressionType,
           ConcentrationUnit concentrationUnit,
           TimeScaleUnit timeScaleUnit,
           TargetUnitsModel targetUnitsModel
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
                                new TargetUnit(
                                    concentrationUnit.GetSubstanceAmountUnit(),
                                    concentrationUnit.GetConcentrationMassUnit(),
                                    timeScaleUnit,
                                    samplingMethod.BiologicalMatrix,
                                    expressionType
                                )
                            );
                        return new HbmSubstanceTargetExposure() {
                            Substance = g.Key,
                            Concentration = concentration,
                            Unit = targetUnitsModel.GetUnit(g.Key, targetBiologicalMatrix),
                            SourceSamplingMethods = new List<HumanMonitoringSamplingMethod>() {
                                samplingMethod
                            },
                            BiologicalMatrix = targetBiologicalMatrix
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
