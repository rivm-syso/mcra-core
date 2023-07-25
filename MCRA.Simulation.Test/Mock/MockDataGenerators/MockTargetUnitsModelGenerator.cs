using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Units;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock target units for substances.
    /// </summary>
    public static class MockTargetUnitsModelGenerator {

        /// <summary>
        /// Generates mcok dietary exposure models.
        /// </summary>
        public static TargetUnitsModel Create(
            ICollection<Compound> substances
        ) {
            var targetUnit = new TargetUnit(SubstanceAmountUnit.Micrograms, ConcentrationMassUnit.Liter, TimeScaleUnit.SteadyState, BiologicalMatrix.Blood, ExpressionType.Lipids);
            return new TargetUnitsModel {
                SubstanceTargetUnits = new Dictionary<TargetUnit, HashSet<Compound>> { { targetUnit, substances.ToHashSet() } }
            };
        }
    }
}
