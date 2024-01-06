namespace MCRA.General {
    public static class TimeUnitExtensions {

        /// <summary>
        /// Gets multiplier to convert to the the target time unit.
        /// </summary>
        /// <param name="resolutionType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static double GetTimeUnitMultiplier(this TimeUnit resolutionType, TimeUnit targetTimeUnit) {
            switch ((resolutionType, targetTimeUnit)) {
                case (TimeUnit.Days, TimeUnit.Days):
                    return 1;
                case (TimeUnit.Hours, TimeUnit.Days):
                    return 1D / 24D;
                case (TimeUnit.Minutes, TimeUnit.Days):
                    return 1D / 1440;
                case (TimeUnit.Seconds, TimeUnit.Days):
                    return 1D / (24 * 60 * 60);
                case (TimeUnit.Days, TimeUnit.Hours):
                    return 24;
                case (TimeUnit.Hours, TimeUnit.Hours):
                    return 1;
                case (TimeUnit.Minutes, TimeUnit.Hours):
                    return 1D / 60;
                case (TimeUnit.Seconds, TimeUnit.Hours):
                    return 1D / 3600;
                case (TimeUnit.Days, TimeUnit.Minutes):
                    return 24 * 60;
                case (TimeUnit.Hours, TimeUnit.Minutes):
                    return 60;
                case (TimeUnit.Minutes, TimeUnit.Minutes):
                    return 1;
                case (TimeUnit.Seconds, TimeUnit.Minutes):
                    return 1D / 60;
                case (TimeUnit.Days, TimeUnit.Seconds):
                    return 24 * 60 * 60;
                case (TimeUnit.Hours, TimeUnit.Seconds):
                    return 60 * 60;
                case (TimeUnit.Minutes, TimeUnit.Seconds):
                    return 60;
                case (TimeUnit.Seconds, TimeUnit.Seconds):
                    return 1;
                default:
                    throw new Exception($"No time unit multiplier for {resolutionType}.");
            }
        }
    }
}
