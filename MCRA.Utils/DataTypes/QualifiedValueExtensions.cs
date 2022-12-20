using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils.DataTypes {
    public static class QualifiedValueExtensions {

        /// <summary>
        /// Returns the average qualified value for the list of qualified values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static QualifiedValue Average(this IEnumerable<QualifiedValue> values) {
            if (!values.Any()) {
                return new QualifiedValue();
            } else if (values.Distinct().Count() == 1) {
                return values.First();
            } else if (values.All(r => r.Qualifier == ValueQualifier.Equals)) {
                return new QualifiedValue(values.Average(r => r.Value), ValueQualifier.Equals);
            } else {
                return new QualifiedValue(double.NaN, ValueQualifier.Equals);
            }
        }

        /// <summary>
        /// Returns the maximum qualified value for the list of qualified values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static QualifiedValue Max(this IEnumerable<QualifiedValue> values) {
            if (!values.Any()) {
                return new QualifiedValue();
            } else if (values.Distinct().Count() == 1) {
                return values.First();
            } else if (values.All(r => r.Qualifier == ValueQualifier.Equals)) {
                return new QualifiedValue(values.Max(r => r.Value), ValueQualifier.Equals);
            } else if (values.All(r => r.Qualifier == ValueQualifier.LessThan)) {
                return new QualifiedValue(values.Max(r => r.Value), ValueQualifier.LessThan);
            } else {
                var max = values.Where(r => r.Qualifier == ValueQualifier.Equals).Max(r => r.Value);
                if (values.Where(r => r.Qualifier == ValueQualifier.LessThan).Max(r => r.Value) <= max) {
                    return new QualifiedValue(max, ValueQualifier.Equals);
                } else {
                    return new QualifiedValue(double.NaN, ValueQualifier.Equals);
                }
            }
        }

        /// <summary>
        /// Returns the minimum qualified value for the list of qualified values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static QualifiedValue Min(this IEnumerable<QualifiedValue> values) {
            if (!values.Any()) {
                return new QualifiedValue();
            } else if (values.Distinct().Count() == 1) {
                return values.First();
            } else if (values.All(r => r.Qualifier == ValueQualifier.Equals)) {
                return new QualifiedValue(values.Min(r => r.Value), ValueQualifier.Equals);
            } else if (values.All(r => r.Qualifier == ValueQualifier.LessThan)) {
                return new QualifiedValue(values.Min(r => r.Value), ValueQualifier.LessThan);
            } else {
                var min = values.Where(r => r.Qualifier == ValueQualifier.LessThan).Min(r => r.Value);
                if (values.Where(r => r.Qualifier == ValueQualifier.Equals).Min(r => r.Value) >= min) {
                    return new QualifiedValue(min, ValueQualifier.LessThan);
                } else {
                    return new QualifiedValue(double.NaN, ValueQualifier.Equals);
                }
            }
        }
    }
}
