using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmSurveySummarySection : SummarySection {

        public HbmSurveySummaryRecord Record { get; set; }

        public void Summarize(
            HumanMonitoringSurvey hbmSurvey,
            Population population
        ) {
            Record = new HbmSurveySummaryRecord() {
                Code = hbmSurvey.Code,
                Name = hbmSurvey.Name,
                Description = hbmSurvey.Description,
                StartDate = hbmSurvey.StartDate?.ToString("MMM yyyy") ?? "-",
                EndDate = hbmSurvey.EndDate?.ToString("MMM yyyy") ?? "-",
                NumberOfSurveyDaysPerIndividual = hbmSurvey.NumberOfSurveyDays,
                NumberOfIndividuals = hbmSurvey.Individuals.Count,
                NumberOfIndividualDays = hbmSurvey.Individuals.Sum(i => i.NumberOfDaysInSurvey)
            };
        }
    }
}
