using System.Collections.Generic;

namespace MCRA.Data.Compiled.Objects {
    public sealed class MolecularDockingModel {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Effect Effect { get; set; }
        public double? Threshold { get; set; }
        public int? NumberOfReceptors { get; set; }
        public string Reference { get; set; }

        public IDictionary<Compound, double> BindingEnergies { get; set; }
        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
