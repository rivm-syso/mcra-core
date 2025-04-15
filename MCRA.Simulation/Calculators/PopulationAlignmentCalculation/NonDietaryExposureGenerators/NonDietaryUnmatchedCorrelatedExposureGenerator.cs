using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.NonDietaryExposureGenerators {

    public class NonDietaryUnmatchedCorrelatedExposureGenerator : NonDietaryExposureGenerator {

        protected List<string> _nonDietaryIndividualCodes = [];

        public override void Initialize(
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposureSets,
            HashSet<ExposureRoute> routes,
            ExposureUnitTriple targetUnit
        ) {
            base.Initialize(
                nonDietaryExposureSets,
                routes,
                targetUnit
            );

            _nonDietaryIndividualCodes = nonDietaryExposureSets
                .SelectMany(ndeuis => ndeuis.Value.Select(r => r.IndividualCode))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Use the correlation between individuals in different nondietary surveys.
        /// Randomly pair non-dietary and dietary individuals
        /// (if the properties of the individual match the covariates of the non-dietary survey)
        /// </summary>
        /// <param name="individual"></param>
        /// <param name="nonDietarySurvey"></param>
        /// <param name="substances"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        protected override List<NonDietaryIntakePerCompound> createNonDietaryIndividualExposure(
            SimulatedIndividual individual,
            NonDietarySurvey nonDietarySurvey,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var nonDietaryExposures = new List<NonDietaryIntakePerCompound>();
            if (_nonDietaryExposureSetsDictionary.TryGetValue(nonDietarySurvey, out var exposureSets)) {
                if (checkIndividualMatchesNonDietarySurvey(individual, nonDietarySurvey) && nonDietarySurvey.ProportionZeros < 100) {
                    generator.Reset();
                    if (generator.NextDouble() >= nonDietarySurvey.ProportionZeros / 100) {
                        var randomIndividualCode = _nonDietaryIndividualCodes.ElementAt(generator.Next(0, _nonDietaryIndividualCodes.Count));
                        if (exposureSets.TryGetValue(randomIndividualCode, out var exposureSet) && exposureSet != null) {
                            nonDietaryExposures.AddRange(nonDietaryIntakePerCompound(exposureSet, nonDietarySurvey, individual, substances));
                        }
                    }
                }
            }
            return nonDietaryExposures;
        }
    }
}
