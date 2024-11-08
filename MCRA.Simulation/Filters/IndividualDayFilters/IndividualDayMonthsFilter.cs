using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Filters.IndividualDayFilters {

    public sealed class IndividualDayMonthsFilter : IndividualDayFilterBase {

        /// <summary>
        /// The period ranges from which individual days should be included.
        /// </summary>
        public List<int> Months { get; set; }

        /// <summary>
        /// Initializes a new <see cref="IndividualDayMonthsFilter"/> instance.
        /// </summary>
        /// <param name="months"></param>
        /// <param name="includeUnspecifiedIndividualDayDates"></param>
        public IndividualDayMonthsFilter(
            IndividualProperty property,
            List<int> months,
            bool includeUnspecifiedIndividualDayDates
        ) : base(property, includeUnspecifiedIndividualDayDates) {
            Months = months;
        }

        /// <summary>
        /// Implements <see cref="IndividualDayFilterBase.Passes(IndividualDay)"/>.
        /// Returns true when the individual day is in one of the specified sampling periods.
        /// If no periods filters are specified, then all individual days should be included. 
        /// </summary>
        /// <param name="individualDay"></param>
        /// <returns></returns>
        public override bool Passes(IndividualDay individualDay) {
            if (Months?.Count > 0) {
                if (individualDay.Date == null) {
                    // If sampling date not specified, return default value for missing individual day dates.
                    return IncludeMissingValueRecords;
                } else {
                     // Return true if the month of sampling is one of the selected months.
                    return Months.Any(r => ((DateTime)individualDay.Date).Month == r);
                }
            }
            // If no month filters are specified, then all individual days should be included
            return true;
        }
    }
}
