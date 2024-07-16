using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DustBodyExposureFractionsDataSection : SummarySection {
        public List<DustBodyExposureFractionsDataRecord> Records { get; set; }

        public void Summarize(
            IList<DustBodyExposureFraction> dustBodyExposureFractions
        ) {
            Records = dustBodyExposureFractions
                .Select(c => {
                    return new DustBodyExposureFractionsDataRecord() {
                        idSubgroup = c.idSubgroup,
                        AgeLower = c.AgeLower.HasValue ? c.AgeLower.Value : null,
                        Sex = c.Sex != GenderType.Undefined ? c.Sex.ToString() : null,
                        Value = c.Value,
                        DistributionType = c.DistributionType != ProbabilityDistribution.Deterministic ? c.DistributionType.ToString() : null,
                        CvVariability = c.CvVariability.HasValue ? c.CvVariability.Value : null,
                    };
                })
                .ToList();
        }
    }
}