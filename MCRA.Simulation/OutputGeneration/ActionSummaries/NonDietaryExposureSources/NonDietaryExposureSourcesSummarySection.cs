using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryExposureSourcesSummarySection : SummarySection {

        public List<NonDietaryExposureSourceSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<NonDietaryExposureSource> sources) {
            Records = sources
                .Select(c => {
                    return new NonDietaryExposureSourceSummaryRecord() {
                        Code = c.Code,
                        Name = c.Name,
                    };
                })
                .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
