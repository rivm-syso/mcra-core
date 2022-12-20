using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    ///  OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, ModelIndividual
    /// </summary>
    [TestClass]
    public class ChronicConditionalSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize, test ChronicDietaryConditionalSection view
        /// </summary>
        [TestMethod]
        public void ChronicConditionalSection_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);

            var conditionalIntakes = new List<ConditionalUsualIntake>();
            var conditionalIntake = new ConditionalUsualIntake() {
                ConditionalUsualIntakes = Enumerable
                    .Range(0, 100)
                    .Select(r => LogNormalDistribution.InvCDF(-2.2, .3, random.NextDouble(0, 1)))
                    .ToList(),
                CovariateGroup = new CovariateGroup() { Cofactor = "male", Covariable = 10, NumberOfIndividuals = 100, GroupSamplingWeight = 100 },
                CovariatesCollection = new CovariatesCollection() { AmountCofactor = "male", AmountCovariable = 10, FrequencyCofactor ="male", FrequencyCovariable = 10, CofactorName = "gender", CovariableName="age"}
            };
            conditionalIntakes.Add(conditionalIntake);
            var section = new ChronicConditionalSection();
            var header = new SectionHeader();
            section.Summarize(
                header,
                new IndividualProperty() { Name = "Gender", },
                new IndividualProperty() { Name = "Age", },
                substances.First(),
                conditionalIntakes,
                new double[] { 50, 90, 95 },
                ExposureMethod.Automatic,
                new double[] { 005, .1, });
            var subHeader = header.GetSubSectionHeader<ConditionalIntakePercentileSection>();
            var percentileSection = subHeader.GetSummarySection() as ConditionalIntakePercentileSection;

            Assert.AreEqual(0.108, percentileSection.ConditionalIntakePercentileSections.First().IntakePercentileSection.Percentiles[0].ReferenceValue, 1e-3);
            Assert.AreEqual(0.116, percentileSection.ConditionalIntakePercentileSections.First().IntakePercentileSection.MeanOfExposure.First().ReferenceValue, 1e-3);
            section.SummarizeUncertainty(
                header,
                conditionalIntakes,
                5,
                95
            );
            //AssertIsValidView(section);
        }
    }
}