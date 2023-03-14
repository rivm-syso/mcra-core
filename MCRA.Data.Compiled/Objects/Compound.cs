using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class Compound : IStrongEntity {

        private string _name;

        public Compound() {
        }

        public Compound(string code) {
            Code = code;
        }

        public string Code { get; set; }

        public string Name {
            get {
                if (!string.IsNullOrEmpty(_name)) {
                    return _name;
                }
                return Code;
            }
            set {
                _name = value;
            }
        }

        public string ConcentrationUnitString { get; set; }
        public string Description { get; set; }
        public int? CramerClass { get; set; }
        public double MolecularMass { get; set; }
        public bool? IsLipidSoluble { get; set; }

        public ConcentrationUnit ConcentrationUnit {
            get {
                return ConcentrationUnitConverter.FromString(this.ConcentrationUnitString, ConcentrationUnit.mgPerKg);
            }
        }

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
