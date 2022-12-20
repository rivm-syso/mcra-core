using MCRA.Utils.DateTimes;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Filters.IndividualDayFilters {
    public sealed class IndividualDayPeriodFilter : IndividualDayFilterBase {

        /// <summary>
        /// The period ranges from which the individual day should be included.
        /// </summary>
        public TimeRange Period { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="period"></param>
        /// <param name="includeUnspecifiedIndividualDayDates"></param>
        public IndividualDayPeriodFilter(
            IndividualProperty property,
            TimeRange period,
            bool includeUnspecifiedIndividualDayDates
        ) : base(property, includeUnspecifiedIndividualDayDates) {
            Period = period;
        }

        /// <summary>
        /// Returns true when the individual day is in one of the specified individual day periods.
        /// If no periods filters are specified, then all individual days should be included. 
        /// </summary>
        /// <param name="individualDay"></param>
        /// <returns></returns>
        public override bool Passes(IndividualDay individualDay) {
            if (Period != null) {
                if (individualDay.Date == null) {
                    // If individual day date not specified, return default value for missing individual day dates.
                    return IncludeMissingValueRecords;
                } else {
                    // Return true if the individual day date matches any of the period definitions.
                    return individualDay.Date >= Period.StartDate && individualDay.Date <= Period.EndDate;
                }
            }
            // If no periods filters are specified, then all individual days should be included
            return true;
        }
    }
}
