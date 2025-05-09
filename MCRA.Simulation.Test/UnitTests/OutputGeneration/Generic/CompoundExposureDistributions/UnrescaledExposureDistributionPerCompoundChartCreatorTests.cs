﻿using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.DietaryExposureImputationCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration {

    /// <summary>
    /// OutputGeneration, Generic, CompoundExposureDistributions
    /// </summary>
    [TestClass]
    public class UnrescaledExposureDistributionPerCompoundChartCreatorTests {

        /// <summary>
        /// UnrescaledExposureDistributionPerCompoundChart_TestUncertainty
        /// </summary>
        [TestMethod]
        public void UnrescaledExposureDistributionPerCompoundChart_TestUncertainty() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(100, 2, true, random, null);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);

            var exposurePerCompoundRecords = new Dictionary<Compound, List<ExposureRecord>>();
            foreach (var substance in substances) {
                var exposureRecords = new List<ExposureRecord>();
                foreach (var day in individualDays) {
                    exposureRecords.Add(new ExposureRecord() {
                        BodyWeight = day.SimulatedIndividual.BodyWeight,
                        SamplingWeight = day.SimulatedIndividual.SamplingWeight,
                        IndividualDayId = day.SimulatedIndividualDayId,
                        Exposure = NormalDistribution.Draw(random, 20, 1),
                    });
                }
                exposurePerCompoundRecords[substance] = exposureRecords;
            }
            var section = new CompoundExposureDistributionsSection();

            section.SummarizeExposureDistributionPerCompound(exposurePerCompoundRecords, rpfs, memberships, false);

            var chart = new UnrescaledExposureDistributionPerCompoundChartCreator(section.CompoundExposureDistributionRecords.First(), 500, 300, "mg/kg");
            chart.CreateToSvg(TestUtilities.ConcatWithOutputPath("UnrescaledExposureDistributionPerCompoundChartCreator.svg"));
        }
    }
}
