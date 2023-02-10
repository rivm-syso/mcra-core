
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Substances {
    public class SubstancesOutputData : IModuleOutputData {
        public Compound ReferenceCompound { get; set; }
        public ICollection<Compound> AllCompounds { get; set; }
        private Compound _cumulativeCompound;
        public Compound CumulativeCompound {
            get {
                if (_cumulativeCompound == null) {
                    _cumulativeCompound = new Compound() {
                        Code = "equivalents",
                        Name = $"_{ReferenceCompound?.Name}Eq",
                    };
                }
                return _cumulativeCompound;
            }
            set {
                _cumulativeCompound = value;
            }
        }
        public IModuleOutputData Copy() {
            return new SubstancesOutputData() {
                ReferenceCompound = ReferenceCompound,
                AllCompounds = AllCompounds,
                CumulativeCompound = CumulativeCompound
            };
        }
    }
}

