using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.ExposureDeterminants;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.Generic.ExposureDeterminants {
    public class ExposureDistributionsDataSection : SummarySection {
        public List<BodyExposureFractionsDataRecord> Records { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }

        public void Summarize<T>(
            IList<T> bodyExposureFractions,
            ExposureSource source,
            string message
        ) where T : IExposureDistribution {
            Source = source.GetShortDisplayName();
            Message = message;
            Records = [.. bodyExposureFractions
            .Select(c => {
                return new BodyExposureFractionsDataRecord() {
                    idSubgroup = c.idSubgroup,
                    AgeLower = c.AgeLower.HasValue ? c.AgeLower.Value : null,
                    Sex = c.Sex != GenderType.Undefined ? c.Sex.ToString() : null,
                    Value = c.Value,
                    DistributionType = c.ExposureDistributionTypeString,
                    CvVariability = c.CvVariability.HasValue ? c.CvVariability.Value : null,
                };
            })];
        }
    }
}
