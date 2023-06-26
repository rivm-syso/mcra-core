using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, Estimates
    /// </summary>
    [TestClass]
    public class FrequenciesCovariableChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Creates frequencies covariable chart, test FrequenciesModelGraphicsSection view
        /// </summary>
        [TestMethod]
        public void FrequenciesCovariableChart_Test1() {
            var xyCollectorConditional = new List<ConditionalPrediction> {
                new() { Prediction = .2, Cofactor = "male", Covariable = 4 },
                new() { Prediction = .3, Cofactor = "male", Covariable = 5 },
                new() { Prediction = .4, Cofactor = "male", Covariable = 6 },
                new() { Prediction = .5, Cofactor = "male", Covariable = 7 },
                new() { Prediction = .6, Cofactor = "male", Covariable = 8 },
                new() { Prediction = .12, Cofactor = "female", Covariable = 2 },
                new() { Prediction = .13, Cofactor = "female", Covariable = 3 },
                new() { Prediction = .14, Cofactor = "female", Covariable = 4 },
                new() { Prediction = .15, Cofactor = "female", Covariable = 5 },
                new() { Prediction = .16, Cofactor = "female", Covariable = 6 }
            };

            var xyCollectorConditionalData = new List<ConditionalPrediction> {
                new() { Prediction = .1, Cofactor = "male", Covariable = 4 },
                new() { Prediction = .4, Cofactor = "male", Covariable = 5 },
                new() { Prediction = .2, Cofactor = "male", Covariable = 6 },
                new() { Prediction = .2, Cofactor = "male", Covariable = 7 },
                new() { Prediction = .7, Cofactor = "male", Covariable = 8 },
                new() { Prediction = .10, Cofactor = "female", Covariable = 2 },
                new() { Prediction = .14, Cofactor = "female", Covariable = 3 },
                new() { Prediction = .15, Cofactor = "female", Covariable = 4 },
                new() { Prediction = .13, Cofactor = "female", Covariable = 5 },
                new() { Prediction = .18, Cofactor = "female", Covariable = 6 },
                new() { Prediction = .3, Cofactor = "male", Covariable = 4 },
                new() { Prediction = .8, Cofactor = "male", Covariable = 5 },
                new() { Prediction = .1, Cofactor = "male", Covariable = 6 },
                new() { Prediction = .11, Cofactor = "male", Covariable = 7 },
                new() { Prediction = .4, Cofactor = "male", Covariable = 8 },
                new() { Prediction = .17, Cofactor = "female", Covariable = 2 },
                new() { Prediction = .19, Cofactor = "female", Covariable = 3 },
                new() { Prediction = .10, Cofactor = "female", Covariable = 4 },
                new() { Prediction = .14, Cofactor = "female", Covariable = 5 },
                new() { Prediction = .11, Cofactor = "female", Covariable = 6 }
            };
            var section = new FrequenciesModelGraphicsSection() {
                CovariableName = "age",
                CofactorName = "gender",
                Predictions = new ConditionalPredictionResults() {
                    ConditionalPrediction = xyCollectorConditional,
                    ConditionalData = xyCollectorConditionalData,
                },

            };

            var chart = new FrequenciesCovariableChartCreator(section);
            RenderChart(chart, $"TestCreate1");
            AssertIsValidView(section);
        }

        /// <summary>
        ///  Creates frequencies covariable chart
        /// </summary>
        [TestMethod]
        public void FrequenciesCovariableChart_Test2() {
            var xyCollectorConditional = new List<ConditionalPrediction> {
                new() { Prediction = .2, Cofactor = "male", Covariable = 4 },
                new() { Prediction = .3, Cofactor = "male", Covariable = 5 },
                new() { Prediction = .4, Cofactor = "male", Covariable = 6 },
                new() { Prediction = .5, Cofactor = "male", Covariable = 7 },
                new() { Prediction = .6, Cofactor = "male", Covariable = 8 },
                new() { Prediction = .12, Cofactor = "male", Covariable = 9 },
                new() { Prediction = .13, Cofactor = "male", Covariable = 10 },
                new() { Prediction = .14, Cofactor = "male", Covariable = 11 },
                new() { Prediction = .15, Cofactor = "male", Covariable = 12 },
                new() { Prediction = .16, Cofactor = "male", Covariable = 13 }
            };

            var xyCollectorConditionalData = new List<ConditionalPrediction> {
                new() { Prediction = .1, Cofactor = "male", Covariable = 4 },
                new() { Prediction = .4, Cofactor = "male", Covariable = 5 },
                new() { Prediction = .2, Cofactor = "male", Covariable = 6 },
                new() { Prediction = .2, Cofactor = "male", Covariable = 7 },
                new() { Prediction = .7, Cofactor = "male", Covariable = 8 },
                new() { Prediction = .10, Cofactor = "male", Covariable = 2 },
                new() { Prediction = .14, Cofactor = "male", Covariable = 3 },
                new() { Prediction = .15, Cofactor = "male", Covariable = 4 },
                new() { Prediction = .13, Cofactor = "male", Covariable = 5 },
                new() { Prediction = .18, Cofactor = "male", Covariable = 6 },
                new() { Prediction = .3, Cofactor = "male", Covariable = 4 },
                new() { Prediction = .8, Cofactor = "male", Covariable = 5 },
                new() { Prediction = .1, Cofactor = "male", Covariable = 6 },
                new() { Prediction = .11, Cofactor = "male", Covariable = 7 },
                new() { Prediction = .4, Cofactor = "male", Covariable = 8 },
                new() { Prediction = .17, Cofactor = "male", Covariable = 2 },
                new() { Prediction = .19, Cofactor = "male", Covariable = 3 },
                new() { Prediction = .10, Cofactor = "male", Covariable = 4 },
                new() { Prediction = .14, Cofactor = "male", Covariable = 5 },
                new() { Prediction = .11, Cofactor = "male", Covariable = 6 }
            };
            var frequenciesModelGraphicsSection = new FrequenciesModelGraphicsSection() {
                CovariableName = "age",
                CofactorName = "gender",
                Predictions = new ConditionalPredictionResults() {
                    ConditionalPrediction = xyCollectorConditional,
                    ConditionalData = xyCollectorConditionalData,
                },

            };

            var chart = new FrequenciesCovariableChartCreator(frequenciesModelGraphicsSection);
            RenderChart(chart, $"TestCreate2");
        }
    }
}


