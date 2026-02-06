using MCRA.Data.Compiled.Interfaces;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Compiled.Objects {
    public sealed class SubstanceConcentration : ISubstanceConcentration {
        public string idSample { get; set; }
        public Compound Substance { get; set; }
        public double Concentration { get; set; }
        public ConcentrationUnit Unit { get; set; } = ConcentrationUnit.ugPerg;
        public string UnitTypeString => Unit.GetShortDisplayName();
    }
}
