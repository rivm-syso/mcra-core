using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.ConsumerProductExposureGenerators {

    public class ConsumerProductUnmatchedCorrelatedExposureGenerator : ConsumerProductExposureGenerator {
        private AgeAlignmentMethod _ageAlignmentMethod { get; set; }
        private List<(double left, double right)> _ageIntervals { get; set; }
        private bool _matchOnAge { get; set; }
        private bool _matchOnSex { get; set; }

        public ConsumerProductUnmatchedCorrelatedExposureGenerator(
            bool alignOnAge,
            bool alignOnSex,
            AgeAlignmentMethod ageAlignmentMethod,
            List<double> ageBins
        ) : base() {
            _ageAlignmentMethod = ageAlignmentMethod;
            _matchOnAge = alignOnAge;
            _matchOnSex = alignOnSex;
            if (_ageAlignmentMethod == AgeAlignmentMethod.AgeBins) {
                _ageIntervals = [.. ageBins
                    .Select((c, ix) => {
                        ix--;
                        return (
                            left: ix < 0 ? 0 : ageBins[ix],
                            right: ageBins[ix + 1]
                        );
                    })];
                _ageIntervals.Add((ageBins.Last(), double.MaxValue));
            }
        }

        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<ConsumerProductIndividualExposure> individualExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var exposurePaths = individualExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();

            var simulatedIndividual = individualDays.First().SimulatedIndividual;
            if (_matchOnSex) {
                //check cofactor match (only sex)
                individualExposures = getExposuresForSexLevel(
                    individualExposures,
                    simulatedIndividual
                );
            }
            if (_matchOnAge) {
                //check covariable match (only age)
                individualExposures = getExposuresForAgeRange(
                    individualExposures,
                    simulatedIndividual,
                    _ageIntervals
                );
            }

            var results = new List<IExternalIndividualDayExposure>();
            if (individualExposures?.Count > 0) {
                var ix = generator.Next(0, individualExposures.Count);
                var selected = individualExposures.ElementAt(ix);
                foreach (var individualDay in individualDays) {
                    var result = createExternalIndividualDayExposure(individualDay, selected);
                    results.Add(result);
                }
            } else {
                //It is not needed to add an empty exposure, so this code can be skipped
                foreach (var individualDay in individualDays) {
                    var result = createEmptyExternalIndividualDayExposure(individualDay, exposurePaths);
                    results.Add(result);
                }
            }
            return results;
        }

        /// <summary>
        /// Check for sex level and select the right individual day exposures, if not found return null.
        /// </summary>
        private static ICollection<ConsumerProductIndividualExposure> getExposuresForSexLevel(
            ICollection<ConsumerProductIndividualExposure> individualDayExposures,
            SimulatedIndividual simulatedIndividual
        ) {
            individualDayExposures = [.. individualDayExposures
                .Where(c => c.SimulatedIndividual.Gender == simulatedIndividual.Gender)];
            return individualDayExposures;
        }

        /// <summary>
        ///  Check for age range and select the right individual day exposures.
        /// </summary>
        private static ICollection<ConsumerProductIndividualExposure> getExposuresForAgeRange(
            ICollection<ConsumerProductIndividualExposure> individualDayExposures,
            SimulatedIndividual simulatedIndividual,
            List<(double left, double right)> intervals
        ) {
            if (individualDayExposures != null) {
                if (intervals != null) {
                    var (left, right) = intervals
                        .Single(c => simulatedIndividual.Age >= c.left && simulatedIndividual.Age < c.right);
                    individualDayExposures = [.. individualDayExposures.Where(c => c.SimulatedIndividual.Age >= left && c.SimulatedIndividual.Age < right)];
                } else {
                    var minimumDifference = individualDayExposures
                        .Select(c => Math.Abs(c.SimulatedIndividual.Age.Value - simulatedIndividual.Age.Value))
                        .Min();
                    individualDayExposures = [.. individualDayExposures.Where(c => Math.Abs(c.SimulatedIndividual.Age.Value - simulatedIndividual.Age.Value) == minimumDifference)];
                }
            }
            return individualDayExposures;
        }
    }
}
