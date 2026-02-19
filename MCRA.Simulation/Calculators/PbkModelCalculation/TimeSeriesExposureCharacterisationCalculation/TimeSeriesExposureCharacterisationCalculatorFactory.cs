using MCRA.General;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.TargetExposureFromTimeSeriesCalculation {
    public class TimeSeriesExposureCharacterisationCalculatorFactory {
        public static ITimeSeriesExposureCharacterisationCalculator Create(
            ExposureType exposureType,
            PbkSimulationSettings simulationSettings
        ) {
            switch (exposureType) {
                case ExposureType.Acute:
                    return new TimeSeriesAverageDailyPeakExposureCalculator(simulationSettings.NonStationaryPeriod);
                case ExposureType.Chronic:
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
