using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.DeSolve;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.PbkModelCalculation {

    [TestClass]
    public class PbkModelCalculatorBaseTests : PbkModelCalculatorBase<DeSolvePbkModelSpecification> {

        public PbkModelCalculatorBaseTests()
            : base(
                  new KineticModelInstance() {
                      KineticModelDefinition = new DeSolvePbkModelSpecification() {
                          Parameters = []
                      }
                  },
                  new PbkSimulationSettings()
            )
        {
        }

        public override List<PbkSimulationOutput> calculate(
            ICollection<(SimulatedIndividual Individual, List<IExternalIndividualDayExposure> IndividualDayExposures)> externalIndividualExposures,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposureRoute> routes,
            ICollection<TargetUnit> targetUnits,
            IRandom generator) {
            throw new NotImplementedException();
        }

        [TestMethod]
        [DataRow(TimeUnit.Hours, PbkModelOutputResolutionTimeUnit.ModelTimeUnit, 99999, 24)]
        [DataRow(TimeUnit.Days, PbkModelOutputResolutionTimeUnit.ModelTimeUnit, 99999, 1)]
        [DataRow(TimeUnit.Hours, PbkModelOutputResolutionTimeUnit.Minutes, 10, 144)] // 6 x 24
        [DataRow(TimeUnit.Days, PbkModelOutputResolutionTimeUnit.Minutes, 10, 144)] // 6 x 24
        [DataRow(TimeUnit.Hours, PbkModelOutputResolutionTimeUnit.Hours, 6, 4)]
        [DataRow(TimeUnit.Hours, PbkModelOutputResolutionTimeUnit.Hours, 12, 2)]
        [DataRow(TimeUnit.Hours, PbkModelOutputResolutionTimeUnit.Hours, 24, 1)]
        [DataRow(TimeUnit.Days, PbkModelOutputResolutionTimeUnit.Days, 1, 1)]
        [DataRow(TimeUnit.Days, PbkModelOutputResolutionTimeUnit.Days, 2, 0.5)]
        public void PbkModelCalculatorBase_TestGetSimulationStepsPerDay(
            TimeUnit modelTimeResolution,
            PbkModelOutputResolutionTimeUnit outputTimeUnit,
            int stepSize,
            double expected
        ) {
            PbkModelSpecification.EvaluationFrequency = 1;
            PbkModelSpecification.Resolution = modelTimeResolution;
            SimulationSettings.OutputResolutionTimeUnit = outputTimeUnit;
            SimulationSettings.OutputResolutionStepSize = stepSize;
            var actual = getSimulationStepsPerDay();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(PbkSimulationMethod.Standard, 10, null, null, 10D)]
        [DataRow(PbkSimulationMethod.LifetimeToCurrentAge, 10, 4D, null, 1461D)] // 4 x 365.25 = 1461
        [DataRow(PbkSimulationMethod.LifetimeToSpecifiedAge, 10, null, 4, 1461D)] // 4 x 365.25 = 1461
        public void PbkModelCalculatorBase_TestGetSimulationDuration(
            PbkSimulationMethod pbkSimulationMethod,
            int numberOfDays,
            double? currentAge,
            int? specifiedAge,
            double expected
        ) {
            SimulationSettings.PbkSimulationMethod = pbkSimulationMethod;
            SimulationSettings.NumberOfSimulatedDays = numberOfDays;
            SimulationSettings.LifetimeYears = specifiedAge ?? 0;
            var duration = getSimulationDuration(currentAge);
            Assert.AreEqual(expected, duration);
        }
    }
}
