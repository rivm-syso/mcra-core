using MCRA.Data.Compiled.Interfaces;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Objects.SubstanceConcentrations {
    public class ConsumerProductSubstanceConcentrationsCollection : SubstanceConcentrationsCollection {
        public ConsumerProductSubstanceConcentrationsCollection() { }

        public ConsumerProductSubstanceConcentrationsCollection(IEnumerable<ISubstanceConcentration> concentrations, ConcentrationUnitSpec unit)
            : base(concentrations, unit) {
        }

        public ConsumerProduct Product { get; init; }
    }
}
