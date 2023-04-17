
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.RelativePotencyFactors {
    public class RelativePotencyFactorsOutputData : IModuleOutputData {

        public Compound ReferenceSubstance { get; set; }
        public IDictionary<Compound, RelativePotencyFactor> RawRelativePotencyFactors { get; set; }
        public IDictionary<Compound, double> CorrectedRelativePotencyFactors { get; set; }

        private Compound _cumulativeSubstance;
        public Compound CumulativeCompound {
            get {
                if (_cumulativeSubstance == null) {
                    _cumulativeSubstance = new Compound() {
                        Code = "equivalents",
                        Name = $"_{ReferenceSubstance?.Name}Eq",
                    };
                }
                return _cumulativeSubstance;
            }
            set {
                _cumulativeSubstance = value;
            }
        }
        public IModuleOutputData Copy() {
            return new RelativePotencyFactorsOutputData() {
                RawRelativePotencyFactors = RawRelativePotencyFactors,
                CorrectedRelativePotencyFactors = CorrectedRelativePotencyFactors,
                ReferenceSubstance = ReferenceSubstance,
                CumulativeCompound= CumulativeCompound
            };
        }
    }
}

