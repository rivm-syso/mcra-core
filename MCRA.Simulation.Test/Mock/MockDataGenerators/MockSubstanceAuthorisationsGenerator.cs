using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock authorised uses
    /// </summary>
    public static class MockSubstanceAuthorisationsGenerator {

        /// <summary>
        /// Creates  authorised uses for foods and substances
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        public static IList<SubstanceAuthorisation> Create(ICollection<Food> foods, ICollection<Compound> substances) {
            var result = new List<SubstanceAuthorisation>();
            foreach (var food in foods) {
                foreach (var substance in substances) {
                    result.Add(new SubstanceAuthorisation() {
                        Food = food,
                        Substance = substance,
                        Reference = "Reference"
                    });
                }
            }
            return result;
        }

        public static Dictionary<(Food food, Compound substance), SubstanceAuthorisation> Create(
            params (Food, Compound)[] authorisedUses
        ) {
            var result = authorisedUses
                .Select(r => new SubstanceAuthorisation() {
                    Food = r.Item1,
                    Substance = r.Item2
                })
                .ToDictionary(r => (food: r.Food, substance: r.Substance));
            return result;
        }
    }
}
