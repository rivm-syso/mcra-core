using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock nondietary exposure sets
    /// </summary>
    public static class MockNonDietaryExposureSetsGenerator {

        /// <summary>
        /// Creates a dictionary of non dietary exposure sets for each survey
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="substances"></param>
        /// <param name="nonDietaryExposureRoutes"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="number"></param>
        /// <param name="random"></param>
        /// <param name="isMatched"></param>
        /// <param name="hasZeros"></param>
        /// <returns></returns>
        public static Dictionary<NonDietarySurvey, List<NonDietaryExposureSet>> MockNonDietarySurveys(
            ICollection<Individual> individuals,
            ICollection<Compound> substances,
            ICollection<ExposurePathType> nonDietaryExposureRoutes,
            IRandom random,
            ExternalExposureUnit exposureUnit = ExternalExposureUnit.mgPerKgBWPerDay,
            int number = 1,
            bool isMatched = false,
            bool hasZeros = false
        ) {
            var nonDietaryExposureSets = MockNonDietaryExposureSets(
                individuals: individuals,
                substances: substances,
                nonDietaryExposureRoutes: nonDietaryExposureRoutes,
                random: random,
                exposureUnit: exposureUnit,
                uncertaintySets: false,
                number: number,
                isMatched: isMatched,
                hasZeros: hasZeros
            );
            var nonDietarySurveys = nonDietaryExposureSets
                .GroupBy(r => r.NonDietarySurvey)
                .ToDictionary(r => r.Key, r => r.ToList());
            return nonDietarySurveys;
        }

        /// <summary>
        /// Creates a nondietary exposure set
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="substances"></param>
        /// <param name="nonDietaryExposureRoutes"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="random"></param>
        /// <param name="uncertaintySets"></param>
        /// <param name="number"></param>
        /// <param name="isMatched"></param>
        /// <param name="hasZeros"></param>
        /// <returns></returns>
        public static List<NonDietaryExposureSet> MockNonDietaryExposureSets(
            ICollection<Individual> individuals,
            ICollection<Compound> substances,
            ICollection<ExposurePathType> nonDietaryExposureRoutes,
            IRandom random,
            ExternalExposureUnit exposureUnit = ExternalExposureUnit.mgPerKgBWPerDay,
            bool uncertaintySets = false,
            int number = 1,
            bool isMatched = false,
            bool hasZeros = false
        ) {
            var surveys = new List<NonDietarySurvey>();
            for (int i = 0; i < number; i++) {
                var survey = new NonDietarySurvey() {
                    ExposureUnit = exposureUnit,
                    Code = $"NonDietarySurvey{i}",
                    Description = "Description",
                    Location = "Location",
                    Date = new System.DateTime(),
                    ProportionZeros = hasZeros ? random.NextDouble() *100: 0,
            };
                surveys.Add(survey);
            }
            var code = string.Empty;
            var result = generateSurveyExposureSets(surveys, nonDietaryExposureRoutes, individuals, substances, code, random, uncertaintySets, isMatched);
            return result;
        }

        /// <summary>
        /// Creates a nondietary exposure set
        /// </summary>
        /// <param name="surveys"></param>
        /// <param name="nonDietaryExposureRoutes"></param>
        /// <param name="individuals"></param>
        /// <param name="substances"></param>
        /// <param name="idUncertaintySet"></param>
        /// <param name="random"></param>
        /// <param name="uncertaintySets"></param>
        /// <param name="isMatched"></param>
        /// <returns></returns>
        private static List<NonDietaryExposureSet> generateSurveyExposureSets(
            List<NonDietarySurvey> surveys,
            ICollection<ExposurePathType> nonDietaryExposureRoutes,
            ICollection<Individual> individuals,
            ICollection<Compound> substances,
            string idUncertaintySet,
            IRandom random,
            bool uncertaintySets = false,
            bool isMatched = false
        ) {
            if (uncertaintySets) {
                idUncertaintySet = "idUncertainty";
            }
            var nonDietaryExposureSet = new List<NonDietaryExposureSet>();
            foreach (var item in surveys) {
                var sets = individuals
                    .Select(r => {
                        var individualCode = r.Code;
                        if (!isMatched) {
                            individualCode = $"{r.Code}_{item.Code}";
                        }
                        return new NonDietaryExposureSet() {
                            Code = idUncertaintySet,
                            IndividualCode = individualCode,
                            NonDietaryExposures = substances
                                 .Select(s => new NonDietaryExposure() {
                                     Compound = s,
                                     Dermal = nonDietaryExposureRoutes.Contains(ExposurePathType.Dermal) ? random.NextDouble() : 0D,
                                     Oral = nonDietaryExposureRoutes.Contains(ExposurePathType.Oral) ? random.NextDouble() : 0D,
                                     Inhalation = nonDietaryExposureRoutes.Contains(ExposurePathType.Inhalation) ? random.NextDouble() : 0D,
                                     IdIndividual = $"{r.Code}_{item.Code}",
                                 })
                                 .ToList(),
                            NonDietarySurvey = item
                        };
                    })
                .ToList();
                nonDietaryExposureSet.AddRange(sets);
            }
            return nonDietaryExposureSet;
        }
    }
}
