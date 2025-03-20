using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AirVentilatoryFlowRatesDataSection : SummarySection {
        public List<AirVentilatoryFlowRatesDataRecord> Records { get; set; }

        public void Summarize(
            IList<AirVentilatoryFlowRate> airVentilatoryFlowRates
        ) {
            Records = airVentilatoryFlowRates
                .Select(c => {
                    return new AirVentilatoryFlowRatesDataRecord() {
                        idSubgroup = c.idSubgroup,
                        AgeLower = c.AgeLower.HasValue ? c.AgeLower.Value : null,
                        Sex = c.Sex != GenderType.Undefined ? c.Sex.ToString() : null,
                        Value = c.Value,
                        DistributionType = c.DistributionType != VentilatoryFlowRateDistributionType.Constant ? c.DistributionType.ToString() : null,
                        CvVariability = c.CvVariability.HasValue ? c.CvVariability.Value : null
                    };
                })
                .ToList();
        }
    }
}