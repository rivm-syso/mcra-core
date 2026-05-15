using MCRA.Data.Compiled.Interfaces;
using MCRA.General;

namespace MCRA.Simulation.Objects.SubstanceConcentrations {
    public class SubstanceConcentrationsCollection : List<ISubstanceConcentration> {
        public ConcentrationUnitSpec Unit { get; set; }

        public SubstanceConcentrationsCollection() { }

        public SubstanceConcentrationsCollection(IEnumerable<ISubstanceConcentration> concentrations, ConcentrationUnitSpec unit)
            : base(concentrations) {
            Unit = unit;
    }
}
}
