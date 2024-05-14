using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCombinationCalculation {
    public static class UrineCombinationCalculator {

        /// <summary>
        /// Combines urine collections of different sampling types (spot, 24h, morning) into one collection.
        /// Note that standardised and non-standardised samples are treated as distinct collections,
        /// so here too, the outcome may also be standardised and non-standardised combined collections.
        /// </summary>
        public static List<HbmIndividualDayCollection> Combine(
           ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
           IEnumerable<SimulatedIndividualDay> simulatedIndividualDays
        ) {
            // Take over all non-urine collections as-is.
            var result = hbmIndividualDayCollections.Where(c => !c.Target.BiologicalMatrix.IsUrine()).ToList();

            // Combine urine collections per expression type
            var urineExpressionTypes = hbmIndividualDayCollections
                .Where(c => c.Target.BiologicalMatrix.IsUrine())
                .Select(c => c.Target.ExpressionType)
                .Distinct()
                .ToList();
            foreach (var expressionType in urineExpressionTypes) {
                var urineCollections = hbmIndividualDayCollections
                    .Where(c => c.Target.BiologicalMatrix.IsUrine() && c.Target.ExpressionType == expressionType);
                if (urineCollections.Any()) {
                    result.Add(CombinedUrineCollections(urineCollections, simulatedIndividualDays));
                }
            }
            return result;
        }

        private static HbmIndividualDayCollection CombinedUrineCollections(
           IEnumerable<HbmIndividualDayCollection> urineCollections,
           IEnumerable<SimulatedIndividualDay> simulatedIndividualDays
        ) {
            if (urineCollections.Count() == 1) {
                return urineCollections.First();
            }

            // We take the properties from the first collection as leading
            var combinedCollection = urineCollections.First().Clone();
            var individualDaySampleSubstances = GetIndividualDaySampleSubstanceRecords(simulatedIndividualDays, urineCollections);

            var combinedRecords = new List<HbmIndividualDayConcentration>();
            foreach (var individualDay in simulatedIndividualDays) {
                if (individualDaySampleSubstances.TryGetValue(individualDay, out List<HbmIndividualDayConcentration> records)) {
                    if (!records.Any()) {
                        continue;
                    }

                    var combinedRecord = records.First().Clone();
                    var combinedSampleSubstances = records
                        .SelectMany(d => d.ConcentrationsBySubstance.Values)
                        .GroupBy(kvp => kvp.Substance)
                        .ToDictionary(g => g.Key, g => getSampleSubstance(g.Key, g));
                    combinedRecord.ConcentrationsBySubstance = combinedSampleSubstances;

                    combinedRecords.Add(combinedRecord);
                }
            }
            combinedCollection.HbmIndividualDayConcentrations = combinedRecords;

            return combinedCollection;
        }

        private static HbmSubstanceTargetExposure getSampleSubstance(
            Compound substance,
            IEnumerable<HbmSubstanceTargetExposure> substanceTargetExposures
        ) {
            if (!substanceTargetExposures.Any()) {
                return null;
            }

            double combinedConcentration;
            var anyValue = substanceTargetExposures.Any(s => !double.IsNaN(s.Exposure));
            if (anyValue) {
                combinedConcentration = substanceTargetExposures.Where(s => !double.IsNaN(s.Exposure)).Average(r => r.Exposure);
            } else {
                combinedConcentration = double.NaN;
            }

            var substanceTargetExposure = new HbmSubstanceTargetExposure {
                Exposure = combinedConcentration,
                IsAggregateOfMultipleSamplingMethods = true,
                SourceSamplingMethods = substanceTargetExposures.SelectMany(s => s.SourceSamplingMethods).Distinct().ToList(),
                Substance = substance
            };

            return substanceTargetExposure;
        }

        private static Dictionary<SimulatedIndividualDay, List<HbmIndividualDayConcentration>> GetIndividualDaySampleSubstanceRecords(
            IEnumerable<SimulatedIndividualDay> simulatedIndividualDays,
            IEnumerable<HbmIndividualDayCollection> urineCollections
        ) {
            var individualDaySampleSubstanceRecords = simulatedIndividualDays
                .ToDictionary(
                    r => r,
                    r => new List<HbmIndividualDayConcentration>()
                );

            foreach (var urineCollection in urineCollections) {
                // Group by simulated individual day
                var hbmIndividualDayConcentrationsLookup = urineCollection.HbmIndividualDayConcentrations
                    .ToDictionary(r => (r.Individual, r.Day));

                foreach (var individualDay in simulatedIndividualDays) {
                    if (hbmIndividualDayConcentrationsLookup.TryGetValue((individualDay.Individual, individualDay.Day), out var hbmSampleSubstanceRecord)) {
                        individualDaySampleSubstanceRecords[individualDay].Add(hbmSampleSubstanceRecord);
                    }
                }
            }
            return individualDaySampleSubstanceRecords;
        }
    }
}
