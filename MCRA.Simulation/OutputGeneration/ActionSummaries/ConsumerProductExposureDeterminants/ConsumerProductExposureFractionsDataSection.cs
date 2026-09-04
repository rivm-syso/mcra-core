using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductExposureFractionsDataSection : SummarySection {
        public List<ConsumerProductExposureFractionRecord> Records { get; set; }

        public void Summarize(
            IList<ConsumerProductExposureFraction> consumerExposureFractions
        ) {
            Records = [.. consumerExposureFractions
                .Select(c => {
                    return new ConsumerProductExposureFractionRecord() {
                        ProductCode = c.Product.Code,
                        ProductName = c.Product.Name,
                        ParentName = c.Product.Parent?.Code,
                        SubstanceCode = c.Substance.Code,
                        SubstanceName = c.Substance.Name,
                        Route = c.Route.ToString(),
                        Fraction = c.ExposureFraction,
                        AgeLower = c.AgeLower,
                        Sex = c.Sex != GenderType.Undefined ? c.Sex.ToString() : null,
                    };
                })];
        }
    }
}