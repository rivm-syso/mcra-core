﻿using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.NonDietaryExposures {

    /// <summary>
    /// OutputGeneration, ActionSummaries, NonDietaryExposures, NonDietaryExposuresDrillDown
    /// </summary>
    [TestClass]
    public class NonDietaryTests : SectionTestBase {
        /// <summary>
        /// Summarize and test NonDietaryDrillDownSection view
        /// </summary>
        [TestMethod]
        public void NonDietaryDrillDownTest() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var nonDietaryExposureRoutes = new HashSet<ExposurePathType>() { ExposurePathType.Dermal, ExposurePathType.Oral };
            var individuals = FakeIndividualsGenerator.Create(100, 2, random);
            var substances = FakeSubstancesGenerator.Create(4);
            var nonDietarySurveys = FakeNonDietaryExposureSetsGenerator.MockNonDietarySurveys(individuals, substances, nonDietaryExposureRoutes, random);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var kineticConversionFactors = FakeKineticModelsGenerator.CreateAbsorptionFactors(substances, .1);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var targetUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);

            var calculator = new NonDietaryUnmatchedCorrelatedExposureGenerator();
            calculator.Initialize(nonDietarySurveys, targetUnit);
            var nonDietaryIntakes = calculator.GenerateAcuteNonDietaryIntakes(
                individualDays.Cast<IIndividualDay>().ToList(),
                substances,
                nonDietarySurveys.Keys,
                123456,
                new CancellationToken());

            var section = new NonDietaryDrillDownSection();
            var ids = section.GetDrillDownRecords(99, nonDietaryIntakes, rpfs, memberships, kineticConversionFactors, false);
            section.Summarize(nonDietaryIntakes, ids, nonDietaryExposureRoutes, rpfs, memberships, kineticConversionFactors, substances.First(), false);

            Assert.IsNotNull(section.DrillDownSummaryRecords);
            AssertIsValidView(section);
        }
    }
}
