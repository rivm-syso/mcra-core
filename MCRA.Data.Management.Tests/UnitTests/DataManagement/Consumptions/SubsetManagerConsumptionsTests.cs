using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement.Consumptions {

    [TestClass]
    public class SubsetManagerConsumptionsTests : CompiledTestsBase {

        /// <summary>
        /// Tests correct loading of the food surveys. Verification by checking the expected
        /// food surveys of the compiled data source ("ValidationSurvey" and "EmptySurvey").
        /// </summary>
        [TestMethod]
        public void SubsetManagerConsumptions_TestLoadFoodSurveys() {
            RawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", SourceTableGroup.Survey);
            var foodSurveys = CompiledDataManager.GetAllFoodSurveys().Values;
            Assert.HasCount(2, foodSurveys);
            var foodSurveyCodes = foodSurveys.Select(fs => fs.Code).ToList();
            CollectionAssert.AreEquivalent(new List<string>() { "ValidationSurvey", "EmptySurvey" }, foodSurveyCodes);
        }

        /// <summary>
        /// Tests correct loading of the consumptions. Verification by counting the consumptions per
        /// food. Counts: apple (=17), bananas (=7), and pineapple (=3).
        /// </summary>
        [TestMethod]
        public void SubsetManagerConsumptions_TestLoadConsumptions() {
            RawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", SourceTableGroup.Survey);
            var consumptions = CompiledDataManager.GetAllFoodConsumptions();

            var foods = CompiledDataManager.GetAllFoods();
            var foodApple = foods["APPLE"];
            var foodBananas = foods["BANANAS"];
            var foodPineapple = foods["PINEAPPLE"];

            Assert.HasCount(27, consumptions);
            var consumptionsApple = consumptions.Where(c => c.Food == foodApple).ToList();
            var consumptionsBananas = consumptions.Where(c => c.Food == foodBananas).ToList();
            var consumptionsPineapple = consumptions.Where(c => c.Food == foodPineapple).ToList();
            Assert.HasCount(17, consumptionsApple);
            Assert.HasCount(7, consumptionsBananas);
            Assert.HasCount(3, consumptionsPineapple);
        }

        /// <summary>
        /// Tests correct loading of the individuals. Verification by counting the individuals.
        /// Expected: 10 individuals.
        /// </summary>
        [TestMethod]
        public void SubsetManagerConsumptions_TestLoadIndividuals() {
            RawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", SourceTableGroup.Survey);
            var individuals = CompiledDataManager.GetAllIndividuals();
            Assert.HasCount(10, individuals);
        }

        /// <summary>
        /// Tests correct loading of the individual properties. Verification by checking the
        /// co-factor and co-variate properties. Co-factors: "Age", "Gender", "ExtraCofactor", and
        /// "ExtraCovariable". Co-variate: "Age" and "ExtraCovariate".
        /// </summary>
        [TestMethod]
        public void SubsetManagerConsumptions_TestLoadIndividualProperties() {
            RawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", SourceTableGroup.Survey);
            var consumptions = CompiledDataManager.GetAllFoodConsumptions();

            // Check the cofactors
            var coFactors = SubsetManager.AllIndividualProperties.Values;
            var coFactorNames = coFactors.Select(c => c.Name).ToList();
            Assert.HasCount(4, coFactors);
            CollectionAssert.Contains(coFactorNames, "Age");
            CollectionAssert.Contains(coFactorNames, "Gender");
            CollectionAssert.Contains(coFactorNames, "ExtraCofactor");
            CollectionAssert.Contains(coFactorNames, "ExtraCovariate");

            // Check the covariates
            var coVariables = SubsetManager.CovariableIndividualProperties;
            var coVariableNames = coVariables.Select(c => c.Name).ToList();
            Assert.HasCount(2, coVariables);
            CollectionAssert.Contains(coVariableNames, "Age");
            CollectionAssert.Contains(coVariableNames, "ExtraCovariate");
        }

        [TestMethod]
        public void SubsetManagerConsumptions_SelectedFoodSurveyTestNoSurvey() {
            RawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests/FoodSurveys")
            );
            var selected = SubsetManager.SelectedFoodSurvey;
            Assert.IsNull(selected);
        }

        [TestMethod]
        public void SubsetManagerConsumptions_SelectedFoodSurveyTestWrongSurvey() {
            RawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests/FoodSurveys")
            );
            RawDataProvider.SetFilterCodes(ScopingType.FoodSurveys, new List<string>() { "ZZ" });
            var selected = SubsetManager.SelectedFoodSurvey;
            Assert.IsNull(selected);
        }

        [TestMethod]
        public void SubsetManagerConsumptions_SelectedFoodSurveyTest() {
            RawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests/FoodSurveys")
            );
            RawDataProvider.SetFilterCodes(ScopingType.FoodSurveys, new List<string>() { "s1" });
            var selected = SubsetManager.SelectedFoodSurvey;
            Assert.AreEqual("S1", selected.Code);
            Assert.AreEqual(SubsetManager.AllFoodSurveys["s1"], selected);
        }

        [TestMethod]
        public void SubsetManagerConsumptions_AvailableIndividualsTestNoSurvey() {
            RawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests/FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests/Individuals")
            );
            var selected = SubsetManager.AllIndividuals;
            Assert.HasCount(5, selected);
        }

        [TestMethod]
        public void SubsetManagerConsumptions_AvailableIndividualsTestWrongSurvey() {
            RawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests/FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests/Individuals")
            );
            Project.SetFilterCodes(ScopingType.FoodSurveys, new List<string>() { "ZZ" });
            var selected = SubsetManager.AllIndividuals;
            Assert.HasCount(5, selected);
        }

        [TestMethod]
        public void SubsetManagerConsumptions_AvailableIndividualsTest() {
            RawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests/FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests/Individuals")
            );
            RawDataProvider.SetFilterCodes(ScopingType.FoodSurveys, new List<string>() { "s2" });
            var selected = SubsetManager.AllIndividuals.Values;
            Assert.AreEqual("3,4", string.Join(",", selected.Select(i => i.Code)));
        }

        [TestMethod]
        public void SubsetManagerConsumptions_AllIndividualPropertiesTest() {
            RawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests/FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests/Individuals"),
                (ScopingType.DietaryIndividualProperties, @"ConsumptionsTests/IndividualProperties"),
                (ScopingType.DietaryIndividualPropertyValues, @"ConsumptionsTests/IndividualPropertyValues")
            );

            var list = SubsetManager.AllIndividualProperties.Values;

            Assert.HasCount(5, list);
            Assert.AreEqual("Age,Gender,Factor,Salary,Bcode", string.Join(",", list.Select(p => p.Code)));

            var covariables = SubsetManager.CovariableIndividualProperties;
            Assert.HasCount(3, covariables);
            Assert.AreEqual("Age,Factor,Salary", string.Join(",", covariables.Select(p => p.Code)));

            var covariable = SubsetManager.CovariableIndividualProperty;
            var cofactor = SubsetManager.CofactorIndividualProperty;
            Assert.IsNull(covariable);
            Assert.IsNull(cofactor);

            var config = Project.DietaryExposuresSettings;
            config.NameCofactor = "factor";
            config.NameCovariable = "gender";
            covariable = SubsetManager.CovariableIndividualProperty;
            cofactor = SubsetManager.CofactorIndividualProperty;
            Assert.AreEqual("Gender", covariable.Code);
            Assert.AreEqual("Factor", cofactor.Code);
        }
    }
}
