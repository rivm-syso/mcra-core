using System;
using System.Collections.Generic;

namespace MCRA.Utils.ExtensionMethods {

    /// <summary>
    /// Extension methods for data/time objects.
    /// </summary>
    public static class DateTimeExtensions {

        private static readonly DateTime TimeStampBase = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly long JavaScriptDatetimeMinTimeTicks = TimeStampBase.Ticks;

        /// <summary>
        /// Returns true if the source date is between the lower and upper bound dates
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static bool IsBetween(this DateTime source, DateTime lower, DateTime upper) {
            return source >= lower && source <= upper;
        }

        /// <summary>
        /// Returns an enumerable with datetime objects of the last x days
        /// </summary>
        /// <param name="source"></param>
        /// <param name="x">The number of days to date back w.r.t. the datetime of the source</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> GetLastXDays(this DateTime source, int x) {
            var startDate = source.AddDays(-x);
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0);
            var endDate = source;
            while (startDate <= endDate) {
                yield return startDate;
                startDate = startDate.AddDays(1);
            }
        }

        /// <summary>
        /// Returns an enumerable with datetime objects of the last x hours
        /// </summary>
        /// <param name="source"></param>
        /// <param name="x">The number of hours to date back w.r.t. the datetime of the source</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> GetLastXHours(this DateTime source, int x) {
            var startDate = source.AddHours(-x);
            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0);
            var endDate = source;
            while (startDate <= endDate) {
                yield return startDate;
                startDate = startDate.AddHours(1);
            }
        }

        /// <summary>
        /// Takes the floor hour of the datetime
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static DateTime Floor(this DateTime source) {
            return new DateTime(source.Year, source.Month, source.Day, source.Hour, 0, 0, source.Kind);
        }

        /// <summary>
        /// Rounds the datetime by hour
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static DateTime Round(this DateTime source) {
            var updated = source.AddMinutes(30);
            return new DateTime(updated.Year, updated.Month, updated.Day, updated.Hour, 0, 0, source.Kind);
        }

        /// <summary>
        /// Returns the time span between the two date/time objects.
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static TimeSpan GetTimeSpan(DateTime? begin, DateTime? end) {
            if (begin == null || end == null) {
                return new TimeSpan(0);
            }
            return ((DateTime)end - (DateTime)begin);
        }

        /// <summary>
        /// Converts the date time to a unix timestamp.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int ToUnixTimeStamp(this DateTime dateTime) {
            var unixTimestamp = (Int32)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }

        /// <summary>
        /// Converts the nullable date time to a unix timestamp. If the datetime
        /// is null, then the timestamp of the current time is returned.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int ToUnixTimeStamp(this DateTime? dateTime) {
            if (dateTime == null) {
                return ToUnixTimeStamp(DateTime.Now);
            } else {
                return ToUnixTimeStamp((DateTime)dateTime);
            }
        }

        /// <summary>
        /// Converts the provided datetime to a javascript timestamp.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToJavaScriptTimeStamp(this DateTime dateTime) {
            return (long)((dateTime.ToUniversalTime().Ticks - JavaScriptDatetimeMinTimeTicks) / 10000);
        }

        /// <summary>
        /// Converts the provided datetime to a javascript timestamp.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToJavaScriptTimeStamp(this DateTime? dateTime) {
            if (dateTime == null) {
                return ToJavaScriptTimeStamp(DateTime.Now);
            } else {
                return ToJavaScriptTimeStamp((DateTime)dateTime);
            }
        }

        /// <summary>
        /// Creates a date time from a unix time stamp.
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(int unixTimeStamp) {
            // Unix timestamp is seconds past epoch
            var dtDateTime = TimeStampBase;
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
