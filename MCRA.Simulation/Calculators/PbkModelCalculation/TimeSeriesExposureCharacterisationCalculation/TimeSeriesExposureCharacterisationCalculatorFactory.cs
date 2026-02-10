using MCRA.General;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.TargetExposureFromTimeSeriesCalculation {
    public class TimeSeriesExposureCharacterisationCalculatorFactory {
        public static ITimeSeriesExposureCharacterisationCalculator Create(
            ExposureType exposureType,
            int nonStationaryPeriod
        ) {
            switch (exposureType) {
                case ExposureType.Acute:
                    return new TimeSeriesAverageDailyPeakExposureCalculator(nonStationaryPeriod);
                case ExposureType.Chronic:
                    return new TimeSeriesSteadyStateExposureCalculator(nonStationaryPeriod);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
