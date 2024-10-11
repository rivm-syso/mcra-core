using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class Compound : StrongEntity {

        public Compound() {
        }

        public Compound(string code) {
            Code = code;
        }

        public int? CramerClass { get; set; }
        public double MolecularMass { get; set; }
        public bool IsLipidSoluble { get; set; }

        public ConcentrationUnit ConcentrationUnit { get; set; } = ConcentrationUnit.mgPerKg;
    }
}
