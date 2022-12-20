using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Compiled.ObjectExtensions {
    public static class PopulationIndividualPropertyValueExtensions {
        public static List<int> GetMonths(this PopulationIndividualPropertyValue propertyValue) {
            if (!double.IsNaN(propertyValue?.MinValue ?? double.NaN)
                && !double.IsNaN(propertyValue?.MaxValue ?? double.NaN)
            ) {
                var min = (int)propertyValue.MinValue;
                var max = (int)propertyValue.MaxValue;
                if (min <= max) {
                    var months = Enumerable.Range(min, (max - min) + 1).ToList();
                    return months;
                } else {
                    var months = Enumerable.Range(min, (12 - min) + 1).ToList();
                    months.AddRange(Enumerable.Range(1, max));
                    return months;
                }
            }
            if (!string.IsNullOrEmpty(propertyValue.Value)) {
                var rawMonths = propertyValue.Value.Split(',').Select(c => c.Trim().ToLower()).ToList();
                var months = rawMonths
                    .Select(c => (int)MonthTypeConverter.FromString(c))
                    .ToList();
                return months;
            }
            return null;
        }
    }
}
