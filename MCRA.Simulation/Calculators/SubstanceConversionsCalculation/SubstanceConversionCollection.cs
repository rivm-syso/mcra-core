using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.SubstanceConversionsCalculation {
    public sealed class SubstanceConversionCollection {

        public Compound MeasuredSubstance { get; set; }

        public Dictionary<Compound, double> LinkedActiveSubstances { get; set; }

        public ICollection<SubstanceConversionSet> SubstanceTranslationSets { get; set; }

    }
}
