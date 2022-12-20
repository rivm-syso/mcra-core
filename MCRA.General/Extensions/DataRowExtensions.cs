using System;
using System.Data;

namespace MCRA.General.Extensions {
    public static class DataRowExtensions {
        public static void WriteNonNullInt32(this DataRow row, Enum fieldEnum, int? value, int[] counts = null) {
            if (value.HasValue) {
                row[Convert.ToInt32(fieldEnum)] = value.Value;
                if(counts != null) {
                    counts[Convert.ToInt32(fieldEnum)]++;
                }
            }
        }
        public static void WriteNonNullDateTime(this DataRow row, Enum fieldEnum, DateTime? value, int[] counts = null) {
            if (value.HasValue) {
                row[Convert.ToInt32(fieldEnum)] = value.Value;
                if(counts != null) {
                    counts[Convert.ToInt32(fieldEnum)]++;
                }
            }
        }
        public static void WriteNonNullDouble(this DataRow row, Enum fieldEnum, double? value, int[] counts = null) {
            if (value.HasValue && !double.IsNaN(value.Value)) {
                row[Convert.ToInt32(fieldEnum)] = value.Value;
                if(counts != null) {
                    counts[Convert.ToInt32(fieldEnum)]++;
                }
            }
        }
        public static void WriteNonNullBoolean(this DataRow row, Enum fieldEnum, bool? value, int[] counts = null) {
            if (value.HasValue) {
                row[Convert.ToInt32(fieldEnum)] = value.Value;
                if(counts != null) {
                    counts[Convert.ToInt32(fieldEnum)]++;
                }
            }
        }
        public static void WriteNonNaNDouble(this DataRow row, Enum fieldEnum, double value, int[] counts = null) {
            if (!double.IsNaN(value)) {
                row[Convert.ToInt32(fieldEnum)] = value;
                if(counts != null) {
                    counts[Convert.ToInt32(fieldEnum)]++;
                }
            }
        }
        public static void WriteNonEmptyString(this DataRow row, Enum fieldEnum, string value, int[] counts = null) {
            if (!string.IsNullOrWhiteSpace(value)) {
                row[Convert.ToInt32(fieldEnum)] = value;
                if(counts != null) {
                    counts[Convert.ToInt32(fieldEnum)]++;
                }
            }
        }
        public static void WriteValue<T>(this DataRow row, Enum fieldEnum, T value, int[] counts = null) {
            if(value != null) {
                row[Convert.ToInt32(fieldEnum)] = value;
                if(counts != null) {
                    counts[Convert.ToInt32(fieldEnum)]++;
                }
            }
        }
    }
}
