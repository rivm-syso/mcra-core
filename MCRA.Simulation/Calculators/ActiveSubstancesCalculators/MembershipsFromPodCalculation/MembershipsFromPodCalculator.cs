using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromPodCalculation {
    public class MembershipsFromPodCalculator {

        private readonly bool _restrictToAvailableHazardCharacterisations;
        private readonly bool _restrictToAvailableHazardDoses;

        public MembershipsFromPodCalculator(
            bool restrictToAvailableHazardCharacterisations,
            bool restrictToAvailableHazardDoses
        ) {
            _restrictToAvailableHazardCharacterisations = restrictToAvailableHazardCharacterisations;
            _restrictToAvailableHazardDoses = restrictToAvailableHazardDoses;
        }

        public ActiveSubstanceModel Compute(
            Effect effect,
            ICollection<Compound> substances,
            ICollection<PointOfDeparture> hazardDoses,
            ICollection<HazardCharacterisationModelCompoundsCollection> hazardCharacterisationModelsCollections
        ) {
            var hazardCharacterisationSubstances = hazardCharacterisationModelsCollections?
                .SelectMany(c => c.HazardCharacterisationModels.Select(r => r.Key))
                .Distinct()
                .ToHashSet();

            var substancesWithAvailablePod = new HashSet<Compound>();
            if (_restrictToAvailableHazardCharacterisations && hazardCharacterisationSubstances != null) {
                substancesWithAvailablePod.UnionWith(hazardCharacterisationSubstances);
            }
            if (_restrictToAvailableHazardDoses && hazardDoses != null) {
                substancesWithAvailablePod.UnionWith(hazardDoses.Select(r => r.Compound).Distinct());
            }

            var result = new ActiveSubstanceModel() {
                Code = $"PoD presence",
                Name = "Memberships from available hazard data",
                Description = "Memberships derived from available hazard data",
                Effect = effect,
                MembershipProbabilities = substances.ToDictionary(r => r, r => substancesWithAvailablePod.Contains(r) ? 1D : 0D)
            };
            return result;
        }
    }
}
