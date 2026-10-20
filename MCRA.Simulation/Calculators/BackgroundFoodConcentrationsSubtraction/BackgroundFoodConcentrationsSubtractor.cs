using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.BackgroundFoodConcentrationsSubtraction {
    public static class BackgroundFoodConcentrationsSubtractor {

        public static Dictionary<Food, SampleCompoundCollection> Subtract(
            Dictionary<Food, SampleCompoundCollection> sampleCompoundCollection,
            ICollection<BackgroundFoodConcentration> backgroundLevels,
            ConcentrationUnit concentrationUnit
        ) {
            // Compute adjusted collection
            var adjustedBackgroundLevels = backgroundLevels
                .Select(c => {
                    var aligmentFactor = c.ConcentrationUnit.GetConcentrationUnitMultiplier(concentrationUnit);
                    return (c.Food, c.Substance, Level: aligmentFactor * c.Value);
                })
                .ToDictionary(c => (c.Food, c.Substance), c => c.Level);

            var result = sampleCompoundCollection.Values.ToDictionary(r => r.Food, r => r.Clone());

            // Substract background from food substanceconcentrations
            foreach (var scc in result.Values) {
                foreach (var scr in scc.SampleCompoundRecords) {
                    foreach (var sc in scr.SampleCompounds.Values) {
                        if (adjustedBackgroundLevels.TryGetValue((scc.Food, sc.ActiveSubstance), out var backgroundLevel)) {
                            if (!double.IsNaN(sc.Residue)) {
                                sc.Residue = Math.Max(sc.Residue - backgroundLevel, 0);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
