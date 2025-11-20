using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock nondietary exposure sets
    /// </summary>
    public static class FakeNonDietaryExposureSetsGenerator {

        /// <summary>
        /// Creates a dictionary of non dietary exposure sets for each survey.
        /// </summary>
        public static Dictionary<NonDietarySurvey, List<NonDietaryExposureSet>> Create(
            ICollection<Individual> individuals,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IRandom random,
            ExternalExposureUnit exposureUnit = ExternalExposureUnit.mgPerKgBWPerDay,
            int numSurveys = 1,
            int numUncertaintySets = 0,
            bool matched = true,
            bool hasZeros = false
        ) {
            var surveys = new List<NonDietarySurvey>();
            for (int i = 0; i < numSurveys; i++) {
                var survey = new NonDietarySurvey() {
                    ExposureUnit = exposureUnit,
                    Code = $"NonDietarySurvey{i}",
                    Description = "Description",
                    Location = "Location",
                    Date = new DateTime(),
                    ProportionZeros = hasZeros ? random.NextDouble() * 100 : 0,
                };
                surveys.Add(survey);
            }
            var result = surveys
                .ToDictionary(
                    r => r,
                    r => generateSurveyExposureSets(
                        survey: r,
                        routes: routes,
                        individualCodes: [.. individuals.Select(r => matched ? r.Code : $"{r.Code}_{r.Code}")],
                        substances: substances,
                        uncertaintySets: numUncertaintySets,
                        random: random
                    )
            );
            return result;
        }

        /// <summary>
        /// Creates a dictionary of non dietary exposure sets for each survey.
        /// </summary>
        public static Dictionary<NonDietarySurvey, List<NonDietaryExposureSet>> CreateUnmatched(
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IRandom random,
            ExternalExposureUnit exposureUnit = ExternalExposureUnit.mgPerKgBWPerDay,
            int numSurveys = 1,
            int numUncertaintySets = 0,
            bool hasZeros = false,
            bool correlated = false,
            int numExposures = 10
        ) {
            var surveys = new List<NonDietarySurvey>();
            for (int i = 0; i < numSurveys; i++) {
                var proportionZeros = hasZeros 
                    ? (!correlated || i == 0 ? random.NextDouble() * 100 : surveys[0].ProportionZeros)
                    : 0;
                var survey = new NonDietarySurvey() {
                    ExposureUnit = exposureUnit,
                    Code = $"NonDietarySurvey{i}",
                    Description = "Description",
                    Location = "Location",
                    Date = new DateTime(),
                    ProportionZeros = proportionZeros,
                };
                surveys.Add(survey);
            }
            var result = surveys
                .ToDictionary(
                    r => r,
                    r => generateSurveyExposureSets(
                        survey: r,
                        routes: routes,
                        individualCodes: correlated
                            ? [.. Enumerable.Range(1, numExposures).Select(i => i.ToString())]
                            : [..Enumerable.Range(1, numExposures).Select(i => $"{r.Code}_{i}")],
                        substances: substances,
                        uncertaintySets: numUncertaintySets,
                        random: random
                    )
            );
            return result;
        }

        /// <summary>
        /// Creates a nondietary exposure set.
        /// </summary>
        private static List<NonDietaryExposureSet> generateSurveyExposureSets(
            NonDietarySurvey survey,
            ICollection<ExposureRoute> routes,
            ICollection<string> individualCodes,
            ICollection<Compound> substances,
            int uncertaintySets,
            IRandom random
        ) {
            var result = new List<NonDietaryExposureSet>();
            for (int i = 0; i < uncertaintySets + 1; i++) {
                var sets = individualCodes
                    .Select(r => {
                        return new NonDietaryExposureSet() {
                            NonDietarySurvey = survey,
                            IndividualCode = r,
                            Code = i == 0 ? string.Empty : i.ToString(),
                            NonDietaryExposures = substances
                                .Select(s => new NonDietaryExposure() {
                                    IdIndividual = r,
                                    Compound = s,
                                    Oral = routes.Contains(ExposureRoute.Oral) ? random.NextDouble() : 0D,
                                    Dermal = routes.Contains(ExposureRoute.Dermal) ? random.NextDouble() : 0D,
                                    Inhalation = routes.Contains(ExposureRoute.Inhalation) ? random.NextDouble() : 0D,
                                })
                            .ToList()
                        };
                    })
                    .ToList();
                result.AddRange(sets);
            }
            return result;
        }
    }
}
