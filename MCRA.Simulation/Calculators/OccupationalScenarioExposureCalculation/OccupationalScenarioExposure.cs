using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalTaskModelCalculation;

namespace MCRA.Simulation.Calculators.OccupationalScenarioExposureCalculation {

    public sealed class OccupationalScenarioExposure {

        public OccupationalScenario Scenario { get; init; }

        public ExposureRoute Route { get; init; }

        public Compound Substance { get; init; }

        public OccupationalExposureUnit Unit { get; init; }

        public double Value { get; init; }

        public List<OccupationalScenarioTaskExposure> TaskExposures { get; set; }

    }
}
