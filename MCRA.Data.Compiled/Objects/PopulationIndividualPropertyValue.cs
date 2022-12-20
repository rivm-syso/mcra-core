using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Compiled.Objects {
    public sealed class PopulationIndividualPropertyValue : IEquatable<PopulationIndividualPropertyValue> {

        private HashSet<string> _categoricalLevels;

        public string Value { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public IndividualProperty IndividualProperty { get; set; }

        public bool IsNumeric() {
            return string.IsNullOrEmpty(Value) || (MinValue != null || MaxValue != null);
        }

        public HashSet<string> CategoricalLevels {
            get {
                if (_categoricalLevels == null) {
                    if (!string.IsNullOrEmpty(Value)) {
                        _categoricalLevels = Value.Split(',').Select(r => r.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    }
                }
                return _categoricalLevels;
            }
        }

        public bool Equals(PopulationIndividualPropertyValue other) {
            throw new NotImplementedException();
        }
    }
}
