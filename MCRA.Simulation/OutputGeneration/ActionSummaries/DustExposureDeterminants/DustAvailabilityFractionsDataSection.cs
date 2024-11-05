using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DustAvailabilityFractionsDataSection : SummarySection {
        public List<DustAvailabilityFractionsDataRecord> Records { get; set; }

        public void Summarize(
            IList<DustAvailabilityFraction> dustAvailabilityFractions
        ) {
            Records = dustAvailabilityFractions
                .Select(c => {
                    return new DustAvailabilityFractionsDataRecord() {
                        idSubgroup = c.idSubgroup,
                        SubstanceName = c.Substance.Name,
                        SubstanceCode = c.Substance.Code,
                        AgeLower = c.AgeLower.HasValue ? c.AgeLower.Value : null,
                        Sex = c.Sex != GenderType.Undefined ? c.Sex.ToString() : null,
                        Value = c.Value,
                        DistributionType = c.DistributionType != DustAvailabilityFractionDistributionType.Constant ? c.DistributionType.ToString() : null,
                        CvVariability = c.CvVariability.HasValue ? c.CvVariability.Value : null,
                    };
                })
                .ToList();
        }
    }
}
