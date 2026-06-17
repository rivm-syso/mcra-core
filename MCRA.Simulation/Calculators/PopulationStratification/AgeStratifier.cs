using MCRA.General;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.Stratification {

    public class AgeStratificationLevel : IStratificationLevel {
        public AgeInterval Interval { get; private set; }
        public string Code => Interval.GetCode();
        public string Name => Interval.GetName();
        public AgeStratificationLevel(AgeInterval interval) {
            Interval = interval;
        }
    }

    /// <summary>
    /// Stratifier for stratification based on the age of the individuals.
    /// </summary>
    public class AgeStratifier : ISimplePopulationStratifier {

        private readonly Dictionary<AgeInterval, IStratificationLevel> _stratificationLevels;
        private static List<AgeInterval> _intervals;

        public OutputStratificationVariable StratificationVariable => OutputStratificationVariable.Age;

        public AgeStratifier(double[] ageGroupBoundaries) {
            _stratificationLevels = CreateLevels(ageGroupBoundaries);
        }

        /// <summary>
        /// Gets the stratification level of the individual.
        /// </summary>
        public IStratificationLevel GetLevel(SimulatedIndividual individual) {
            if (individual.Age.HasValue) {
                var age = individual.Age.Value;
                var interval = _intervals.Where(i => age >= i.Lower && (double.IsNaN(i.Upper) || age < i.Upper)).Single();
                return _stratificationLevels.TryGetValue(interval, out var val) ? val : null;
            }
            return null;
        }

        /// <summary>
        /// Creates a stratification level lookup dictionary for the age values.
        /// </summary>
        public static Dictionary<AgeInterval, IStratificationLevel> CreateLevels(
            double[] ageGroupBoundaries
        ) {
            var boundaries = ageGroupBoundaries.OrderBy(x => x).ToList();
            _intervals = getIntervals(boundaries);
            var result = _intervals
                .Select(r => new AgeStratificationLevel(r))
                .ToDictionary(
                    r => r.Interval,
                    r => r as IStratificationLevel
                );
            return result;
        }

        private static List<AgeInterval> getIntervals(List<double> boundaries) {
            var intervals = new List<AgeInterval>();
            var lower = double.NaN;
            for (int i = 0; i < boundaries.Count; i++) {
                var upper = boundaries[i];
                var ageInterval = double.IsNaN(lower) ? new AgeInterval(0, upper) : new AgeInterval(lower, upper);
                intervals.Add(ageInterval);
                lower = upper;
            }
            intervals.Add(new AgeInterval(lower, double.NaN));
            return intervals;
        }
    }

    public class AgeInterval(double lower, double upper) {
        public double Lower { get; } = lower;
        public double Upper { get; } = upper;

        public string GetName() {
            return Lower == 0
                ? $"<= {Upper:G3}"
                : double.IsNaN(Upper)
                    ? $"> {Lower:G3}"
                    : $"{Lower:G3} - {Upper:G3}";
        }

        public string GetCode() {
            return Lower == 0
                ? $"le{Upper:G3}"
                : double.IsNaN(Upper)
                    ? $"gt{Lower:G3}"
                    : $"{Lower:G3}-{Upper:G3}";
        }
    }
}
