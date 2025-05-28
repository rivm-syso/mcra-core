using MCRA.Data.Compiled.Objects;

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
                        SubstanceCode = c.Substance.Code,
                        SubstanceName = c.Substance.Name,
                        Route = c.Route.ToString(),
                        Fraction = c.ExposureFraction,
                    };
                })];
        }
    }
}