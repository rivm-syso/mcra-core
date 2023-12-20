using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmSamplesSummarySection : SummarySection {

        public List<HbmSamplesSummaryRecord> Records { get; set; }

        public void Summarize(
            ICollection<HumanMonitoringSample> samples,
            Dictionary<(HumanMonitoringSamplingMethod method, Compound a), List<string>> nonAnalysedSamples) {
            var nonAnalysedPerMethod = nonAnalysedSamples
                .GroupBy(c => c.Key.method)
                .Select(g => new {
                    method = g.Key,
                    totalNonSampled = g.SelectMany(s => s.Value).Distinct().Count(),
                });
            Records = samples
                .GroupBy(r => r.SamplingMethod)
                .Select(r => new HbmSamplesSummaryRecord() {
                    BiologicalMatrix = r.Key.BiologicalMatrix.GetDisplayName(),
                    ExposureRoute = r.Key.ExposureRoute,
                    SamplingType = r.Key.SampleTypeCode,
                    NumberOfSamples = r.Count(),
                    NumberOfIndividualDaysWithSamples = r.GroupBy(s => (s.Individual, s.DayOfSurvey)).Count(),
                    NumberOfIndividualsWithSamples = r.GroupBy(s => s.Individual).Count(),
                    NumberOfSamplesNonAnalysed = nonAnalysedPerMethod.FirstOrDefault(g => g.method == r.Key)?.totalNonSampled ?? 0,
                    SamplingTimes = r
                        .Where(s => !string.IsNullOrEmpty(s.TimeOfSampling))
                        .Select(s => s.TimeOfSampling)
                        .Distinct()
                        .ToList()
                })
                .ToList();
        }
    }
}
