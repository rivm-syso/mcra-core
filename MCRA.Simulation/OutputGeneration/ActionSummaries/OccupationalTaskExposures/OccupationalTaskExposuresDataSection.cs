using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OccupationalTaskExposuresDataSection : SummarySection {
        public List<OccupationalTaskExposuresDataRecord> Records { get; set; }

        public void Summarize(
            ICollection<OccupationalTaskExposure> OccupationalTaskExposures
        ) {
            Records = [.. OccupationalTaskExposures
                .Select(c => {
                    return new OccupationalTaskExposuresDataRecord() {
                        TaskCode = c.OccupationalTask.Code,
                        TaskName = c.OccupationalTask.Name,
                        RpeType = c.RpeType != RPEType.Undefined ? c.RpeType.ToString() : null,
                        ExposureRoute = c.ExposureRoute.ToString(),
                        Unit = c.Unit.GetShortDisplayName(),
                        EstimateType = c.EstimateType != JobTaskExposureEstimateType.Undefined
                            ? c.EstimateType.GetDisplayName() : null,
                        CodeSubstance = c.Substance.Code.ToString(),
                        NameSubstance = c.Substance.Name,
                        Percentage = c.Percentage,
                        Value = c.Value,
                        Reference = c.Reference
                    };
                })];
        }
    }
}