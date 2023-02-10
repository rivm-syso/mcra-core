
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.RelativePotencyFactors {
    public class RelativePotencyFactorsOutputData : IModuleOutputData {
        public IDictionary<Compound, RelativePotencyFactor> RawRelativePotencyFactors { get; set; }
        public IDictionary<Compound, double> CorrectedRelativePotencyFactors { get; set; }
        public IModuleOutputData Copy() {
            return new RelativePotencyFactorsOutputData() {
                RawRelativePotencyFactors = RawRelativePotencyFactors,
                CorrectedRelativePotencyFactors = CorrectedRelativePotencyFactors
            };
        }
    }
}

