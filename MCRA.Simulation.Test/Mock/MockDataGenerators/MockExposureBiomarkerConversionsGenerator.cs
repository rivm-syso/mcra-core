using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock substance approvals data
    /// </summary>
    public static class MockExposureBiomarkerConversionsGenerator {

        /// <summary>
        /// Creates fake conversions.
        /// </summary>
        public static IList<ExposureBiomarkerConversion> Create(ICollection<Compound> substances) {
            if (substances.Count == 1) { return null; }

            var n = substances.Count()/2;
            var substancesFrom = substances.Take(n).ToList();
            var substancesTo = substances.Skip(n).ToList();
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var result = substancesFrom.Select((s,ix) => {
                return new ExposureBiomarkerConversion {
                    SubstanceFrom = s,
                    SubstanceTo = substancesTo[ix],
                    BiologicalMatrix = BiologicalMatrix.Blood,
                    UnitFrom = new ExposureUnitTriple(SubstanceAmountUnit.Micrograms, ConcentrationMassUnit.Liter),
                    UnitTo = new ExposureUnitTriple(SubstanceAmountUnit.Micrograms, ConcentrationMassUnit.Liter),
                    ExpressionTypeFrom = ExpressionType.None, 
                    ExpressionTypeTo = ExpressionType.None,   
                    ConversionFactor = random.NextDouble(),
                };
            });
            return result.ToList();
        }
    }
}
