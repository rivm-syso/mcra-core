using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromPodCalculation {
    public class MembershipsFromPodCalculator {

        private readonly bool _restrictToAvailableHazardCharacterisations;
        private readonly bool _restrictToAvailableHazardDoses;
        private readonly bool _restrictToAvailableRpfs;

        public MembershipsFromPodCalculator(IMembershipsFromPodCalculatorSettings settings) {
            _restrictToAvailableHazardCharacterisations = settings.RestrictToAvailableHazardCharacterisations;
            _restrictToAvailableHazardDoses = settings.RestrictToAvailableHazardDoses;
            _restrictToAvailableRpfs = settings.RestrictToAvailableRpfs;
        }

        public ActiveSubstanceModel Compute(
            Effect effect,
            ICollection<Compound> substances,
            ICollection<PointOfDeparture> hazardDoses,
            IDictionary<Compound, RelativePotencyFactor> rpfs,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations
        ) {
            var substancesWithAvailablePod = new HashSet<Compound>();
            if (_restrictToAvailableHazardCharacterisations && hazardCharacterisations != null) {
                substancesWithAvailablePod.UnionWith(hazardCharacterisations.Keys.Distinct());
            }
            if (_restrictToAvailableHazardDoses && hazardDoses != null) {
                substancesWithAvailablePod.UnionWith(hazardDoses.Select(r => r.Compound).Distinct());
            }
            if (_restrictToAvailableRpfs && rpfs != null) {
                substancesWithAvailablePod.UnionWith(rpfs.Keys.Distinct());
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
