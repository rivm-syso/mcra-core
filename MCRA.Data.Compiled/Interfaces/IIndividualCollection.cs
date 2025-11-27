using MCRA.Data.Compiled.Objects;

namespace MCRA.Data.Compiled.Interfaces {
    public interface IIndividualCollection {
        ICollection<Individual> Individuals { get; }
    }
}
