using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SoilIngestionsDataSection : SummarySection {
        public List<SoilIngestionsDataRecord> Records { get; set; }

        public void Summarize(
            IList<SoilIngestion> soilIngestions
        ) {
            Records = soilIngestions
                .Select(c => {
                    return new SoilIngestionsDataRecord() {
                        idSubgroup = c.idSubgroup,
                        AgeLower = c.AgeLower.HasValue ? c.AgeLower.Value : null,
                        Sex = c.Sex != GenderType.Undefined ? c.Sex.ToString() : null,
                        Value = c.Value,
                        Unit = c.ExposureUnit.GetShortDisplayName(),
                        DistributionType = c.DistributionType != SoilIngestionDistributionType.Constant ? c.DistributionType.ToString() : null,
                        CvVariability = c.CvVariability.HasValue ? c.CvVariability.Value : null,
                    };
                })
                .ToList();
        }
    }
}