using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling {
    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass()]
    public class DesignMatrixToolsTests {

        private List<Compound> _substances;
        private List<Food> _foods;
        private Dictionary<Compound, double> _rpfs;
        private Dictionary<Compound, double> _membershipProbabilities;

        /// <summary>
        /// Creates design matrix for frequencies, CovariateModelType.Constant, empty list
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMFrequencyEmptyList() {
            var individualFrequencies = new List<IndividualFrequency>();
            var result = DesignMatrixTools.GetDMFrequency(individualFrequencies, CovariateModelType.Constant, 5);
            Assert.AreEqual("constant", result.DesignMatrixDescription.Single());
        }

        /// <summary>
        /// Creates design matrix for frequencies, CovariateModelType.Constant, one element
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMFrequencySingleElement1() {
            var individualFrequency = mockSingleIndividualFrequency();
            var individualFrequencies = new List<IndividualFrequency>() { individualFrequency };
            var result = DesignMatrixTools.GetDMFrequency(individualFrequencies, CovariateModelType.Constant, 5);
            Assert.AreEqual("constant", result.DesignMatrixDescription.Single());
            Assert.IsNull(result.Cofactor);
            Assert.IsNull(result.Covariable);
            Assert.AreEqual(.5, result.Weights.First());
        }

        /// <summary>
        /// Creates design matrix for frequencies, CovariateModelType.Constant
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMFrequencyListConstant() {
            var individualFrequencies = mockIndividualFrequencies();
            var nPol = 0;
            var fdr = DesignMatrixTools.GetDMFrequency(individualFrequencies, CovariateModelType.Constant, nPol);
            Assert.HasCount(1 + nPol, fdr.DesignMatrixDescription);
            Assert.IsNull(fdr.Cofactor);
            Assert.IsNull(fdr.Covariable);
            Assert.AreEqual(10, fdr.Weights.First());
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualFrequencies, CovariateModelType.Constant, fdr, null);
            Assert.HasCount(1, result.DesignMatrixDescription);
            Assert.AreEqual("constant", result.DesignMatrixDescription.First());
            Assert.IsNull(result.Cofactor);
            Assert.IsNull(result.Covariable);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualFrequencies, CovariateModelType.Constant, fdr);
            Assert.HasCount(1, result.DesignMatrixDescription);
            Assert.AreEqual("constant", result.DesignMatrixDescription.First());
            Assert.IsNull(result.Cofactor);
            Assert.IsNull(result.Covariable);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualFrequencies, CovariateModelType.Constant, fdr);
            Assert.HasCount(1, result.DesignMatrixDescription);
            Assert.AreEqual("constant", result.DesignMatrixDescription.First());
            Assert.IsNull(result.Cofactor);
            Assert.IsNull(result.Covariable);
            Assert.IsNull(result.Weights);
        }

        /// <summary>
        /// Creates design matrix for frequencies, CovariateModelType.Cofactor
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMFrequencyListCofactor() {
            var individualFrequencies = mockIndividualFrequencies();
            var nPol = 3;
            var fdr = DesignMatrixTools.GetDMFrequency(individualFrequencies, CovariateModelType.Cofactor, nPol);
            Assert.HasCount(2, fdr.DesignMatrixDescription);
            Assert.HasCount(2, fdr.Cofactor);
            Assert.IsNull(fdr.Covariable);
            Assert.AreEqual(5, fdr.Weights.First());
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualFrequencies, CovariateModelType.Cofactor, fdr, null);
            Assert.HasCount(2, result.DesignMatrixDescription);
            Assert.HasCount(2, result.Cofactor);
            Assert.IsNull(result.Covariable);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualFrequencies, CovariateModelType.Cofactor, fdr);
            Assert.HasCount(2, result.DesignMatrixDescription);
            Assert.HasCount(2, result.Cofactor);
            Assert.IsNull(result.Covariable);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualFrequencies, CovariateModelType.Cofactor, fdr);
            Assert.HasCount(2, result.DesignMatrixDescription);
            Assert.HasCount(20, result.Cofactor);
            Assert.IsNull(result.Covariable);
            Assert.IsNull(result.Weights);
        }

        /// <summary>
        /// Creates design matrix for frequencies, CovariateModelType.Covariable
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMFrequencyListCovariable() {
            var individualFrequencies = mockIndividualFrequencies();
            var nPol = 3;
            var fdr = DesignMatrixTools.GetDMFrequency(individualFrequencies, CovariateModelType.Covariable, nPol);
            Assert.HasCount(1 + nPol, fdr.DesignMatrixDescription);
            Assert.IsNull(fdr.Cofactor);
            Assert.HasCount(10, fdr.Covariable);
            Assert.AreEqual(1, fdr.Weights.First());
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualFrequencies, CovariateModelType.Covariable, fdr, predictions);
            Assert.HasCount(4, result.DesignMatrixDescription);
            Assert.HasCount(10, result.Covariable);
            Assert.IsNull(result.Cofactor);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualFrequencies, CovariateModelType.Covariable, fdr);
            Assert.HasCount(4, result.DesignMatrixDescription);
            Assert.HasCount(10, result.Covariable);
            Assert.IsNull(result.Cofactor);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualFrequencies, CovariateModelType.Covariable, fdr);
            Assert.HasCount(4, result.DesignMatrixDescription);
            Assert.HasCount(20, result.Covariable);
            Assert.IsNull(result.Cofactor);
            Assert.IsNull(result.Weights);
        }

        /// <summary>
        /// Creates design matrix for frequencies, CovariateModelType.CovariableCofactor
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMFrequencyListCovariableCofactor() {
            var individualFrequencies = mockIndividualFrequencies();
            var nPol = 3;
            var fdr = DesignMatrixTools.GetDMFrequency(individualFrequencies, CovariateModelType.CovariableCofactor, nPol);
            Assert.HasCount(2 + nPol, fdr.DesignMatrixDescription);
            Assert.HasCount(20, fdr.Cofactor);
            Assert.HasCount(20, fdr.Covariable);
            Assert.AreEqual(0.5, fdr.Weights.First());
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualFrequencies, CovariateModelType.CovariableCofactor, fdr, predictions);
            Assert.HasCount(5, result.DesignMatrixDescription);
            Assert.HasCount(20, result.Covariable);
            Assert.HasCount(20, result.Cofactor);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualFrequencies, CovariateModelType.CovariableCofactor, fdr);
            Assert.HasCount(5, result.DesignMatrixDescription);
            Assert.HasCount(20, result.Covariable);
            Assert.HasCount(20, result.Cofactor);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualFrequencies, CovariateModelType.CovariableCofactor, fdr);
            Assert.HasCount(5, result.DesignMatrixDescription);
            Assert.HasCount(20, result.Covariable);
            Assert.HasCount(20, result.Cofactor);
            Assert.IsNull(result.Weights);
        }

        /// <summary>
        /// Creates design matrix for frequencies, CovariateModelType.CovariableCofactorInteraction
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMFrequencyListCovariableCofactorInt() {
            var individualFrequencies = mockIndividualFrequencies();
            var nPol = 3;
            var fdr = DesignMatrixTools.GetDMFrequency(individualFrequencies, CovariateModelType.CovariableCofactorInteraction, nPol);
            Assert.HasCount(8, fdr.DesignMatrixDescription);
            Assert.HasCount(20, fdr.Cofactor);
            Assert.HasCount(20, fdr.Covariable);
            Assert.AreEqual(0.5, fdr.Weights.First());
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualFrequencies, CovariateModelType.CovariableCofactorInteraction, fdr, predictions);
            Assert.HasCount(8, result.DesignMatrixDescription);
            Assert.HasCount(20, result.Covariable);
            Assert.HasCount(20, result.Cofactor);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualFrequencies, CovariateModelType.CovariableCofactorInteraction, fdr);
            Assert.HasCount(8, result.DesignMatrixDescription);
            Assert.HasCount(20, result.Covariable);
            Assert.HasCount(20, result.Cofactor);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualFrequencies, CovariateModelType.CovariableCofactorInteraction, fdr);
            Assert.HasCount(8, result.DesignMatrixDescription);
            Assert.HasCount(20, result.Covariable);
            Assert.HasCount(20, result.Cofactor);
            Assert.IsNull(result.Weights);
        }

        /// <summary>
        /// Amounts, empty list
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMAmountsEmptyList() {
            var individualDailyIntakes = new List<DietaryIndividualDayIntake>();
            var foods = new List<Food>();
            var intakeTransformer = new LogTransformer();
            var relativePotencyFactors = new Dictionary<Compound, double>();
            var membershipProbabilities = new Dictionary<Compound, double>();

            var individualIntakeAmountsCalculator = new SimpleIndividualDayIntakesCalculator(_substances, _rpfs, _membershipProbabilities, false, _foods);
            var individualAmounts = individualIntakeAmountsCalculator.ComputeIndividualAmounts(individualDailyIntakes);
            var transformedIndividualAmounts = AmountsModelBase.ComputeTransformedPositiveIndividualAmounts(individualAmounts, intakeTransformer);

            var adr = DesignMatrixTools.GetDMAmount(transformedIndividualAmounts, CovariateModelType.Constant, 5);
            Assert.IsEmpty(adr.DesignMatrixDescriptions);
            var individualIntakeAmounts = getIndividualIntakeAmounts(individualDailyIntakes, foods, intakeTransformer, relativePotencyFactors, membershipProbabilities);
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualIntakeAmounts, CovariateModelType.Constant, adr, null);
            Assert.IsNull(result.DesignMatrixDescriptions);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualIntakeAmounts, CovariateModelType.Constant, adr);
            Assert.IsNull(result.DesignMatrixDescriptions);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualIntakeAmounts, CovariateModelType.Constant, adr);
            Assert.IsEmpty(result.DesignMatrixDescriptions);
        }

        /// <summary>
        /// Amounts, constant
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMAmountsListConstant() {
            var individualDailyIntakes = mockIndividualDayIntakes();
            var intakeTransformer = new LogTransformer();

            var individualIntakeAmountsCalculator = new SimpleIndividualDayIntakesCalculator(_substances, _rpfs, _membershipProbabilities, false, _foods);
            var individualAmounts = individualIntakeAmountsCalculator.ComputeIndividualAmounts(individualDailyIntakes);
            var transformedIndividualAmounts = AmountsModelBase.ComputeTransformedPositiveIndividualAmounts(individualAmounts, intakeTransformer);

            var nPol = 3;
            var adr = DesignMatrixTools.GetDMAmount(transformedIndividualAmounts, CovariateModelType.Constant, nPol);
            Assert.IsNull(adr.Cofactors);
            Assert.IsNull(adr.Covariables);
            var individualIntakeAmounts = getIndividualIntakeAmounts(individualDailyIntakes, _foods, intakeTransformer, _rpfs, _membershipProbabilities);
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualIntakeAmounts, CovariateModelType.Constant, adr, null);
            Assert.IsNull(result.Cofactors);
            Assert.IsNull(result.Covariables);
            Assert.IsNull(result.IndividualSamplingWeights);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualIntakeAmounts, CovariateModelType.Constant, adr);
            Assert.IsNull(result.Cofactors);
            Assert.IsNull(result.Covariables);
            Assert.IsNull(result.IndividualSamplingWeights);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualIntakeAmounts, CovariateModelType.Constant, adr);
            Assert.IsNull(result.Cofactors);
            Assert.IsNull(result.Covariables);
            Assert.HasCount(20, result.IndividualSamplingWeights);
        }

        /// <summary>
        /// Amounts, cofactor
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMAmountsListCofactor() {
            var individualDailyIntakes = mockIndividualDayIntakes();
            var intakeTransformer = new LogTransformer();

            var individualIntakeAmountsCalculator = new SimpleIndividualDayIntakesCalculator(_substances, _rpfs, _membershipProbabilities, false, _foods);
            var individualAmounts = individualIntakeAmountsCalculator.ComputeIndividualAmounts(individualDailyIntakes);
            var transformedIndividualAmounts = AmountsModelBase.ComputeTransformedPositiveIndividualAmounts(individualAmounts, intakeTransformer);

            var adr = DesignMatrixTools.GetDMAmount(transformedIndividualAmounts, CovariateModelType.Cofactor, 3);
            Assert.AreEqual(individualDailyIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight), adr.IndividualSamplingWeights.Sum(), 1e-3);
            Assert.HasCount(1, adr.DesignMatrixDescriptions);
            Assert.HasCount(40, adr.Cofactors);
            Assert.IsNull(adr.Covariables);
            var individualIntakeAmounts = getIndividualIntakeAmounts(individualDailyIntakes, _foods, intakeTransformer, _rpfs, _membershipProbabilities);
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualIntakeAmounts, CovariateModelType.Cofactor, adr, null);
            Assert.HasCount(1, result.DesignMatrixDescriptions);
            Assert.HasCount(2, result.Cofactors);
            Assert.IsNull(result.Covariables);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualIntakeAmounts, CovariateModelType.Cofactor, adr);
            Assert.HasCount(1, result.DesignMatrixDescriptions);
            Assert.HasCount(2, result.Cofactors);
            Assert.IsNull(result.Covariables);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualIntakeAmounts, CovariateModelType.Cofactor, adr);
            Assert.HasCount(1, result.DesignMatrixDescriptions);
            Assert.HasCount(20, result.Cofactors);
            Assert.IsNull(result.Covariables);
            Assert.HasCount(20, result.IndividualSamplingWeights);
        }

        /// <summary>
        /// Amounts, covariable
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMAmountsListCovariable() {
            var individualDailyIntakes = mockIndividualDayIntakes();
            var intakeTransformer = new LogTransformer();
            var nPol = 3;

            var individualIntakeAmountsCalculator = new SimpleIndividualDayIntakesCalculator(_substances, _rpfs, _membershipProbabilities, false, _foods);
            var individualAmounts = individualIntakeAmountsCalculator.ComputeIndividualAmounts(individualDailyIntakes);
            var transformedIndividualAmounts = AmountsModelBase.ComputeTransformedPositiveIndividualAmounts(individualAmounts, intakeTransformer);

            var adr = DesignMatrixTools.GetDMAmount(transformedIndividualAmounts, CovariateModelType.Covariable, nPol);
            Assert.AreEqual(individualDailyIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight), adr.IndividualSamplingWeights.Sum(), 1e-3);
            Assert.HasCount(nPol, adr.DesignMatrixDescriptions);
            Assert.IsNull(adr.Cofactors);
            Assert.HasCount(40, adr.Covariables);
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var individualIntakeAmounts = getIndividualIntakeAmounts(individualDailyIntakes, _foods, intakeTransformer, _rpfs, _membershipProbabilities);
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualIntakeAmounts, CovariateModelType.Covariable, adr, predictions);
            Assert.HasCount(nPol, result.DesignMatrixDescriptions);
            Assert.IsNull(result.Cofactors);
            Assert.HasCount(10, result.Covariables);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualIntakeAmounts, CovariateModelType.Covariable, adr);
            Assert.HasCount(nPol, result.DesignMatrixDescriptions);
            Assert.IsNull(result.Cofactors);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualIntakeAmounts, CovariateModelType.Covariable, adr);
            Assert.HasCount(nPol, result.DesignMatrixDescriptions);
            Assert.IsNull(result.Cofactors);
            Assert.HasCount(20, result.Covariables);
            Assert.HasCount(20, result.IndividualSamplingWeights);
        }

        /// <summary>
        /// Amounts, covariable and cofactor
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMAmountsListCovariableCofactor() {
            var individualDailyIntakes = mockIndividualDayIntakes();
            var intakeTransformer = new LogTransformer();
            var nPol = 3;

            var individualIntakeAmountsCalculator = new SimpleIndividualDayIntakesCalculator(_substances, _rpfs, _membershipProbabilities, false, _foods);
            var individualAmounts = individualIntakeAmountsCalculator.ComputeIndividualAmounts(individualDailyIntakes);
            var transformedIndividualAmounts = AmountsModelBase.ComputeTransformedPositiveIndividualAmounts(individualAmounts, intakeTransformer);

            var adr = DesignMatrixTools.GetDMAmount(transformedIndividualAmounts, CovariateModelType.CovariableCofactor, nPol);
            Assert.AreEqual(individualDailyIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight), adr.IndividualSamplingWeights.Sum(), 1e-3);
            Assert.HasCount(nPol + 1, adr.DesignMatrixDescriptions);
            Assert.HasCount(40, adr.Cofactors);
            Assert.HasCount(40, adr.Covariables);
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var individualIntakeAmounts = getIndividualIntakeAmounts(individualDailyIntakes, _foods, intakeTransformer, _rpfs, _membershipProbabilities);
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualIntakeAmounts, CovariateModelType.CovariableCofactor, adr, predictions);
            Assert.HasCount(nPol + 1, result.DesignMatrixDescriptions);
            Assert.HasCount(20, result.Cofactors);
            Assert.HasCount(20, result.Covariables);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualIntakeAmounts, CovariateModelType.CovariableCofactor, adr);
            Assert.HasCount(nPol + 1, result.DesignMatrixDescriptions);
            Assert.HasCount(20, result.Cofactors);
            Assert.HasCount(20, result.Covariables);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualIntakeAmounts, CovariateModelType.CovariableCofactor, adr);
            Assert.HasCount(nPol + 1, result.DesignMatrixDescriptions);
            Assert.HasCount(20, result.Cofactors);
            Assert.HasCount(20, result.Covariables);
        }

        /// <summary>
        /// Amounts, covariable and cofactor + interaction
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMAmountsListCovariableCofactorInt() {
            var individualDailyIntakes = mockIndividualDayIntakes();
            var individualIntakeAmountsCalculator = new SimpleIndividualDayIntakesCalculator(_substances, _rpfs, _membershipProbabilities, false, _foods);
            var individualAmounts = individualIntakeAmountsCalculator.ComputeIndividualAmounts(individualDailyIntakes);
            var intakeTransformer = new LogTransformer();
            var transformedIndividualAmounts = AmountsModelBase.ComputeTransformedPositiveIndividualAmounts(individualAmounts, intakeTransformer);
            var nPol = 3;

            var adr = DesignMatrixTools.GetDMAmount(transformedIndividualAmounts, CovariateModelType.CovariableCofactorInteraction, nPol);
            Assert.AreEqual(individualDailyIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight), adr.IndividualSamplingWeights.Sum(), 1e-3);
            Assert.HasCount(nPol * 2 + 1, adr.DesignMatrixDescriptions);
            Assert.HasCount(40, adr.Cofactors);
            Assert.HasCount(40, adr.Covariables);
            Assert.HasCount(nPol * 2 + 1, adr.DesignMatrixDescriptions);
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var individualIntakeAmounts = getIndividualIntakeAmounts(individualDailyIntakes, _foods, intakeTransformer, _rpfs, _membershipProbabilities);
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualIntakeAmounts, CovariateModelType.CovariableCofactorInteraction, adr, predictions);
            Assert.HasCount(nPol * 2 + 1, result.DesignMatrixDescriptions);
            Assert.HasCount(20, result.Cofactors);
            Assert.HasCount(20, result.Covariables);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualIntakeAmounts, CovariateModelType.CovariableCofactorInteraction, adr);
            Assert.HasCount(nPol * 2 + 1, result.DesignMatrixDescriptions);
            Assert.HasCount(20, result.Cofactors);
            Assert.HasCount(20, result.Covariables);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualIntakeAmounts, CovariateModelType.CovariableCofactorInteraction, adr);
            Assert.HasCount(nPol * 2 + 1, result.DesignMatrixDescriptions);
            Assert.HasCount(20, result.Cofactors);
            Assert.HasCount(20, result.Covariables);
        }

        /// <summary>
        /// Amounts, LNN, constant
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMAmountsLnnListConstant() {
            var individualDailyIntakes = mockIndividualDayIntakes();
            var nPol = 3;
            var indidualAmountsCalculator = new SimpleIndividualDayIntakesCalculator(_substances, _rpfs, _membershipProbabilities, false, null);
            var individualDayAmounts = indidualAmountsCalculator.Compute(individualDailyIntakes);
            var fdr = DesignMatrixTools.GetDMFrequencyLNN(individualDayAmounts, CovariateModelType.Constant, nPol);
            var adr = DesignMatrixTools.GetDMAmountLNN(individualDayAmounts, CovariateModelType.Constant, nPol);
            Assert.HasCount(1, fdr.DesignMatrixDescription);
            Assert.IsNull(fdr.Cofactor);
            Assert.IsNull(fdr.Covariable);
            Assert.HasCount(40, fdr.Weights);
            var resultFreq = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.Constant, fdr, null);
            Assert.IsNull(fdr.Cofactor);
            Assert.IsNull(resultFreq.Covariable);
            Assert.IsNull(resultFreq.Weights);
            resultFreq = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.Constant, fdr);
            Assert.IsNull(fdr.Cofactor);
            Assert.IsNull(resultFreq.Covariable);
            Assert.IsNull(resultFreq.Weights);
            var resultAmount = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.Constant, adr, null);
            Assert.IsNull(resultAmount.Cofactors);
            Assert.IsNull(resultAmount.Covariables);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            resultAmount = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.Constant, adr);
            Assert.IsNull(resultAmount.Cofactors);
            Assert.IsNull(resultAmount.Covariables);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
        }

        /// <summary>
        /// Amounts, LNN, cofactor
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMAmountsLnnListCofactor() {
            var individualDailyIntakes = mockIndividualDayIntakes();
            var nPol = 3;
            var indidualAmountsCalculator = new SimpleIndividualDayIntakesCalculator(_substances, _rpfs, _membershipProbabilities, false, null);
            var individualDayAmounts = indidualAmountsCalculator.Compute(individualDailyIntakes);
            var fdr = DesignMatrixTools.GetDMFrequencyLNN(individualDayAmounts, CovariateModelType.Cofactor, nPol);
            var adr = DesignMatrixTools.GetDMAmountLNN(individualDayAmounts, CovariateModelType.Cofactor, nPol);
            Assert.HasCount(2, fdr.DesignMatrixDescription);
            Assert.HasCount(40, fdr.Cofactor);
            Assert.IsNull(fdr.Covariable);
            Assert.HasCount(40, fdr.Weights);
            var resultFreq = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.Cofactor, fdr, null);
            Assert.HasCount(2, resultFreq.Cofactor);
            Assert.IsNull(resultFreq.Covariable);
            Assert.IsNull(resultFreq.Weights);
            Assert.HasCount(2, resultFreq.DesignMatrixDescription);
            resultFreq = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.Cofactor, fdr);
            Assert.HasCount(2, resultFreq.Cofactor);
            Assert.IsNull(resultFreq.Covariable);
            Assert.IsNull(resultFreq.Weights);
            Assert.HasCount(2, resultFreq.DesignMatrixDescription);
            var resultAmount = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.Cofactor, adr, null);
            Assert.HasCount(2, resultAmount.Cofactors);
            Assert.IsNull(resultAmount.Covariables);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.HasCount(2, resultAmount.DesignMatrixDescriptions);
            resultAmount = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.Cofactor, adr);
            Assert.HasCount(2, resultAmount.Cofactors);
            Assert.IsNull(resultAmount.Covariables);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.HasCount(2, resultAmount.DesignMatrixDescriptions);
        }

        /// <summary>
        /// Amounts, LNN, covariable
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMAmountsLnnListCovariable() {
            var individualDailyIntakes = mockIndividualDayIntakes();
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var nPol = 3;
            var indidualAmountsCalculator = new SimpleIndividualDayIntakesCalculator(_substances, _rpfs, _membershipProbabilities, false, null);
            var individualDayAmounts = indidualAmountsCalculator.Compute(individualDailyIntakes);
            var fdr = DesignMatrixTools.GetDMFrequencyLNN(individualDayAmounts, CovariateModelType.Covariable, nPol);
            var adr = DesignMatrixTools.GetDMAmountLNN(individualDayAmounts, CovariateModelType.Covariable, nPol);
            Assert.HasCount(nPol + 1, fdr.DesignMatrixDescription);
            Assert.IsNull(fdr.Cofactor);
            Assert.HasCount(40, fdr.Covariable);
            Assert.HasCount(40, fdr.Weights);
            var resultFreq = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.Covariable, fdr, predictions);
            Assert.IsNull(resultFreq.Cofactor);
            Assert.HasCount(10, resultFreq.Covariable);
            Assert.IsNull(resultFreq.Weights);
            Assert.HasCount(nPol + 1, resultFreq.DesignMatrixDescription);
            resultFreq = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.Covariable, fdr);
            Assert.IsNull(resultFreq.Cofactor);
            Assert.IsNull(resultFreq.Weights);
            Assert.HasCount(nPol + 1, resultFreq.DesignMatrixDescription);
            var resultAmount = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.Covariable, adr, predictions);
            Assert.IsNull(resultAmount.Cofactors);
            Assert.HasCount(10, resultAmount.Covariables);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.HasCount(nPol + 1, resultAmount.DesignMatrixDescriptions);
            resultAmount = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.Covariable, adr);
            Assert.IsNull(resultAmount.Cofactors);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.HasCount(nPol + 1, resultAmount.DesignMatrixDescriptions);
        }

        /// <summary>
        /// Amounts, LNN, covariable and cofactor
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMAmountsLnnListCovariableCofactor() {
            var individualDailyIntakes = mockIndividualDayIntakes();
            var indidualAmountsCalculator = new SimpleIndividualDayIntakesCalculator(_substances, _rpfs, _membershipProbabilities, false, null);
            var individualDayAmounts = indidualAmountsCalculator.Compute(individualDailyIntakes);
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var nPol = 3;
            var fdr = DesignMatrixTools.GetDMFrequencyLNN(individualDayAmounts, CovariateModelType.CovariableCofactor, nPol);
            var adr = DesignMatrixTools.GetDMAmountLNN(individualDayAmounts, CovariateModelType.CovariableCofactor, nPol);
            Assert.HasCount(nPol + 2, fdr.DesignMatrixDescription);
            Assert.HasCount(40, fdr.Cofactor);
            Assert.HasCount(40, fdr.Covariable);
            Assert.HasCount(40, fdr.Weights);
            var resultFreq = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactor, fdr, predictions);
            Assert.HasCount(20, resultFreq.Cofactor);
            Assert.HasCount(20, resultFreq.Covariable);
            Assert.IsNull(resultFreq.Weights);
            Assert.HasCount(nPol + 2, resultFreq.DesignMatrixDescription);
            resultFreq = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactor, fdr);
            Assert.HasCount(20, resultFreq.Cofactor);
            Assert.HasCount(20, resultFreq.Covariable);
            Assert.IsNull(resultFreq.Weights);
            Assert.HasCount(nPol + 2, resultFreq.DesignMatrixDescription);
            var resultAmount = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactor, adr, predictions);
            Assert.HasCount(20, resultAmount.Cofactors);
            Assert.HasCount(20, resultAmount.Covariables);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.HasCount(nPol + 2, resultAmount.DesignMatrixDescriptions);
            resultAmount = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactor, adr);
            Assert.HasCount(20, resultAmount.Cofactors);
            Assert.HasCount(20, resultAmount.Covariables);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.HasCount(nPol + 2, resultAmount.DesignMatrixDescriptions);
        }

        /// <summary>
        /// Amounts, LNN, covariable and cofactor + interaction
        /// </summary>
        [TestMethod()]
        public void DesignMatrixTools_TestGetDMAmountsLnnListCovariableCofactorInt() {
            var individualDailyIntakes = mockIndividualDayIntakes();
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var nPol = 3;
            var indidualAmountsCalculator = new SimpleIndividualDayIntakesCalculator(_substances, _rpfs, _membershipProbabilities, false, null);
            var individualDayAmounts = indidualAmountsCalculator.Compute(individualDailyIntakes);
            var fdr = DesignMatrixTools.GetDMFrequencyLNN(individualDayAmounts, CovariateModelType.CovariableCofactorInteraction, nPol);
            var adr = DesignMatrixTools.GetDMAmountLNN(individualDayAmounts, CovariateModelType.CovariableCofactorInteraction, nPol);
            Assert.HasCount(nPol * 2 + 2, fdr.DesignMatrixDescription);
            Assert.HasCount(40, fdr.Cofactor);
            Assert.HasCount(40, fdr.Covariable);
            Assert.HasCount(40, fdr.Weights);
            var resultFreq = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactorInteraction, fdr, predictions);
            Assert.HasCount(20, resultFreq.Cofactor);
            Assert.HasCount(20, resultFreq.Covariable);
            Assert.IsNull(resultFreq.Weights);
            Assert.HasCount(nPol * 2 + 2, resultFreq.DesignMatrixDescription);
            resultFreq = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactorInteraction, fdr);
            Assert.HasCount(20, resultFreq.Cofactor);
            Assert.HasCount(20, resultFreq.Covariable);
            Assert.IsNull(resultFreq.Weights);
            Assert.HasCount(nPol * 2 + 2, resultFreq.DesignMatrixDescription);
            var resultAmount = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactorInteraction, adr, predictions);
            Assert.HasCount(20, resultAmount.Cofactors);
            Assert.HasCount(20, resultAmount.Covariables);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.HasCount(nPol * 2 + 2, resultAmount.DesignMatrixDescriptions);
            resultAmount = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactorInteraction, adr);
            Assert.HasCount(20, resultAmount.Cofactors);
            Assert.HasCount(20, resultAmount.Covariables);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.HasCount(nPol * 2 + 2, resultAmount.DesignMatrixDescriptions);
        }
        private static IndividualFrequency mockSingleIndividualFrequency() {
            var sim = new SimulatedIndividual(new(1) { SamplingWeight = 0.5 }, 1);
            return new IndividualFrequency(sim) {
                Nbinomial = 6,
                Cofactor = "m",
                Covariable = 3,
                Frequency = 5,
                NumberOfIndividuals = 4,
            };
        }

        private List<DietaryIndividualDayIntake> mockIndividualDayIntakes() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            _substances = FakeSubstancesGenerator.Create(4);
            _foods = FakeFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            _rpfs = _substances.ToDictionary(r => r, r => 1d);
            _membershipProbabilities = _substances.ToDictionary(r => r, r => 1d);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            return FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, _foods, _substances, 0, true, random);
        }

        private static List<IndividualFrequency> mockIndividualFrequencies() {
            var result = new List<IndividualFrequency>();
            var cofactors = new[] { "m", "f" };
            for (int i = 0; i < 10; i++) {
                foreach (var cofactor in cofactors) {
                    var sim = new SimulatedIndividual(new(1) { SamplingWeight = 0.5 }, 1);
                    var record = new IndividualFrequency(sim) {
                        Nbinomial = 1,
                        Cofactor = cofactor,
                        Covariable = i,
                        Frequency = 1,
                        NumberOfIndividuals = 4,
                    };
                    result.Add(record);
                }
            }
            return result;
        }

        private List<ModelledIndividualAmount> getIndividualIntakeAmounts(
            List<DietaryIndividualDayIntake> individualDailyIntakes,
            ICollection<Food> foods,
            IntakeTransformer intakeTransformer,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return individualDailyIntakes
                .GroupBy(idi => idi.SimulatedIndividual)
                .Select(g => new ModelledIndividualAmount(g.Key) {
                    Cofactor = g.Key.Individual.Cofactor,
                    Covariable = g.Key.Individual.Covariable,
                    NumberOfPositiveIntakeDays = g.Count(idi => idi.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, false) > 0),
                    TransformedAmount = g.Sum(idi => intakeTransformer.Transform(idi.GetTotalDietaryIntakePerMassUnitPerCategory(foods, relativePotencyFactors, membershipProbabilities, false)
                        + idi.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities) / g.Count() / g.Key.BodyWeight))
                        / g.Count(idi => idi.GetTotalDietaryIntakePerMassUnitPerCategory(foods, relativePotencyFactors, membershipProbabilities, false) > 0 || idi.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities) > 0),
                }).ToList();
        }
    }
}
