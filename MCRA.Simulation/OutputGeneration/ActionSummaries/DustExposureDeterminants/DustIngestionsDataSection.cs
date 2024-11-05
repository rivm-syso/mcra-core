using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DustIngestionsDataSection : SummarySection {
        public List<DustIngestionsDataRecord> Records { get; set; }

        public void Summarize(
            IList<DustIngestion> dustIngestions
        ) {
            Records = dustIngestions
                .Select(c => {
                    return new DustIngestionsDataRecord() {
                        idSubgroup = c.idSubgroup,
                        AgeLower = c.AgeLower.HasValue ? c.AgeLower.Value : null,
                        Sex = c.Sex != GenderType.Undefined ? c.Sex.ToString() : null,
                        Value = c.Value,
                        Unit = c.ExposureUnit.GetShortDisplayName(),
                        DistributionType = c.DistributionType != DustIngestionDistributionType.Constant ? c.DistributionType.ToString() : null,
                        CvVariability = c.CvVariability.HasValue ? c.CvVariability.Value : null,
                    };
                })
                .ToList();
        }
    }
}