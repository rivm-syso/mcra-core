using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductExposureAssessmentSummarySection : SummarySection {

        public List<ConsumerProductExposureAssessmentRecord> Records { get; set; }

        public void Summarize(
            ICollection<ConsumerProductExposureAssessment> exposureAssessments
        ) {
            Records = [.. exposureAssessments.Select(c => new ConsumerProductExposureAssessmentRecord() {
                Name = c.Name,
                Code = c.IdAssessment,
                Description = c.Description,
                ExposureType = c.ExposureType.GetShortDisplayName(),
                NumberOfSets = c.ExposureSets.Count,
                Gender = c.Gender == GenderType.Undefined ? null : c.Gender.GetDisplayName(),
                AgeMin = c.AgeMin??double.NaN,
                AgeMax = c.AgeMax??double.NaN,
                Country = c.Location
            })];
        }
    }
}
