using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation.PbkModelCalculation {

    [TestClass]
    public class PbkModelCalculatorBaseTests : PbkModelCalculatorBase {

        public PbkModelCalculatorBaseTests()
            : base(
                  new KineticModelInstance() {
                      KineticModelDefinition = new KineticModelDefinition()
                  },
                  new PbkSimulationSettings()
            )
        {
        }

        protected override Dictionary<int, List<SubstanceTargetExposurePattern>> calculate(IDictionary<int, List<IExternalIndividualDayExposure>> externalIndividualExposures, ExposureUnitTriple externalExposureUnit, ICollection<ExposureRoute> selectedExposureRoutes, ICollection<TargetUnit> targetUnits, ExposureType exposureType, bool isNominal, IRandom generator, ProgressState progressState) {
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
            KineticModelDefinition.EvaluationFrequency = 1;
            KineticModelDefinition.Resolution = modelTimeResolution;
            SimulationSettings.OutputResolutionTimeUnit = outputTimeUnit;
            SimulationSettings.OutputResolutionStepSize = stepSize;
            Assert.AreEqual(expected, getSimulationStepsPerDay());
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
