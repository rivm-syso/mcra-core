using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.SubstanceConversionsCalculation {
    public sealed class SubstanceConversionSet {

        public double TranslationProportion { get; set; }

        public Dictionary<Compound, double> PositiveSubstanceConversions { get; set; }

        public bool IsAuthorised { get; set; }

    }
}
