using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.Generic {
    public sealed class SimpleSubstanceConcentration {
        public Compound Substance { get; set; }
        public double Concentration { get; set; }
        public string UnitString { get; set; }

        public static SimpleSubstanceConcentration Clone(SubstanceConcentration concentration) {
            return new SimpleSubstanceConcentration {
                Substance = concentration.Substance,
                Concentration = concentration.Concentration,
                UnitString = concentration.Unit.GetShortDisplayName()
            };
        }
        public static SimpleSubstanceConcentration Clone(AirConcentration concentration) {
            return new SimpleSubstanceConcentration {
                Substance = concentration.Substance,
                Concentration = concentration.Concentration,
                UnitString = concentration.Unit.GetShortDisplayName()
            };
        }
    }
}
