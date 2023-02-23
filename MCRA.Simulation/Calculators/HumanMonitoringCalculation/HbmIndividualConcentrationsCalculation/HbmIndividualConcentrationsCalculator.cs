using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmIndividualConcentrationsCalculator {

        public List<HbmIndividualConcentration> Calculate(
            ICollection<Compound> substances,
            ICollection<HbmIndividualDayConcentration> hbmIndividualDayConcentrations
        ) {
            var result = hbmIndividualDayConcentrations
                .GroupBy(r => r.SimulatedIndividualId)
                .Select(r => {
                    var record = new HbmIndividualConcentration() {
                        Individual = r.First().Individual,
                        SimulatedIndividualId = r.First().SimulatedIndividualId,
                        NumberOfDays = r.Count(), // TODO: check? Count / number of days in survey?
                    };

                    var concentrationsBySubstance = new Dictionary<Compound, HbmSubstanceTargetExposure>();
                    foreach (var substance in substances) {
                        var concentrations = r
                            .Select(c => c.AverageEndpointSubstanceExposure(substance))
                            .ToList();
                        var meanConcentration = concentrations.Any() ? concentrations.Average() : 0;
                        var substanceIndividualDayConcentrations = r
                            .Select(c => c.ConcentrationsBySubstance.TryGetValue(substance, out var conc) ? conc : null)
                            .Where(r => r != null)
                            .ToList();
                        var originalSamplingMethods = substanceIndividualDayConcentrations
                            .SelectMany(r => ((IHbmSubstanceTargetExposure)r).SourceSamplingMethods)
                            .Distinct()
                            .ToList();

                        var item = new HbmSubstanceTargetExposure() {
                            Concentration = meanConcentration,
                            Substance = substance,
                            BiologicalMatrix = substanceIndividualDayConcentrations.Any() ? ((IHbmSubstanceTargetExposure)substanceIndividualDayConcentrations.First()).BiologicalMatrix : string.Empty,
                            SourceSamplingMethods = originalSamplingMethods
                        };

                        concentrationsBySubstance.Add(substance, item);
                    }

                    record.ConcentrationsBySubstance = concentrationsBySubstance.ToDictionary(o => o.Key, o => (IHbmSubstanceTargetExposure)o.Value);
                    return record;
                })
                .ToList();

            return result;
        }
    }
}
