using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.SubstanceConversionsCalculation {
    public sealed class SubstanceConversionCollection {

        public Compound MeasuredSubstance { get; set; }

        public Dictionary<Compound, double> LinkedActiveSubstances { get; set; }

        public ICollection<SubstanceConversionSet> SubstanceTranslationSets { get; set; }

    }
}
