using MCRA.Utils.ExtensionMethods;

namespace MCRA.Utils.DateTimes {

    /// <summary>
    /// Represents a TimeSpan with a fixed start and enddate.
    /// </summary>
    public sealed class TimeRange {

        /// <summary>
        /// The start of the time range.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end of the time range.
        /// </summary>
        public DateTime EndDate { get; set; }

        public TimeRange() { }

        public TimeRange(int year) {
            StartDate = new DateTime(year, 1, 1);
            EndDate = new DateTime(year, 12, 31);
        }

        public TimeRange(DateTime startDate, DateTime endDate) {
            if (startDate < endDate) {
                StartDate = startDate;
                EndDate = endDate;
            } else {
                throw new ArgumentException("End date must be later than start date.");
            }
        }

        /// <summary>
        /// True if the provided DateTime lies in this time range.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public bool Contains(DateTime dateTime) {
            return dateTime.IsBetween(StartDate, EndDate);
        }

        public bool Contains(TimeRange timeRange) {
            return Contains(timeRange.StartDate) && Contains(timeRange.EndDate);
        }

        public bool OverlapsWith(TimeRange timeRange) {
            return Contains(timeRange.StartDate) || Contains(timeRange.EndDate);
        }

        public static TimeRange Union(TimeRange t1, TimeRange t2) {
            if (t1.OverlapsWith(t2)) {
                if (t1.EndDate > t2.EndDate) {
                    return new TimeRange(t2.StartDate, t2.EndDate);
                } else {
                    return new TimeRange(t1.StartDate, t2.EndDate);
                }
            } else {
                throw new ArgumentException("The two timeranges do not overlap");
            }
        }

        public override bool Equals(object obj) {
            var t = obj as TimeRange;
            return t != null ? StartDate.Equals(t.StartDate) && EndDate.Equals(t.EndDate) : false;
        }

        public override int GetHashCode() {
            return StartDate.GetHashCode() ^ EndDate.GetHashCode();
        }
    }
}
