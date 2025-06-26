using MCRA.Utils.ExtensionMethods;

namespace MCRA.General.Action.Settings {

    public enum QueryDefinitionType {
        Empty,
        ValueList,
        Range
    }

    public class IndividualsSubsetDefinition {

        public virtual string NameIndividualProperty { get; set; }

        public virtual string IndividualPropertyQuery { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndividualsSubsetDefinition"/> class.
        /// </summary>
        public IndividualsSubsetDefinition() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndividualsSubsetDefinition"/> class
        /// with the specified name and query string.
        /// </summary>
        /// <param name="name">The name of the individual property.</param>
        /// <param name="query">The query string.</param>
        public IndividualsSubsetDefinition(string name, string query) : this() {
            NameIndividualProperty = name;
            IndividualPropertyQuery = query;
        }

        /// <summary>
        /// Returns the type of query (i.e., numerical range, value list, or empty).
        /// </summary>
        /// <returns></returns>
        public QueryDefinitionType GetQueryDefinitionType() {
            if (string.IsNullOrEmpty(IndividualPropertyQuery) || IndividualPropertyQuery == "-") {
                return QueryDefinitionType.Empty;
            } else if (IndividualPropertyQuery.Contains('\'')) {
                return QueryDefinitionType.ValueList;
            } else if (!string.IsNullOrEmpty(IndividualPropertyQuery.GetSmallerEqualString())) {
                return QueryDefinitionType.Range;
            } else if (IndividualPropertyQuery.GetRangeStrings().Any()) {
                return QueryDefinitionType.Range;
            } else if (!string.IsNullOrEmpty(IndividualPropertyQuery.GetGreaterEqualString())) {
                return QueryDefinitionType.Range;
            }
            return QueryDefinitionType.Empty;
        }

        public HashSet<string> GetQueryKeywords() {
            if (!string.IsNullOrEmpty(IndividualPropertyQuery) && GetQueryDefinitionType() == QueryDefinitionType.ValueList) {
                return IndividualPropertyQuery.Split(',').Select(r => r.Trim().Trim('\'')).ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            return null;
        }

        public double GetRangeMin() {
            var min = double.NaN;
            if (!string.IsNullOrEmpty(IndividualPropertyQuery) && GetQueryDefinitionType() == QueryDefinitionType.Range) {
                foreach (var strRange in IndividualPropertyQuery.GetRangeStrings()) {
                    var val = double.Parse(strRange.Split('-')[0]);
                    if (val < min || double.IsNaN(min)) {
                        min = val;
                    }
                }
                var greaterEqualString = IndividualPropertyQuery.GetGreaterEqualString();
                if (!string.IsNullOrEmpty(greaterEqualString)) {
                    var val = double.Parse(greaterEqualString.Replace("-", string.Empty));
                    if (val < min || double.IsNaN(min)) {
                        min = val;
                    }
                }
            }
            return min;
        }

        public double GetRangeMax() {
            var max = double.NaN;
            if (!string.IsNullOrEmpty(IndividualPropertyQuery) && GetQueryDefinitionType() == QueryDefinitionType.Range) {
                foreach (var strRange in IndividualPropertyQuery.GetRangeStrings()) {
                    var val = double.Parse(strRange.Split('-')[1]);
                    if (val > max || double.IsNaN(max)) {
                        max = val;
                    }
                }
                var smallerEqualString = IndividualPropertyQuery.GetSmallerEqualString();
                if (!string.IsNullOrEmpty(smallerEqualString)) {
                    var val = double.Parse(smallerEqualString.Replace("-", string.Empty));
                    if (val > max || double.IsNaN(max)) {
                        max = val;
                    }
                }
            }
            return max;
        }

        public void SetQueryKeywords(IEnumerable<string> values) {
            var newValues = values
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrEmpty(r))
                .Select(sv => "'" + sv + "'");
            IndividualPropertyQuery = string.Join(",", newValues);
        }

        public void SetRange(double min, double max) {
            if (min >= 0 || max >= 0) {
                if (min >= 0 && max >= 0 && min > max) {
                    var tmp = min;
                    min = max;
                    max = tmp;
                }
                var minString = min >= 0 ? min.ToString() : string.Empty;
                var maxString = max >= 0 ? max.ToString() : string.Empty;
                IndividualPropertyQuery = $"{minString}-{maxString}";
            } else {
                IndividualPropertyQuery = string.Empty;
            }
        }
    }
}
