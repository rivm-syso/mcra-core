using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmIndividualConcentrationsCalculator {

        public List<HbmIndividualCollection> Calculate(
            ICollection<Compound> substances,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayConcentrationsCollections
        ) {
            var results = new List<HbmIndividualCollection>();
            foreach (var collection in hbmIndividualDayConcentrationsCollections) {
                var result = collection.HbmIndividualDayConcentrations
                    .GroupBy(r => r.SimulatedIndividualId)
                    .Select(r => {
                        var record = new HbmIndividualConcentration() {
                            Individual = r.First().Individual,
                            SimulatedIndividualId = r.First().SimulatedIndividualId,
                            IndividualSamplingWeight = r.First().IndividualSamplingWeight,
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

                            if (substanceIndividualDayConcentrations.Any()) {
                                var originalSamplingMethods = substanceIndividualDayConcentrations
                                    .SelectMany(r => r.SourceSamplingMethods)
                                    .Distinct()
                                    .ToList();
                                var item = new HbmSubstanceTargetExposure() {
                                    Concentration = meanConcentration,
                                    Substance = substance,
                                    SourceSamplingMethods = originalSamplingMethods
                                };
                                concentrationsBySubstance.Add(substance, item);
                            }
                        }

                        record.ConcentrationsBySubstance = concentrationsBySubstance
                            .ToDictionary(o => o.Key, o => o.Value);
                        return record;
                    })
                    .ToList();
                var hbmCollection = new HbmIndividualCollection() {
                    TargetUnit = collection.TargetUnit,
                    HbmIndividualConcentrations = result 
                };
                results.Add(hbmCollection);
            }
            return results;
        }
    }
}
