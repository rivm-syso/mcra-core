using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock hazard doses
    /// </summary>
    public static class MockPointsOfDepartureGenerator {
        /// <summary>
        /// Creates a dictionary of Points Of Departure for each substance
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="pointsOfDepartureType"></param>
        /// <param name="effect"></param>
        /// <param name="species"></param>
        /// <param name="random"></param>
        /// <param name="uncertaintySets"></param>
        /// <returns></returns>
        public static IDictionary<Compound, Data.Compiled.Objects.PointOfDeparture> Create(
             IEnumerable<Compound> substances,
             PointOfDepartureType pointsOfDepartureType,
             Effect effect,
             string species,
             IRandom random,
             bool uncertaintySets = false
        ) {

            var pointOfDepartureUncertaintySets = new List<PointOfDepartureUncertain>();
            if (uncertaintySets) {
                pointOfDepartureUncertaintySets = new List<PointOfDepartureUncertain>();
                for (int i = 0; i < 5; i++) {
                    var hazardDoseUncertaintySet = substances.Select(c => new PointOfDepartureUncertain() {
                        Compound = c,
                        Effect = effect,
                        LimitDose = random.NextDouble() * 10000,
                        IdUncertaintySet = $"{i}_Set",
                    }).ToList();
                    pointOfDepartureUncertaintySets.AddRange(hazardDoseUncertaintySet);
                }
            }

            var result = substances.ToDictionary(s => s, s => new Data.Compiled.Objects.PointOfDeparture() {
                Code = "code",
                Compound = s,
                PointOfDepartureTypeString = pointsOfDepartureType.ToString(),
                Effect = effect,
                Species = species,
                LimitDose = random.NextDouble() * 10000,
                CriticalEffectSize = 0.05,
                PointOfDepartureUncertains = pointOfDepartureUncertaintySets
            });
            return result;
        }
    }
}
