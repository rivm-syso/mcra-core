using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmSamplesSummarySection : SummarySection {

        public List<HbmSamplesSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<HumanMonitoringSample> samples) {
            Records = samples
                .GroupBy(r => r.SamplingMethod)
                .Select(r => new HbmSamplesSummaryRecord() {
                    BiologicalMatrix = r.Key.BiologicalMatrix.GetDisplayName(),
                    ExposureRoute = r.Key.ExposureRoute,
                    SamplingType = r.Key.SampleTypeCode,
                    NumberOfSamples = r.Count(),
                    NumberOfIndividualDaysWithSamples = r.GroupBy(s => (s.Individual, s.DayOfSurvey)).Count(),
                    NumberOfIndividualsWithSamples = r.GroupBy(s => s.Individual).Count(),
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
