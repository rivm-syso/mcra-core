using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, Estimates
    /// </summary>
    [TestClass]
    public class FrequenciesModelGraphicsSectionTests : ChartCreatorTestBase {
        /// <summary>
        /// Test FrequenciesModelGraphicsSection view
        /// </summary>
        [TestMethod]
        public void FrequenciesModelGraphicsSection_Test1() {
            var xyCollectorConditional = new List<ConditionalPrediction>();
            xyCollectorConditional.Add(new ConditionalPrediction() { Prediction = .2, Cofactor = "male", Covariable = 4 });
            xyCollectorConditional.Add(new ConditionalPrediction() { Prediction = .3, Cofactor = "male", Covariable = 5 });
            xyCollectorConditional.Add(new ConditionalPrediction() { Prediction = .4, Cofactor = "male", Covariable = 6 });
            xyCollectorConditional.Add(new ConditionalPrediction() { Prediction = .5, Cofactor = "male", Covariable = 7 });
            xyCollectorConditional.Add(new ConditionalPrediction() { Prediction = .6, Cofactor = "male", Covariable = 8 });
            xyCollectorConditional.Add(new ConditionalPrediction() { Prediction = .12, Cofactor = "female", Covariable = 2 });
            xyCollectorConditional.Add(new ConditionalPrediction() { Prediction = .13, Cofactor = "female", Covariable = 3 });
            xyCollectorConditional.Add(new ConditionalPrediction() { Prediction = .14, Cofactor = "female", Covariable = 4 });
            xyCollectorConditional.Add(new ConditionalPrediction() { Prediction = .15, Cofactor = "female", Covariable = 5 });
            xyCollectorConditional.Add(new ConditionalPrediction() { Prediction = .16, Cofactor = "female", Covariable = 6 });

            var xyCollectorConditionalData = new List<ConditionalPrediction>();
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .1, Cofactor = "male", Covariable = 4 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .4, Cofactor = "male", Covariable = 5 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .2, Cofactor = "male", Covariable = 6 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .2, Cofactor = "male", Covariable = 7 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .7, Cofactor = "male", Covariable = 8 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .10, Cofactor = "female", Covariable = 2 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .14, Cofactor = "female", Covariable = 3 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .15, Cofactor = "female", Covariable = 4 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .13, Cofactor = "female", Covariable = 5 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .18, Cofactor = "female", Covariable = 6 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .3, Cofactor = "male", Covariable = 4 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .8, Cofactor = "male", Covariable = 5 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .1, Cofactor = "male", Covariable = 6 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .11, Cofactor = "male", Covariable = 7 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .4, Cofactor = "male", Covariable = 8 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .17, Cofactor = "female", Covariable = 2 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .19, Cofactor = "female", Covariable = 3 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .10, Cofactor = "female", Covariable = 4 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .14, Cofactor = "female", Covariable = 5 });
            xyCollectorConditionalData.Add(new ConditionalPrediction() { Prediction = .11, Cofactor = "female", Covariable = 6 });
            var section = new FrequenciesModelGraphicsSection() {
                CovariableName = "age",
                CofactorName = "gender",
                Predictions = new ConditionalPredictionResults() {
                    ConditionalPrediction = xyCollectorConditional,
                    ConditionalData = xyCollectorConditionalData,
                },

            };
            AssertIsValidView(section);
        }
    }
}


