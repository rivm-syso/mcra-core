using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DoseResponseDataSection : SummarySection {
        public List<DoseResponseExperimentSection> Records { get; set; }

        public void Summarize(ICollection<DoseResponseExperiment> experiments, ICollection<Response> selectedResponses, SectionHeader subHeader, int subOrder) {
            var doseResponseExperimentSections = new List<DoseResponseExperimentSection>();
            foreach (var experiment in experiments) {
                var responses = experiment.Responses.Where(r => selectedResponses.Contains(r));
                foreach (var response in responses) {
                    var record = new DoseResponseExperimentSection();
                    record.Summarize(experiment, response);
                    doseResponseExperimentSections.Add(record);
                }
            }
            Records = doseResponseExperimentSections
                .OrderBy(r => r.ResponseCode, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.ExperimentCode, System.StringComparer.OrdinalIgnoreCase)
                .ToList();
            foreach (var record in Records) {
                var subSubHeader = subHeader.AddSubSectionHeaderFor(record, $"{record.ResponseCode} ({record.ExperimentCode})", subOrder++);
                subSubHeader.SaveSummarySection(record);
            }
        }
    }
}
