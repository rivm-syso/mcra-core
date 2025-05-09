﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.AirExposureGenerators {

    public class AirUnmatchedCorrelatedExposureGenerator : AirExposureGenerator {

        private AgeAlignmentMethod _ageAlignmentMethod { get; set; }
        private List<(double left, double right)> _ageIntervals { get; set; }
        private bool _matchOnAge { get; set; }
        private bool _matchOnSex { get; set; }

        public AirUnmatchedCorrelatedExposureGenerator(
            bool alignOnAge,
            bool alignOnSex,
            AgeAlignmentMethod ageAlignmentMethod,
            List<double> ageBins
        ) : base() {
            _ageAlignmentMethod = ageAlignmentMethod;
            _matchOnAge = alignOnAge;
            _matchOnSex = alignOnSex;
            if (_ageAlignmentMethod == AgeAlignmentMethod.AgeBins) {
                _ageIntervals = ageBins
                    .Select((c, ix) => {
                        ix--;
                        return (
                            left: ix < 0 ? 0 : ageBins[ix],
                            right: ageBins[ix + 1]
                        );
                    })
                    .ToList();
                _ageIntervals.Add((ageBins.Last(), double.MaxValue));
            }
        }

        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var exposurePaths = airIndividualDayExposures
                .SelectMany(c => c.ExposuresPerPath.Keys)
                .ToHashSet();

            var simulatedIndividual = individualDays.First().SimulatedIndividual;
            if (_matchOnSex) {
                //check cofactor match (only sex)
                airIndividualDayExposures = getExposuresForSexLevel(
                    airIndividualDayExposures,
                    simulatedIndividual
                );
            }
            if (_matchOnAge) {
                //check covariable match (only age)
                airIndividualDayExposures = getExposuresForAgeRange(
                    airIndividualDayExposures,
                    simulatedIndividual,
                    _ageIntervals
                );
            }

            var results = new List<IExternalIndividualDayExposure>();
            if (airIndividualDayExposures?.Count > 0) {
                var ix = generator.Next(0, airIndividualDayExposures.Count);
                var selected = airIndividualDayExposures.ElementAt(ix);
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
        private static ICollection<AirIndividualDayExposure> getExposuresForSexLevel(
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            SimulatedIndividual simulatedIndividual
        ) {
            airIndividualDayExposures = airIndividualDayExposures
                .Where(c => c.SimulatedIndividual.Gender == simulatedIndividual.Gender)
                .ToList();
            return airIndividualDayExposures;
        }

        /// <summary>
        ///  Check for age range and select the right individual day exposures.
        /// </summary>
        private static ICollection<AirIndividualDayExposure> getExposuresForAgeRange(
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            SimulatedIndividual simulatedIndividual,
            List<(double left, double right)> intervals
        ) {
            if (airIndividualDayExposures != null) {
                if (intervals != null) {
                    var (left, right) = intervals
                        .Single(c => simulatedIndividual.Age >= c.left && simulatedIndividual.Age < c.right);
                    airIndividualDayExposures = airIndividualDayExposures
                        .Where(c => c.SimulatedIndividual.Age >= left && c.SimulatedIndividual.Age < right)
                        .ToList();
                } else {
                    var minimumDifference = airIndividualDayExposures
                        .Select(c => Math.Abs(c.SimulatedIndividual.Age.Value - simulatedIndividual.Age.Value))
                        .Min();
                    airIndividualDayExposures = airIndividualDayExposures
                        .Where(c => Math.Abs(c.SimulatedIndividual.Age.Value - simulatedIndividual.Age.Value) == minimumDifference)
                        .ToList();
                }
            }
            return airIndividualDayExposures;
        }
    }
}
