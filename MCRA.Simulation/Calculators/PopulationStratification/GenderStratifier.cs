using MCRA.General;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.Stratification {

    public class GenderStratificationLevel : IStratificationLevel {
        public GenderType Gender { get; private set; }
        public string Code => Gender.ToString();
        public string Name => Gender.GetDisplayName();
        public GenderStratificationLevel(GenderType gender) {
            Gender = gender == 0 ? GenderType.Undefined : gender;
        }
    }

    /// <summary>
    /// Stratifier for stratification based on the gender/sex of the individuals.
    /// </summary>
    public class GenderStratifier : PopulationStratifier {

        private readonly Dictionary<GenderType, IStratificationLevel> _stratificationLevels;

        public GenderStratifier() {
            _stratificationLevels = CreateLevels();
        }

        /// <summary>
        /// Gets the stratification level of the individual.
        /// </summary>
        public override IStratificationLevel GetLevel(SimulatedIndividual individual) {
            var gender = individual.Gender != 0 ? individual.Gender : GenderType.Undefined;
            return _stratificationLevels.TryGetValue(gender, out var val) ? val : null;
        }

        /// <summary>
        /// Creates a stratification level lookup dictionary for the gender values.
        /// </summary>
        public static Dictionary<GenderType, IStratificationLevel> CreateLevels() {
            var result = Enum.GetValues<GenderType>()
                .Where(r => r != GenderType.Undefined)
                .Select(r => new GenderStratificationLevel(r))
                .ToDictionary(
                    r => r.Gender,
                    r => r as IStratificationLevel
                );
            return result;
        }
    }
}
