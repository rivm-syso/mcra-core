using MCRA.Utils.Sbml.Objects;

namespace MCRA.General {
    public static class TimeUnitExtensions {

        /// <summary>
        /// Gets multiplier to convert to the the target time unit.
        /// </summary>
        /// <param name="resolutionType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static double GetTimeUnitMultiplier(this TimeUnit resolutionType, TimeUnit targetTimeUnit) {
            return (resolutionType, targetTimeUnit) switch {
                (TimeUnit.Days, TimeUnit.Days) => 1,
                (TimeUnit.Hours, TimeUnit.Days) => 1D / 24D,
                (TimeUnit.Minutes, TimeUnit.Days) => 1D / 1440,
                (TimeUnit.Seconds, TimeUnit.Days) => 1D / (24 * 60 * 60),
                (TimeUnit.Days, TimeUnit.Hours) => 24,
                (TimeUnit.Hours, TimeUnit.Hours) => 1,
                (TimeUnit.Minutes, TimeUnit.Hours) => 1D / 60,
                (TimeUnit.Seconds, TimeUnit.Hours) => 1D / 3600,
                (TimeUnit.Days, TimeUnit.Minutes) => 24 * 60,
                (TimeUnit.Hours, TimeUnit.Minutes) => 60,
                (TimeUnit.Minutes, TimeUnit.Minutes) => 1,
                (TimeUnit.Seconds, TimeUnit.Minutes) => 1D / 60,
                (TimeUnit.Days, TimeUnit.Seconds) => 24 * 60 * 60,
                (TimeUnit.Hours, TimeUnit.Seconds) => 60 * 60,
                (TimeUnit.Minutes, TimeUnit.Seconds) => 60,
                (TimeUnit.Seconds, TimeUnit.Seconds) => 1,
                _ => throw new Exception($"No time unit multiplier for {resolutionType}."),
            };
        }

        /// <summary>
        /// Creates a <see cref="TimeUnit"/> from a <see cref="SbmlTimeUnit"/>.
        /// </summary>
        /// <param name="sbmlTimeUnit"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static TimeUnit ToTimeUnit(this SbmlTimeUnit sbmlTimeUnit) {
            switch (sbmlTimeUnit) {
                case SbmlTimeUnit.NotSpecified:
                    return TimeUnit.NotSpecified;
                case SbmlTimeUnit.Seconds:
                    return TimeUnit.Seconds;
                case SbmlTimeUnit.Minutes:
                    return TimeUnit.Minutes;
                case SbmlTimeUnit.Hours:
                    return TimeUnit.Hours;
                case SbmlTimeUnit.Days:
                    return TimeUnit.Days;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
