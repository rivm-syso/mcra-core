using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;


namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.Generic.ExternalExposures.ExposuresByRoute {

    [TestClass]
    public class ExternalExposuresByRouteSectionTests : SectionTestBase {

        [TestMethod]
        public void ExternalExposuresByRouteSection_SummarizeChronicTest() {

            var seed = 1;
            var random = new McraRandomGenerator(seed);

            // create simulated individuals days
            var individuals = FakeIndividualsGenerator.Create(10, 2, random);
            var simulatedIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);

            // common routes and exposure paths
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var paths = FakeExposurePathGenerator.Create(routes);

            // create substances sets for different combinations
            var substancesOne = FakeSubstancesGenerator.Create(1);
            var substancesThree = FakeSubstancesGenerator.Create(3);

            ExposureUnitTriple unit = ExposureUnitTriple.CreateDefaultExposureUnit(
                ExposureTarget.DietaryExposureTarget,
                ExposureType.Chronic
            );

            // create external individual exposures for the individuals and substances
            var exposuresForThree = FakeExternalExposureGenerator.CreateExternalIndividualExposures(
                simulatedIndividualDays,
                substancesThree,
                paths,
                seed,
                percentageZeros: 20
            );

            var exposuresForOne = FakeExternalExposureGenerator.CreateExternalIndividualExposures(
                simulatedIndividualDays,
                substancesOne,
                paths,
                seed + 1,
                percentageZeros: 20
            );

            var emptyExposures = new List<IExternalIndividualExposure>();

            // NOTE: we must instantiate a production class here because the creation of the corresponding view
            //       uses reflection and can only find the type in the Simulation assembly, and not in this test assembly
            var section = new DustExposuresByRouteSection();

            // 1) Typical case: multiple substances, explicit RPFs and memberships
            section.Summarize(
                exposuresForThree,
                routes,
                substancesThree,
                substancesThree.ToDictionary(s => s, s => random.NextDouble(0.1, 2.0)),
                substancesThree.ToDictionary(s => s, s => random.NextDouble(1, 3)),
                25,
                75,
                2.5,
                97.5,
                [50, 75, 90, 95],
                isPerPerson: false,
                exposureUnit: unit
            );
            AssertIsValidView(section);

            // 2) Single substance, rely on default RPFs/memberships by passing null
            section.Summarize(
                exposuresForOne,
                routes,
                substancesOne,
                rpfs: null,
                memberships: null,
                  lowerPercentage: 25,
                upperPercentage: 75,
                uncertaintyLowerBound: 2.5,
                uncertaintyUpperBound: 97.5,
                percentages: [50, 75, 90, 95],
                isPerPerson: false,
                exposureUnit: unit
            );
            AssertIsValidView(section);

            // 3) No exposures (empty list) -> should not throw and view can be validated (no records expected)
            section.Summarize(
                emptyExposures,
                routes,
                substancesThree,
                rpfs: null,
                memberships: null,
                 lowerPercentage: 25,
                upperPercentage: 75,
                uncertaintyLowerBound: 2.5,
                uncertaintyUpperBound: 97.5,
                percentages: [50, 75, 90, 95],
                isPerPerson: false,
                exposureUnit: unit
            );
            AssertIsValidView(section);

            // 4) Multiple substances but only a subset present in exposures - exercise filtering by substances
            var subsetSubstances = new List<Compound> { substancesThree.First() };
            section.Summarize(
                exposuresForThree,
                routes,
                subsetSubstances,
                subsetSubstances.ToDictionary(s => s, s => 1d),
                subsetSubstances.ToDictionary(s => s, s => 1d),
                25,
                75,
                2.5,
                97.5,
                [50, 75, 90, 95],
                isPerPerson: false,
                exposureUnit: unit
            );
            AssertIsValidView(section);
        }
    }
}