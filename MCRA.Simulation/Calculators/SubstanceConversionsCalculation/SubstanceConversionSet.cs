using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.SubstanceConversionsCalculation {
    public sealed class SubstanceConversionSet {

        public double TranslationProportion { get; set; }

        public Dictionary<Compound, double> PositiveSubstanceConversions { get; set; }

        public bool IsAuthorised { get; set; }

    }
}
