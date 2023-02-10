
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.MarketShares {
    public class MarketSharesOutputData : IModuleOutputData {
        public ICollection<MarketShare> MarketShares { get; set; }
        public IModuleOutputData Copy() {
            return new MarketSharesOutputData() {
                MarketShares = MarketShares
            };
        }
    }
}

