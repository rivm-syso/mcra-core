using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock hazard doses
    /// </summary>
    public static class FakePointsOfDepartureGenerator {
        /// <summary>
        /// Creates a dictionary of Points Of Departure for each substance
        /// </summary>
        public static IDictionary<Compound, Data.Compiled.Objects.PointOfDeparture> Create(
             IEnumerable<Compound> substances,
             PointOfDepartureType pointsOfDepartureType,
             Effect effect,
             string species,
             IRandom random,
             ExposureTarget exposureTarget = null,
             TargetLevelType targetLevelType = TargetLevelType.External,
             DoseUnit doseUnit = DoseUnit.mgPerKgBWPerDay,
             bool uncertaintySets = false
        ) {

            var pointOfDepartureUncertaintySets = new List<PointOfDepartureUncertain>();
            if (uncertaintySets) {
                pointOfDepartureUncertaintySets = [];
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
                PointOfDepartureType = pointsOfDepartureType,
                Effect = effect,
                Species = species,
                LimitDose = random.NextDouble() * 10000,
                CriticalEffectSize = 0.05,
                PointOfDepartureUncertains = pointOfDepartureUncertaintySets,
                BiologicalMatrix = exposureTarget != null ? exposureTarget.BiologicalMatrix : BiologicalMatrix.Undefined,
                ExpressionType = exposureTarget != null ? exposureTarget.ExpressionType : ExpressionType.None,
                ExposureRoute = exposureTarget != null ? exposureTarget.ExposureRoute : ExposureRoute.Oral,
                TargetLevel = targetLevelType,
                DoseUnit = doseUnit
            });
            return result;
        }
    }
}
