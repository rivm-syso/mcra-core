using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ExposureEvent;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation.PbkModelCalculation {

    [TestClass]
    public class ExposureEventsGeneratorTests {

        [TestMethod]
        [DataRow(TimeUnit.Hours)]
        [DataRow(TimeUnit.Days)]
        public void ExposureEventsGenerator_TestCreateAverageDaily(
            TimeUnit timeUnit
        ) {
            var random = new McraRandomGenerator(seed: 1);
            var routes = new[] { ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation };
            var substances = FakeSubstancesGenerator.Create(3);
            var externalExposures = createFakeIndividualDayExposures(random, routes, substances);

            var settings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 10,
                UseRepeatedDailyEvents = true
            };
            var generator = new ExposureEventsGenerator(
                settings,
                timeUnit,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                routes.ToDictionary(r => r, r => DoseUnit.ugPerDay)
            );
            var events = generator.CreateExposureEvents(
                externalExposures,
                routes,
                substances.First(),
                random
            );

            Assert.AreEqual(routes.Length, events.Count);
            CollectionAssert.AreEquivalent(routes, events.Select(r => r.Route).ToArray());
            Assert.IsTrue(events.All(e => e is RepeatingExposureEvent));
            var expectedInterval = timeUnit == TimeUnit.Hours ? 24 : 1;
            Assert.IsTrue(events.Cast<RepeatingExposureEvent>().All(e => e.Interval == expectedInterval));
        }

        [TestMethod]
        [DataRow(TimeUnit.Hours, 1)]
        [DataRow(TimeUnit.Days, 1)]
        [DataRow(TimeUnit.Hours, 2)]
        [DataRow(TimeUnit.Days, 2)]
        public void ExposureEventsGenerator_TestCreateRandomDaily(
            TimeUnit timeUnit,
            int oralDosesPerDay
        ) {
            var random = new McraRandomGenerator(seed: 1);
            var routes = new [] { ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation };
            var substances = FakeSubstancesGenerator.Create(3);
            var externalExposures = createFakeIndividualDayExposures(random, routes, substances);

            var settings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 10,
                UseRepeatedDailyEvents = false,
                NumberOfOralDosesPerDay = oralDosesPerDay,
                NumberOfDermalDosesPerDay = 3,
                NumberOfInhalationDosesPerDay = 4,
            };
            var generator = new ExposureEventsGenerator(
                settings,
                timeUnit,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                routes.ToDictionary(r => r, r => DoseUnit.ugPerDay)
            );
            var events = generator.CreateExposureEvents(
                externalExposures,
                routes,
                substances.First(),
                random
            );

            Assert.IsTrue(events.All(e => e is SingleExposureEvent));
            Assert.AreEqual(
                settings.NumberOfOralDosesPerDay * settings.NumberOfSimulatedDays,
                events.Count(r => r.Route == ExposureRoute.Oral)
            );
            Assert.AreEqual(
                settings.NumberOfDermalDosesPerDay * settings.NumberOfSimulatedDays,
                events.Count(r => r.Route == ExposureRoute.Dermal)
            );
            Assert.AreEqual(
                settings.NumberOfInhalationDosesPerDay * settings.NumberOfSimulatedDays,
                events.Count(r => r.Route == ExposureRoute.Inhalation)
            );
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void ExposureEventsGenerator_TestCreateRandomDaily_Oral(
            bool specifyEvents
        ) {
            var random = new McraRandomGenerator(seed: 1);
            var routes = new[] { ExposureRoute.Oral };
            var substances = FakeSubstancesGenerator.Create(3);
            var externalExposures = createFakeIndividualDayExposures(random, routes, substances);

            var settings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 10,
                UseRepeatedDailyEvents = false,
                SpecifyEvents = specifyEvents,
                SelectedEvents = [0, 8, 16],
                NumberOfOralDosesPerDay = 6
            };
            var generator = new ExposureEventsGenerator(
                settings,
                TimeUnit.Hours,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                routes.ToDictionary(r => r, r => DoseUnit.ugPerDay)
            );
            var events = generator.CreateExposureEvents(
                externalExposures,
                routes,
                substances.First(),
                random
            );

            var expectedEventsCount = specifyEvents
                ? settings.SelectedEvents.Length * settings.NumberOfSimulatedDays
                : settings.NumberOfOralDosesPerDay * settings.NumberOfSimulatedDays;
            Assert.AreEqual(expectedEventsCount, events.Count);
            Assert.IsTrue(events.All(r => r.Route == ExposureRoute.Oral));
            Assert.IsTrue(events.All(e => e is SingleExposureEvent));
        }

        [TestMethod]
        [DataRow(true, true)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(false, false)]
        public void ExposureEventsGenerator_TestCreatePerBwDoses(
            bool bodyWeightCorrected,
            bool useRepeatedDailyEvents
        ) {
            var random = new McraRandomGenerator(seed: 1);
            var routes = new[] { ExposureRoute.Oral };
            var substance = FakeSubstancesGenerator.Create(1).Single();
            var externalExposures = createFakeIndividualDayExposures(random, routes, [substance], nDays: 1);

            var settings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 10,
                UseRepeatedDailyEvents = useRepeatedDailyEvents,
                BodyWeightCorrected = bodyWeightCorrected
            };
            var generator = new ExposureEventsGenerator(
                settings,
                TimeUnit.Hours,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                routes.ToDictionary(r => r, r => DoseUnit.ugPerDay)
            );
            var events = generator.CreateExposureEvents(
                externalExposures,
                routes,
                substance,
                random
            );

            var exp = externalExposures.Single().GetExposure(substance, !bodyWeightCorrected);
            Assert.IsTrue(events.All(r => r.Value == exp));
        }

        private static List<IExternalIndividualDayExposure> createFakeIndividualDayExposures(
            IRandom random,
            ExposureRoute[] routes,
            List<Compound> substances,
            int nDays = 2
        ) {
            var paths = FakeExposurePathGenerator.Create(routes);
            var individuals = FakeIndividualsGenerator.Create(1, nDays, random);
            var individualDays = FakeIndividualDaysGenerator
                .CreateSimulatedIndividualDays(individuals);
            var externalExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, paths, seed: 1);
            return externalExposures;
        }
    }
}
