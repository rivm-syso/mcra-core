using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalExposureCalculation;
using MCRA.Simulation.Calculators.OccupationalTaskModelCalculation;

namespace MCRA.Simulation.Calculators.OccupationalScenarioExposureCalculation {
    public sealed class OccupationalScenarioTaskExposure {

        public OccupationalScenarioTask ScenarioTask { get; init; }

        public OccupationalScenario Scenario { get; init; }

        public OccupationalTask Task => ScenarioTask.OccupationalTask;

        public OccupationalTaskDeterminants Determinants => ScenarioTask.Determinants();

        public Compound Substance { get; init; }

        public ExposureRoute Route { get; init; }

        public double Duration => ScenarioTask.Duration;

        public double Frequency => ScenarioTask.Frequency;

        public FrequencyResolutionType FrequencyResolution => ScenarioTask.FrequencyResolution;

        public OccupationalExposureUnit Unit { get; init; }

        public double Value { get; init; }

    }
}


