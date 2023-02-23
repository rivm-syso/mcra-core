using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.ResidueGeneration {
    public interface IResidueGenerator {
        List<CompoundConcentration> GenerateResidues(
            Food food,
            ICollection<Compound> substances,
            IRandom random
        );
    }
}
