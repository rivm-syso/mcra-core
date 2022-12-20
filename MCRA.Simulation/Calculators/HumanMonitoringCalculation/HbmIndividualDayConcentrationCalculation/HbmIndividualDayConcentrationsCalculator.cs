using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmIndividualDayConcentrationsCalculator {

        private readonly IHbmIndividualDayConcentrationsCalculatorSettings _settings;

        public HbmIndividualDayConcentrationsCalculator(
            IHbmIndividualDayConcentrationsCalculatorSettings settings
        ) {
            _settings = settings;
        }

        public List<HbmIndividualDayConcentration> Calculate(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            HumanMonitoringSamplingMethod samplingMethod
        ) {
            var result = new List<HbmIndividualDayConcentration>();
            var samplesPerIndividualDay = hbmSampleSubstanceCollections
                .FirstOrDefault(r => r.SamplingMethod == samplingMethod)
                .HumanMonitoringSampleSubstanceRecords?
                .GroupBy(r => (r.Individual, r.Day));
            if (samplesPerIndividualDay != null) {
                foreach (var groupedSample in samplesPerIndividualDay) {
                    var concentrationsBySubstance = computeConcentrationsBySubstance(
                        groupedSample.ToList(),
                        samplingMethod
                    );
                    var individualDayConcentration = new HbmIndividualDayConcentration() {
                        SimulatedIndividualId = groupedSample.Key.Individual.Id,
                        Individual = groupedSample.Key.Individual,
                        Day = groupedSample.Key.Day,
                        ConcentrationsBySubstance = concentrationsBySubstance
                    };
                    result.Add(individualDayConcentration);
                }
            }
            return result;
        }

        private Dictionary<Compound, HbmConcentrationByMatrixSubstance> computeConcentrationsBySubstance(
            ICollection<HumanMonitoringSampleSubstanceRecord> individualDaySamples,
            HumanMonitoringSamplingMethod samplingMethod
        ) {
            var result = individualDaySamples
                .SelectMany(sample => {
                    var specificGravityCorrectionFactor = computeConcentrationCorrectionFactor(sample);
                    var sampleIntakesBySubstance = sample.HumanMonitoringSampleSubstances.Values
                        .SelectMany(r => getConcentrationsBySubstance(r, specificGravityCorrectionFactor))
                        .GroupBy(r => r.Substance)
                        .Select(g => (
                            substance: g.Key,
                            concentration: g.Any() ? g.Average(r => r.Concentration) : double.NaN
                        )
                    );
                    return sampleIntakesBySubstance;
                })
                .GroupBy(r => r.substance)
                .ToDictionary(
                    g => g.Key,
                    g => new HbmConcentrationByMatrixSubstance() {
                        Substance = g.Key,
                        Concentration = g.Any() ? g.Average(r => r.concentration) : double.NaN,
                        SamplingMethod = samplingMethod,
                    }
                );
            return result;
        }

        private List<HbmConcentrationByMatrixSubstance> getConcentrationsBySubstance(
            SampleCompound sampleSubstance,
            double specificGravityCorrectionFactor
        ) {
            var result = new List<HbmConcentrationByMatrixSubstance>();

            if (sampleSubstance.IsPositiveResidue) {
                var exposure = new HbmConcentrationByMatrixSubstance() {
                    Substance = sampleSubstance.ActiveSubstance,
                    Concentration = sampleSubstance.Residue * specificGravityCorrectionFactor,
                };
                result.Add(exposure);
            } 
            return result;
        }

        private static double computeConcentrationCorrectionFactor(HumanMonitoringSampleSubstanceRecord sample) {
            // TODO: get sg correction from sample directly (may be computed; not from data record)
            return sample.SpecificGravityCorrectionFactor ?? 1;
        }
    }
}
