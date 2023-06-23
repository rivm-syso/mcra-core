using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Assert.AreEqual(1 + nPol, fdr.DesignMatrixDescription.Count);
            Assert.IsNull(fdr.Cofactor);
            Assert.IsNull(fdr.Covariable);
            Assert.AreEqual(10, fdr.Weights.First());
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualFrequencies, CovariateModelType.Constant, fdr, null);
            Assert.AreEqual(1, result.DesignMatrixDescription.Count);
            Assert.AreEqual("constant", result.DesignMatrixDescription.First());
            Assert.IsNull(result.Cofactor);
            Assert.IsNull(result.Covariable);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualFrequencies, CovariateModelType.Constant, fdr);
            Assert.AreEqual(1, result.DesignMatrixDescription.Count);
            Assert.AreEqual("constant", result.DesignMatrixDescription.First());
            Assert.IsNull(result.Cofactor);
            Assert.IsNull(result.Covariable);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualFrequencies, CovariateModelType.Constant, fdr);
            Assert.AreEqual(1, result.DesignMatrixDescription.Count);
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
            Assert.AreEqual(2, fdr.DesignMatrixDescription.Count);
            Assert.AreEqual(2, fdr.Cofactor.Count);
            Assert.IsNull(fdr.Covariable);
            Assert.AreEqual(5, fdr.Weights.First());
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualFrequencies, CovariateModelType.Cofactor, fdr, null);
            Assert.AreEqual(2, result.DesignMatrixDescription.Count);
            Assert.AreEqual(2, result.Cofactor.Count);
            Assert.IsNull(result.Covariable);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualFrequencies, CovariateModelType.Cofactor, fdr);
            Assert.AreEqual(2, result.DesignMatrixDescription.Count);
            Assert.AreEqual(2, result.Cofactor.Count);
            Assert.IsNull(result.Covariable);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualFrequencies, CovariateModelType.Cofactor, fdr);
            Assert.AreEqual(2, result.DesignMatrixDescription.Count);
            Assert.AreEqual(20, result.Cofactor.Count);
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
            Assert.AreEqual(1 + nPol, fdr.DesignMatrixDescription.Count);
            Assert.IsNull(fdr.Cofactor);
            Assert.AreEqual(10, fdr.Covariable.Count);
            Assert.AreEqual(1, fdr.Weights.First());
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualFrequencies, CovariateModelType.Covariable, fdr, predictions);
            Assert.AreEqual(4, result.DesignMatrixDescription.Count);
            Assert.AreEqual(10, result.Covariable.Count);
            Assert.IsNull(result.Cofactor);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualFrequencies, CovariateModelType.Covariable, fdr);
            Assert.AreEqual(4, result.DesignMatrixDescription.Count);
            Assert.AreEqual(10, result.Covariable.Count);
            Assert.IsNull(result.Cofactor);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualFrequencies, CovariateModelType.Covariable, fdr);
            Assert.AreEqual(4, result.DesignMatrixDescription.Count);
            Assert.AreEqual(20, result.Covariable.Count);
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
            Assert.AreEqual(2 + nPol, fdr.DesignMatrixDescription.Count);
            Assert.AreEqual(20, fdr.Cofactor.Count);
            Assert.AreEqual(20, fdr.Covariable.Count);
            Assert.AreEqual(0.5, fdr.Weights.First());
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualFrequencies, CovariateModelType.CovariableCofactor, fdr, predictions);
            Assert.AreEqual(5, result.DesignMatrixDescription.Count);
            Assert.AreEqual(20, result.Covariable.Count);
            Assert.AreEqual(20, result.Cofactor.Count);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualFrequencies, CovariateModelType.CovariableCofactor, fdr);
            Assert.AreEqual(5, result.DesignMatrixDescription.Count);
            Assert.AreEqual(20, result.Covariable.Count);
            Assert.AreEqual(20, result.Cofactor.Count);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualFrequencies, CovariateModelType.CovariableCofactor, fdr);
            Assert.AreEqual(5, result.DesignMatrixDescription.Count);
            Assert.AreEqual(20, result.Covariable.Count);
            Assert.AreEqual(20, result.Cofactor.Count);
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
            Assert.AreEqual(8, fdr.DesignMatrixDescription.Count);
            Assert.AreEqual(20, fdr.Cofactor.Count);
            Assert.AreEqual(20, fdr.Covariable.Count);
            Assert.AreEqual(0.5, fdr.Weights.First());
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualFrequencies, CovariateModelType.CovariableCofactorInteraction, fdr, predictions);
            Assert.AreEqual(8, result.DesignMatrixDescription.Count);
            Assert.AreEqual(20, result.Covariable.Count);
            Assert.AreEqual(20, result.Cofactor.Count);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualFrequencies, CovariateModelType.CovariableCofactorInteraction, fdr);
            Assert.AreEqual(8, result.DesignMatrixDescription.Count);
            Assert.AreEqual(20, result.Covariable.Count);
            Assert.AreEqual(20, result.Cofactor.Count);
            Assert.IsNull(result.Weights);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualFrequencies, CovariateModelType.CovariableCofactorInteraction, fdr);
            Assert.AreEqual(8, result.DesignMatrixDescription.Count);
            Assert.AreEqual(20, result.Covariable.Count);
            Assert.AreEqual(20, result.Cofactor.Count);
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
            Assert.AreEqual(0, adr.DesignMatrixDescriptions.Count);
            var individualIntakeAmounts = getIndividualIntakeAmounts(individualDailyIntakes, foods, intakeTransformer, relativePotencyFactors, membershipProbabilities);
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualIntakeAmounts, CovariateModelType.Constant, adr, null);
            Assert.IsNull(result.DesignMatrixDescriptions);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualIntakeAmounts, CovariateModelType.Constant, adr);
            Assert.IsNull(result.DesignMatrixDescriptions);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualIntakeAmounts, CovariateModelType.Constant, adr);
            Assert.AreEqual(0, result.DesignMatrixDescriptions.Count);
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
            Assert.AreEqual(20, result.IndividualSamplingWeights.Count);
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
            Assert.AreEqual(individualDailyIntakes.Sum(c => c.Individual.SamplingWeight), adr.IndividualSamplingWeights.Sum(), 1e-3);
            Assert.AreEqual(1, adr.DesignMatrixDescriptions.Count);
            Assert.AreEqual(40, adr.Cofactors.Count);
            Assert.IsNull(adr.Covariables);
            var individualIntakeAmounts = getIndividualIntakeAmounts(individualDailyIntakes, _foods, intakeTransformer, _rpfs, _membershipProbabilities);
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualIntakeAmounts, CovariateModelType.Cofactor, adr, null);
            Assert.AreEqual(1, result.DesignMatrixDescriptions.Count);
            Assert.AreEqual(2, result.Cofactors.Count);
            Assert.IsNull(result.Covariables);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualIntakeAmounts, CovariateModelType.Cofactor, adr);
            Assert.AreEqual(1, result.DesignMatrixDescriptions.Count);
            Assert.AreEqual(2, result.Cofactors.Count);
            Assert.IsNull(result.Covariables);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualIntakeAmounts, CovariateModelType.Cofactor, adr);
            Assert.AreEqual(1, result.DesignMatrixDescriptions.Count);
            Assert.AreEqual(20, result.Cofactors.Count);
            Assert.IsNull(result.Covariables);
            Assert.AreEqual(20, result.IndividualSamplingWeights.Count);
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
            Assert.AreEqual(individualDailyIntakes.Sum(c => c.Individual.SamplingWeight), adr.IndividualSamplingWeights.Sum(), 1e-3);
            Assert.AreEqual(nPol, adr.DesignMatrixDescriptions.Count);
            Assert.IsNull(adr.Cofactors);
            Assert.AreEqual(40, adr.Covariables.Count);
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var individualIntakeAmounts = getIndividualIntakeAmounts(individualDailyIntakes, _foods, intakeTransformer, _rpfs, _membershipProbabilities);
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualIntakeAmounts, CovariateModelType.Covariable, adr, predictions);
            Assert.AreEqual(nPol, result.DesignMatrixDescriptions.Count);
            Assert.IsNull(result.Cofactors);
            Assert.AreEqual(10, result.Covariables.Count);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualIntakeAmounts, CovariateModelType.Covariable, adr);
            Assert.AreEqual(nPol, result.DesignMatrixDescriptions.Count);
            Assert.IsNull(result.Cofactors);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualIntakeAmounts, CovariateModelType.Covariable, adr);
            Assert.AreEqual(nPol, result.DesignMatrixDescriptions.Count);
            Assert.IsNull(result.Cofactors);
            Assert.AreEqual(20, result.Covariables.Count);
            Assert.AreEqual(20, result.IndividualSamplingWeights.Count);
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
            Assert.AreEqual(individualDailyIntakes.Sum(c => c.Individual.SamplingWeight), adr.IndividualSamplingWeights.Sum(), 1e-3);
            Assert.AreEqual(nPol + 1, adr.DesignMatrixDescriptions.Count);
            Assert.AreEqual(40, adr.Cofactors.Count);
            Assert.AreEqual(40, adr.Covariables.Count);
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var individualIntakeAmounts = getIndividualIntakeAmounts(individualDailyIntakes, _foods, intakeTransformer, _rpfs, _membershipProbabilities);
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualIntakeAmounts, CovariateModelType.CovariableCofactor, adr, predictions);
            Assert.AreEqual(nPol + 1, result.DesignMatrixDescriptions.Count);
            Assert.AreEqual(20, result.Cofactors.Count);
            Assert.AreEqual(20, result.Covariables.Count);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualIntakeAmounts, CovariateModelType.CovariableCofactor, adr);
            Assert.AreEqual(nPol + 1, result.DesignMatrixDescriptions.Count);
            Assert.AreEqual(20, result.Cofactors.Count);
            Assert.AreEqual(20, result.Covariables.Count);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualIntakeAmounts, CovariateModelType.CovariableCofactor, adr);
            Assert.AreEqual(nPol + 1, result.DesignMatrixDescriptions.Count);
            Assert.AreEqual(20, result.Cofactors.Count);
            Assert.AreEqual(20, result.Covariables.Count);
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
            Assert.AreEqual(individualDailyIntakes.Sum(c => c.Individual.SamplingWeight), adr.IndividualSamplingWeights.Sum(), 1e-3);
            Assert.AreEqual(nPol * 2 + 1, adr.DesignMatrixDescriptions.Count);
            Assert.AreEqual(40, adr.Cofactors.Count);
            Assert.AreEqual(40, adr.Covariables.Count);
            Assert.AreEqual(nPol * 2 + 1, adr.DesignMatrixDescriptions.Count);
            var predictions = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var individualIntakeAmounts = getIndividualIntakeAmounts(individualDailyIntakes, _foods, intakeTransformer, _rpfs, _membershipProbabilities);
            var result = DesignMatrixTools.GetDMSpecifiedPredictions(individualIntakeAmounts, CovariateModelType.CovariableCofactorInteraction, adr, predictions);
            Assert.AreEqual(nPol * 2 + 1, result.DesignMatrixDescriptions.Count);
            Assert.AreEqual(20, result.Cofactors.Count);
            Assert.AreEqual(20, result.Covariables.Count);
            result = DesignMatrixTools.GetDMIndividualPredictions(individualIntakeAmounts, CovariateModelType.CovariableCofactorInteraction, adr);
            Assert.AreEqual(nPol * 2 + 1, result.DesignMatrixDescriptions.Count);
            Assert.AreEqual(20, result.Cofactors.Count);
            Assert.AreEqual(20, result.Covariables.Count);
            result = DesignMatrixTools.GetDMConditionalPredictions(individualIntakeAmounts, CovariateModelType.CovariableCofactorInteraction, adr);
            Assert.AreEqual(nPol * 2 + 1, result.DesignMatrixDescriptions.Count);
            Assert.AreEqual(20, result.Cofactors.Count);
            Assert.AreEqual(20, result.Covariables.Count);
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
            Assert.AreEqual(1, fdr.DesignMatrixDescription.Count);
            Assert.IsNull(fdr.Cofactor);
            Assert.IsNull(fdr.Covariable);
            Assert.AreEqual(40, fdr.Weights.Count);
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
            Assert.AreEqual(2, fdr.DesignMatrixDescription.Count);
            Assert.AreEqual(40, fdr.Cofactor.Count);
            Assert.IsNull(fdr.Covariable);
            Assert.AreEqual(40, fdr.Weights.Count);
            var resultFreq = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.Cofactor, fdr, null);
            Assert.AreEqual(2, resultFreq.Cofactor.Count);
            Assert.IsNull(resultFreq.Covariable);
            Assert.IsNull(resultFreq.Weights);
            Assert.AreEqual(2, resultFreq.DesignMatrixDescription.Count);
            resultFreq = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.Cofactor, fdr);
            Assert.AreEqual(2, resultFreq.Cofactor.Count);
            Assert.IsNull(resultFreq.Covariable);
            Assert.IsNull(resultFreq.Weights);
            Assert.AreEqual(2, resultFreq.DesignMatrixDescription.Count);
            var resultAmount = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.Cofactor, adr, null);
            Assert.AreEqual(2, resultAmount.Cofactors.Count);
            Assert.IsNull(resultAmount.Covariables);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.AreEqual(2, resultAmount.DesignMatrixDescriptions.Count);
            resultAmount = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.Cofactor, adr);
            Assert.AreEqual(2, resultAmount.Cofactors.Count);
            Assert.IsNull(resultAmount.Covariables);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.AreEqual(2, resultAmount.DesignMatrixDescriptions.Count);
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
            Assert.AreEqual(nPol + 1, fdr.DesignMatrixDescription.Count);
            Assert.IsNull(fdr.Cofactor);
            Assert.AreEqual(40, fdr.Covariable.Count);
            Assert.AreEqual(40, fdr.Weights.Count);
            var resultFreq = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.Covariable, fdr, predictions);
            Assert.IsNull(resultFreq.Cofactor);
            Assert.AreEqual(10, resultFreq.Covariable.Count);
            Assert.IsNull(resultFreq.Weights);
            Assert.AreEqual(nPol + 1, resultFreq.DesignMatrixDescription.Count);
            resultFreq = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.Covariable, fdr);
            Assert.IsNull(resultFreq.Cofactor);
            Assert.IsNull(resultFreq.Weights);
            Assert.AreEqual(nPol + 1, resultFreq.DesignMatrixDescription.Count);
            var resultAmount = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.Covariable, adr, predictions);
            Assert.IsNull(resultAmount.Cofactors);
            Assert.AreEqual(10, resultAmount.Covariables.Count);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.AreEqual(nPol + 1, resultAmount.DesignMatrixDescriptions.Count);
            resultAmount = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.Covariable, adr);
            Assert.IsNull(resultAmount.Cofactors);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.AreEqual(nPol + 1, resultAmount.DesignMatrixDescriptions.Count);
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
            Assert.AreEqual(nPol + 2, fdr.DesignMatrixDescription.Count);
            Assert.AreEqual(40, fdr.Cofactor.Count);
            Assert.AreEqual(40, fdr.Covariable.Count);
            Assert.AreEqual(40, fdr.Weights.Count);
            var resultFreq = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactor, fdr, predictions);
            Assert.AreEqual(20, resultFreq.Cofactor.Count);
            Assert.AreEqual(20, resultFreq.Covariable.Count);
            Assert.IsNull(resultFreq.Weights);
            Assert.AreEqual(nPol + 2, resultFreq.DesignMatrixDescription.Count);
            resultFreq = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactor, fdr);
            Assert.AreEqual(20, resultFreq.Cofactor.Count);
            Assert.AreEqual(20, resultFreq.Covariable.Count);
            Assert.IsNull(resultFreq.Weights);
            Assert.AreEqual(nPol + 2, resultFreq.DesignMatrixDescription.Count);
            var resultAmount = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactor, adr, predictions);
            Assert.AreEqual(20, resultAmount.Cofactors.Count);
            Assert.AreEqual(20, resultAmount.Covariables.Count);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.AreEqual(nPol + 2, resultAmount.DesignMatrixDescriptions.Count);
            resultAmount = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactor, adr);
            Assert.AreEqual(20, resultAmount.Cofactors.Count);
            Assert.AreEqual(20, resultAmount.Covariables.Count);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.AreEqual(nPol + 2, resultAmount.DesignMatrixDescriptions.Count);
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
            Assert.AreEqual(nPol * 2 + 2, fdr.DesignMatrixDescription.Count);
            Assert.AreEqual(40, fdr.Cofactor.Count);
            Assert.AreEqual(40, fdr.Covariable.Count);
            Assert.AreEqual(40, fdr.Weights.Count);
            var resultFreq = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactorInteraction, fdr, predictions);
            Assert.AreEqual(20, resultFreq.Cofactor.Count);
            Assert.AreEqual(20, resultFreq.Covariable.Count);
            Assert.IsNull(resultFreq.Weights);
            Assert.AreEqual(nPol * 2 + 2, resultFreq.DesignMatrixDescription.Count);
            resultFreq = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactorInteraction, fdr);
            Assert.AreEqual(20, resultFreq.Cofactor.Count);
            Assert.AreEqual(20, resultFreq.Covariable.Count);
            Assert.IsNull(resultFreq.Weights);
            Assert.AreEqual(nPol * 2 + 2, resultFreq.DesignMatrixDescription.Count);
            var resultAmount = DesignMatrixTools.GetDMSpecifiedPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactorInteraction, adr, predictions);
            Assert.AreEqual(20, resultAmount.Cofactors.Count);
            Assert.AreEqual(20, resultAmount.Covariables.Count);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.AreEqual(nPol * 2 + 2, resultAmount.DesignMatrixDescriptions.Count);
            resultAmount = DesignMatrixTools.GetDMConditionalPredictionsLNN(individualDayAmounts, CovariateModelType.CovariableCofactorInteraction, adr);
            Assert.AreEqual(20, resultAmount.Cofactors.Count);
            Assert.AreEqual(20, resultAmount.Covariables.Count);
            Assert.IsNull(resultAmount.IndividualSamplingWeights);
            Assert.AreEqual(nPol * 2 + 2, resultAmount.DesignMatrixDescriptions.Count);
        }
        private static IndividualFrequency mockSingleIndividualFrequency() {
            return new IndividualFrequency() {
                SimulatedIndividualId = 1,
                Nbinomial = 6,
                Cofactor = "m",
                Covariable = 3,
                Frequency = 5,
                SamplingWeight = .5,
                NumberOfIndividuals = 4,
            };
        }

        private List<DietaryIndividualDayIntake> mockIndividualDayIntakes() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            _substances = MockSubstancesGenerator.Create(4);
            _foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            _rpfs = _substances.ToDictionary(r => r, r => 1d);
            _membershipProbabilities = _substances.ToDictionary(r => r, r => 1d);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random, properties);
            return MockDietaryIndividualDayIntakeGenerator.Create(individualDays, _foods, _substances, 0, true, random);
        }

        private static List<IndividualFrequency> mockIndividualFrequencies() {
            var result = new List<IndividualFrequency>();
            var cofactors = new[] { "m", "f" };
            for (int i = 0; i < 10; i++) {
                foreach (var cofactor in cofactors) {
                    var record = new IndividualFrequency() {
                        SimulatedIndividualId = 1,
                        Nbinomial = 1,
                        Cofactor = cofactor,
                        Covariable = i,
                        Frequency = 1,
                        SamplingWeight = .5,
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
                    .GroupBy(idi => idi.SimulatedIndividualId)
                    .Select(g => new ModelledIndividualAmount() {
                        SimulatedIndividualId = g.Key,
                        Cofactor = g.Select(idi => idi.Individual.Cofactor).First(),
                        Covariable = g.Select(idi => idi.Individual.Covariable).First(),
                        NumberOfPositiveIntakeDays = g.Count(idi => idi.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, false) > 0),
                        TransformedAmount = g.Sum(idi => intakeTransformer.Transform(idi.GetTotalDietaryIntakePerMassUnitPerCategory(foods, relativePotencyFactors, membershipProbabilities, false)
                            + idi.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities) / g.Count() / g.First().Individual.BodyWeight))
                            / g.Count(idi => idi.GetTotalDietaryIntakePerMassUnitPerCategory(foods, relativePotencyFactors, membershipProbabilities, false) > 0 || idi.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities) > 0),
                        IndividualSamplingWeight = g.First().IndividualSamplingWeight,
                    }).ToList();
        }
    }
}
