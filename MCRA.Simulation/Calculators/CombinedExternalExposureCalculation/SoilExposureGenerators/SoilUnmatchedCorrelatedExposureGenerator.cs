using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.SoilExposureGenerators {

    public class SoilUnmatchedCorrelatedExposureGenerator : SoilExposureGenerator {

        /// <summary>
        /// Randomly pair soil and dietary individuals
        /// (if the properties of the dietary individual match the properties of the soil individual)
        /// </summary>
        private AgeAlignmentMethod _ageAlignmentMethod { get; set; }
        private List<(double left, double right)> _ageIntervals { get; set; }
        private bool _matchOnAge { get; set; }
        private bool _matchOnSex { get; set; }

        public SoilUnmatchedCorrelatedExposureGenerator(
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
            ICollection<SoilIndividualDayExposure> individualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var exposurePaths = individualDayExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();

            var simulatedIndividual = individualDays.First().SimulatedIndividual;
            if (_matchOnSex) {
                //check cofactor match (only sex)
                individualDayExposures = getExposuresForSexLevel(
                    individualDayExposures,
                    simulatedIndividual
                );
            }
            if (_matchOnAge) {
                //check covariable match (only age)
                individualDayExposures = getExposuresForAgeRange(
                    individualDayExposures,
                    simulatedIndividual,
                    _ageIntervals
                );
            }

            var results = new List<IExternalIndividualDayExposure>();
            if (individualDayExposures?.Count > 0) {
                var ix = generator.Next(0, individualDayExposures.Count);
                var selected = individualDayExposures.ElementAt(ix);
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
        private static ICollection<SoilIndividualDayExposure> getExposuresForSexLevel(
            ICollection<SoilIndividualDayExposure> individualDayExposures,
            SimulatedIndividual simulatedIndividual
        ) {
            individualDayExposures = [.. individualDayExposures.Where(c => c.SimulatedIndividual.Gender == simulatedIndividual.Gender)];
            return individualDayExposures;
        }

        /// <summary>
        ///  Check for age range and select the right individual day exposures.
        /// </summary>
        private static ICollection<SoilIndividualDayExposure> getExposuresForAgeRange(
            ICollection<SoilIndividualDayExposure> individualDayExposures,
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
