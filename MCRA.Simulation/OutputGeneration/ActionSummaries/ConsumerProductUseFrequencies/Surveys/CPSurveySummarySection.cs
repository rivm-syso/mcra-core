using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CPSurveySummarySection : SummarySection {

        public CPSurveySummaryRecord Record { get; set; }

        public bool PopulationSubsetSelection { get; set; }

        public void Summarize(
            ConsumerProductSurvey survey,
            Population population
        ) {
            Record = new CPSurveySummaryRecord() {
                Code = survey.Code,
                Name = survey.Name,
                Description = survey.Description,
                StartDate = survey.StartDate?.ToString("MMM yyyy") ?? "-",
                EndDate = survey.EndDate?.ToString("MMM yyyy") ?? "-",
                NumberOfIndividuals = survey.Individuals.Count,
            };
        }
    }
}
