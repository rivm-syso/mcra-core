using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CPSurveySummarySection : SummarySection {

        public List<CPSurveySummaryRecord> Records { get; set; }

        public bool PopulationSubsetSelection { get; set; }

        public void Summarize(
            ICollection<ConsumerProductSurvey> surveys,
            Population population
        ) {
            Records = [.. surveys.Select(survey => new CPSurveySummaryRecord() {
                Code = survey.Code,
                Name = survey.Name,
                Description = survey.Description,
                StartDate = survey.StartDate?.ToString("MMM yyyy"),
                EndDate = survey.EndDate?.ToString("MMM yyyy"),
                NumberOfIndividuals = survey.Individuals.Count,
            })];
        }
    }
}
