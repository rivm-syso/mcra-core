using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, Estimates
    /// </summary>
    [TestClass]
    public class AmountsCovariableChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Creates amounts covariable charts, test NormalAmountsModelGraphicsSection view
        /// </summary>
        [TestMethod]
        public void AmountsCovariableChart_Test1() {
            var conditionalPrediction = new List<ConditionalPrediction>();
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 2, Cofactor = "male", Covariable = 4 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 3, Cofactor = "male", Covariable = 5 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 4, Cofactor = "male", Covariable = 6 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 5, Cofactor = "male", Covariable = 7 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 6, Cofactor = "male", Covariable = 8 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 12, Cofactor = "female", Covariable = 2 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 13, Cofactor = "female", Covariable = 3 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 14, Cofactor = "female", Covariable = 4 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 15, Cofactor = "female", Covariable = 5 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 16, Cofactor = "female", Covariable = 6 });

            var conditionalPredictionData = new List<ConditionalPrediction>();
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 1, Cofactor = "male", Covariable = 4 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 4, Cofactor = "male", Covariable = 5 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 2, Cofactor = "male", Covariable = 6 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 2, Cofactor = "male", Covariable = 7 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 7, Cofactor = "male", Covariable = 8 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 10, Cofactor = "female", Covariable = 2 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 14, Cofactor = "female", Covariable = 3 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 15, Cofactor = "female", Covariable = 4 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 13, Cofactor = "female", Covariable = 5 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 18, Cofactor = "female", Covariable = 6 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 3, Cofactor = "male", Covariable = 4 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 8, Cofactor = "male", Covariable = 5 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 1, Cofactor = "male", Covariable = 6 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 11, Cofactor = "male", Covariable = 7 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 4, Cofactor = "male", Covariable = 8 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 17, Cofactor = "female", Covariable = 2 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 19, Cofactor = "female", Covariable = 3 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 10, Cofactor = "female", Covariable = 4 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 14, Cofactor = "female", Covariable = 5 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 11, Cofactor = "female", Covariable = 6 });
            var section = new NormalAmountsModelGraphicsSection() {
                CovariableName = "age",
                CofactorName = "gender",
                Predictions = new ConditionalPredictionResults() {
                    ConditionalPrediction = conditionalPrediction,
                    ConditionalData = conditionalPredictionData,
                },

            };
            var chart = new AmountsCovariableChartCreator(section);
            RenderChart(chart, $"TestCreate1");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Creates Amounts covariable charts
        /// </summary>
        [TestMethod]
        public void AmountsCovariableChart_Test2() {
            var conditionalPrediction = new List<ConditionalPrediction>();
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 2, Cofactor = "male", Covariable = 4 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 3, Cofactor = "male", Covariable = 5 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 4, Cofactor = "male", Covariable = 6 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 5, Cofactor = "male", Covariable = 7 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 6, Cofactor = "male", Covariable = 8 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 12, Cofactor = "male", Covariable = 9 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 13, Cofactor = "male", Covariable = 10 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 14, Cofactor = "male", Covariable = 11 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 15, Cofactor = "male", Covariable = 12 });
            conditionalPrediction.Add(new ConditionalPrediction() { Prediction = 16, Cofactor = "male", Covariable = 13 });

            var conditionalPredictionData = new List<ConditionalPrediction>();
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 1, Cofactor = "male", Covariable = 4 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 4, Cofactor = "male", Covariable = 5 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 2, Cofactor = "male", Covariable = 6 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 2, Cofactor = "male", Covariable = 7 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 7, Cofactor = "male", Covariable = 8 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 10, Cofactor = "male", Covariable = 2 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 14, Cofactor = "male", Covariable = 3 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 15, Cofactor = "male", Covariable = 4 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 13, Cofactor = "male", Covariable = 5 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 18, Cofactor = "male", Covariable = 6 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 3, Cofactor = "male", Covariable = 4 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 8, Cofactor = "male", Covariable = 5 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 1, Cofactor = "male", Covariable = 6 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 11, Cofactor = "male", Covariable = 7 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 4, Cofactor = "male", Covariable = 8 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 17, Cofactor = "male", Covariable = 2 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 19, Cofactor = "male", Covariable = 3 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 10, Cofactor = "male", Covariable = 4 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 14, Cofactor = "male", Covariable = 5 });
            conditionalPredictionData.Add(new ConditionalPrediction() { Prediction = 11, Cofactor = "male", Covariable = 6 });
            var section = new NormalAmountsModelGraphicsSection() {
                CovariableName = "age",
                CofactorName = "gender",
                Predictions = new ConditionalPredictionResults() {
                    ConditionalPrediction = conditionalPrediction,
                    ConditionalData = conditionalPredictionData,
                },
            };
            var chart = new AmountsCovariableChartCreator(section);
            RenderChart(chart, $"TestCreate2");
        }
    }
}
