using System;
using System.Globalization;

namespace MCRA.Data.Compiled.Objects {
    public sealed class IndividualPropertyValue : IEquatable<IndividualPropertyValue> {
        public string TextValue { get; set; }
        public double? DoubleValue { get; set; }
        public IndividualProperty IndividualProperty { get; set; }

        public string Value {
            get {
                if (TextValue == null) {
                    return string.Format(CultureInfo.InvariantCulture, "{0}", DoubleValue);
                }
                return TextValue;
            }
        }

        public bool IsNumeric() {
            return string.IsNullOrEmpty(TextValue) || DoubleValue != null;
        }

        #region Equality

        public bool Equals(IndividualPropertyValue other) {
            if (other == null) {
                return false;
            }
            return IndividualProperty == other.IndividualProperty
                && string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj) {
            if (obj is null) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals(obj as IndividualPropertyValue);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (IndividualProperty?.GetHashCode() ?? 1)
                    + 17 * (!string.IsNullOrEmpty(Value) ? Value.GetHashCode() : 1);
                return hashCode;
            }
        }

        #endregion
    }
}
