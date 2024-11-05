using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DustAdherenceAmountsDataSection : SummarySection {
        public List<DustAdherenceAmountsDataRecord> Records { get; set; }

        public void Summarize(
            IList<DustAdherenceAmount> dustAdherenceAmounts
        ) {
            Records = dustAdherenceAmounts
                .Select(c => {
                    return new DustAdherenceAmountsDataRecord() {
                        idSubgroup = c.idSubgroup,
                        AgeLower = c.AgeLower.HasValue ? c.AgeLower.Value : null,
                        Sex = c.Sex != GenderType.Undefined ? c.Sex.ToString() : null,
                        Value = c.Value,
                        DistributionType = c.DistributionType != DustAdherenceAmountDistributionType.Constant ? c.DistributionType.ToString() : null,
                        CvVariability = c.CvVariability.HasValue ? c.CvVariability.Value : null,
                    };
                })
                .ToList();
        }
    }
}