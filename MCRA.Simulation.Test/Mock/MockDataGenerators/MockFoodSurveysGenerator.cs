using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock food surveys
    /// </summary>
    public static class MockFoodSurveysGenerator
    {

        private static string[] _defaultFoodSurveys = {
            "Survey1", "Survey2", "Survey3",
            "Survey4",
        };


        /// <summary>
        /// Creates a list of food surveys
        /// </summary>
        /// <param name="n"></param>
        /// <param name="individuals"></param>
        /// <returns></returns>
        public static List<FoodSurvey> Create(int n, ICollection<Individual> individuals) {
            if (n <= _defaultFoodSurveys.Length) {

                var result = _defaultFoodSurveys.Take(n).Select(r => {
                    foreach (var item in individuals) {
                        item.CodeFoodSurvey = r;
                    }
                    return new FoodSurvey() {
                        Code = r,
                        AgeUnitString = "Year",
                        Location = "Location",
                        Description = "Description",
                        Individuals = individuals,
                        NumberOfSurveyDays = individuals.First().NumberOfDaysInSurvey,
                    };
                }).ToList();
                return result;
            }
            throw new Exception($"Cannot create more than {_defaultFoodSurveys.Length} mock foodSurveys using this method!");
        }
    }
}
