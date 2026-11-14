using MCRA.General;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.TargetExposureFromTimeSeriesCalculation {
    public class TimeSeriesExposureCharacterisationCalculatorFactory {
        public static ITimeSeriesExposureCharacterisationCalculator Create(
            ExposureType exposureType,
            PbkSimulationSettings simulationSettings
        ) {
            switch (exposureType) {
                case ExposureType.Acute:
                    if (simulationSettings.NumberOfSimulatedDays < simulationSettings.NonStationaryPeriod) {
                        throw new Exception("Invalid PBK simulation settings: non-stationary period cannot exceed the simulation duration.");
                    }
                    return new TimeSeriesAverageDailyPeakExposureCalculator(
                        simulationSettings.NonStationaryPeriod
                    );
                case ExposureType.Chronic:
                    if (!simulationSettings.IsLifetime() && simulationSettings.NumberOfSimulatedDays < simulationSettings.NonStationaryPeriod) {
                        throw new Exception("Invalid PBK simulation settings: non-stationary period cannot exceed the simulation duration.");
                    }
                    return new TimeSeriesSteadyStateExposureCalculator(
                        simulationSettings.IsLifetime(),
                        simulationSettings.NonStationaryPeriod,
                        simulationSettings.LifetimeStationaryPeriod
                    );
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
