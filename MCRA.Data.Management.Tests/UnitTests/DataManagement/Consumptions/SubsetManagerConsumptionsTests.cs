using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement.Consumptions {

    [TestClass]
    public class SubsetManagerConsumptionsTests : SubsetManagerTestsBase {

        /// <summary>
        /// Tests correct loading of the food surveys. Verification by checking the expected
        /// food surveys of the compiled data source ("ValidationSurvey" and "EmptySurvey").
        /// </summary>
        [TestMethod]
        public void SubsetManagerConsumptions_TestLoadFoodSurveys() {
            _rawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", SourceTableGroup.Survey);
            var foodSurveys = _compiledDataManager.GetAllFoodSurveys().Values;
            Assert.AreEqual(2, foodSurveys.Count);
            var foodSurveyCodes = foodSurveys.Select(fs => fs.Code).ToList();
            CollectionAssert.AreEquivalent(new List<string>() { "ValidationSurvey", "EmptySurvey" }, foodSurveyCodes);
        }

        /// <summary>
        /// Tests correct loading of the consumptions. Verification by counting the consumptions per
        /// food. Counts: apple (=17), bananas (=7), and pineapple (=3).
        /// </summary>
        [TestMethod]
        public void SubsetManagerConsumptions_TestLoadConsumptions() {
            _rawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", SourceTableGroup.Survey);
            var consumptions = _compiledDataManager.GetAllFoodConsumptions();

            var foods = _compiledDataManager.GetAllFoods();
            var foodApple = foods["APPLE"];
            var foodBananas = foods["BANANAS"];
            var foodPineapple = foods["PINEAPPLE"];

            Assert.AreEqual(27, consumptions.Count);
            var consumptionsApple = consumptions.Where(c => c.Food == foodApple).ToList();
            var consumptionsBananas = consumptions.Where(c => c.Food == foodBananas).ToList();
            var consumptionsPineapple = consumptions.Where(c => c.Food == foodPineapple).ToList();
            Assert.AreEqual(17, consumptionsApple.Count);
            Assert.AreEqual(7, consumptionsBananas.Count);
            Assert.AreEqual(3, consumptionsPineapple.Count);
        }

        /// <summary>
        /// Tests correct loading of the individuals. Verification by counting the individuals.
        /// Expected: 10 individuals.
        /// </summary>
        [TestMethod]
        public void SubsetManagerConsumptions_TestLoadIndividuals() {
            _rawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", SourceTableGroup.Survey);
            var individuals = _compiledDataManager.GetAllIndividuals();
            Assert.AreEqual(10, individuals.Count);
        }

        /// <summary>
        /// Tests correct loading of the individual properties. Verification by checking the
        /// co-factor and co-variate properties. Co-factors: "Age", "Gender", "ExtraCofactor", and
        /// "ExtraCovariable". Co-variate: "Age" and "ExtraCovariate".
        /// </summary>
        [TestMethod]
        public void SubsetManagerConsumptions_TestLoadIndividualProperties() {
            _rawDataProvider.SetDataGroupsFromFolder(1, "_DataGroupsTest", SourceTableGroup.Survey);
            var consumptions = _compiledDataManager.GetAllFoodConsumptions();

            // Check the cofactors
            var coFactors = _subsetManager.AllIndividualProperties.Values;
            var coFactorNames = coFactors.Select(c => c.Name).ToList();
            Assert.AreEqual(4, coFactors.Count);
            CollectionAssert.Contains(coFactorNames, "Age");
            CollectionAssert.Contains(coFactorNames, "Gender");
            CollectionAssert.Contains(coFactorNames, "ExtraCofactor");
            CollectionAssert.Contains(coFactorNames, "ExtraCovariate");

            // Check the covariates
            var coVariables = _subsetManager.CovariableIndividualProperties;
            var coVariableNames = coVariables.Select(c => c.Name).ToList();
            Assert.AreEqual(2, coVariables.Count);
            CollectionAssert.Contains(coVariableNames, "Age");
            CollectionAssert.Contains(coVariableNames, "ExtraCovariate");
        }

        [TestMethod]
        public void SubsetManagerConsumptions_SelectedFoodSurveyTestNoSurvey() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys")
            );
            var selected = _subsetManager.SelectedFoodSurvey;
            Assert.IsNull(selected);
        }

        [TestMethod]
        public void SubsetManagerConsumptions_SelectedFoodSurveyTestWrongSurvey() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.FoodSurveys, new List<string>() { "ZZ" });
            var selected = _subsetManager.SelectedFoodSurvey;
            Assert.IsNull(selected);
        }

        [TestMethod]
        public void SubsetManagerConsumptions_SelectedFoodSurveyTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.FoodSurveys, new List<string>() { "s1" });
            var selected = _subsetManager.SelectedFoodSurvey;
            Assert.AreEqual("S1", selected.Code);
            Assert.AreEqual(_subsetManager.AllFoodSurveys["s1"], selected);
        }

        [TestMethod]
        public void SubsetManagerConsumptions_AvailableIndividualsTestNoSurvey() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals")
            );
            var selected = _subsetManager.AllIndividuals;
            Assert.AreEqual(5, selected.Count);
        }

        [TestMethod]
        public void SubsetManagerConsumptions_AvailableIndividualsTestWrongSurvey() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals")
            );
            _project.SetFilterCodes(ScopingType.FoodSurveys, new List<string>() { "ZZ" });
            var selected = _subsetManager.AllIndividuals;
            Assert.AreEqual(5, selected.Count);
        }

        [TestMethod]
        public void SubsetManagerConsumptions_AvailableIndividualsTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.FoodSurveys, new List<string>() { "s2" });
            var selected = _subsetManager.AllIndividuals.Values;
            Assert.AreEqual("3,4", string.Join(",", selected.Select(i => i.Code)));
        }

        [TestMethod]
        public void SubsetManagerConsumptions_AllIndividualPropertiesTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSurveys, @"ConsumptionsTests\FoodSurveys"),
                (ScopingType.DietaryIndividuals, @"ConsumptionsTests\Individuals"),
                (ScopingType.DietaryIndividualProperties, @"ConsumptionsTests\IndividualProperties"),
                (ScopingType.DietaryIndividualPropertyValues, @"ConsumptionsTests\IndividualPropertyValues")
            );

            var list = _subsetManager.AllIndividualProperties.Values;

            Assert.AreEqual(5, list.Count);
            Assert.AreEqual("Age,Gender,Factor,Salary,Bcode", string.Join(",", list.Select(p => p.Code)));

            var covariables = _subsetManager.CovariableIndividualProperties;
            Assert.AreEqual(3, covariables.Count);
            Assert.AreEqual("Age,Factor,Salary", string.Join(",", covariables.Select(p => p.Code)));

            var covariable = _subsetManager.CovariableIndividualProperty;
            var cofactor = _subsetManager.CofactorIndividualProperty;
            Assert.IsNull(covariable);
            Assert.IsNull(cofactor);

            _project.CovariatesSelectionSettings.NameCofactor = "factor";
            _project.CovariatesSelectionSettings.NameCovariable = "gender";
            covariable = _subsetManager.CovariableIndividualProperty;
            cofactor = _subsetManager.CofactorIndividualProperty;
            Assert.AreEqual("Gender", covariable.Code);
            Assert.AreEqual("Factor", cofactor.Code);
        }
    }
}
