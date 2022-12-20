using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmSamplesSummarySection : SummarySection {

        public List<HbmSamplesSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<HumanMonitoringSample> samples) {
            Records = samples
                .GroupBy(r => r.SamplingMethod)
                .Select(r => new HbmSamplesSummaryRecord() {
                    Compartment = r.Key.Compartment,
                    ExposureRoute = r.Key.ExposureRoute,
                    SamplingType = r.Key.SampleType,
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
