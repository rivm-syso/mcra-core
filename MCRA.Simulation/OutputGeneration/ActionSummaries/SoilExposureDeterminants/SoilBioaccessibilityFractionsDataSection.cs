using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SoilBioaccessibilityFractionsDataSection : SummarySection {
        public List<BioaccessibilityFractionsDataRecord> Records { get; set; }

        public void Summarize(
            IList<SoilBioaccessibilityFraction> soilBioaccessibilityFractions   
        ) {
            Records = [.. soilBioaccessibilityFractions
                .Select(c => {
                    return new BioaccessibilityFractionsDataRecord() {
                        SubstanceName = c.Substance.Name,
                        SubstanceCode = c.Substance.Code,
                        Value = c.Value,
                        DistributionType = c.DistributionType != BioaccessibilityFractionDistributionType.Constant ? c.DistributionType.ToString() : null,
                        CvVariability = c.CvVariability.HasValue ? c.CvVariability.Value : null,
                    };
                })];
        }
    }
}
