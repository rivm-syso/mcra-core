using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CompleteSamplesCalculation {

    /// <summary>
    /// Calculator for removing samples that are incomplete.
    /// </summary>
    public static class CompleteSamplesCalculator {
        /// <summary>
        /// Removes all samples of those individual-days for which one or more substances are not
        /// analysed by any of the sampling methods (matrices).
        /// </summary>
        public static ICollection<HumanMonitoringSample> FilterCompleteAnalysedSamples(
           ICollection<HumanMonitoringSample> humanMonitoringSamples,
           ICollection<HumanMonitoringSamplingMethod> samplingMethods,
           Dictionary<string, List<string>> excludedSubstanceMethods
        ) {
            var completeSamplesPerSamplingMethod = new Dictionary<HumanMonitoringSamplingMethod, List<HumanMonitoringSample>>();
            foreach (var method in samplingMethods) {
                excludedSubstanceMethods.TryGetValue(method.Code, out List<string> excludedSubstances);
                var allSubstances = humanMonitoringSamples
                    .Where(c => c.SamplingMethod == method)
                    .SelectMany(c => c.SampleAnalyses.SelectMany(am => am.AnalyticalMethod.AnalyticalMethodCompounds.Keys))
                    .Distinct()
                    .Where(s => !excludedSubstances?.Contains(s.Code) ?? true)
                    .ToList();

                var completeSamples = humanMonitoringSamples
                    .Where(c => c.SamplingMethod == method)
                    .SelectMany(c =>
                        c.SampleAnalyses.Select(am => am.AnalyticalMethod.AnalyticalMethodCompounds.Keys)
                        .Where(r => !allSubstances.Except(r).Any()),
                        (c, k) => c)
                    .ToList();

                completeSamplesPerSamplingMethod[method] = completeSamples;
            }

            // Take intersect on individual days for all sampling methods
            List<(int Individual, string DayOfSurvey)> completeIndividualsDays = null;
            foreach (var completeSamples in completeSamplesPerSamplingMethod.Values) {
                if (completeIndividualsDays == null) {
                    completeIndividualsDays = completeSamples.Select(c => (c.Individual.Id, c.DayOfSurvey)).ToList();
                } else {
                    var completeIndividualDaysForSamplingMethod = completeSamples.Select(c => (c.Individual.Id, c.DayOfSurvey)).ToList();
                    completeIndividualsDays = completeIndividualsDays.Intersect(completeIndividualDaysForSamplingMethod).ToList();
                }
            }

            foreach (var kv in completeSamplesPerSamplingMethod) {
                completeSamplesPerSamplingMethod[kv.Key] = kv.Value
                    .Where(s => completeIndividualsDays.Contains((s.Individual.Id, s.DayOfSurvey)))
                    .ToList();
            };

            return completeSamplesPerSamplingMethod.SelectMany(d => d.Value).ToList();
        }
    }
}
