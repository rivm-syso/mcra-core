﻿using System.Globalization;

namespace MCRA.Data.Compiled.Objects {
    public sealed class SamplePropertyValue : IEquatable<SamplePropertyValue> {
        public string TextValue { get; set; }
        public double? DoubleValue { get; set; }
        public SampleProperty SampleProperty { get; set; }

        public string Value => TextValue ?? DoubleValue?.ToString(CultureInfo.InvariantCulture) ?? "";

        public bool IsNumeric() => string.IsNullOrEmpty(TextValue) || DoubleValue.HasValue;

        #region Equality

        public bool Equals(SamplePropertyValue other) {
            if (other == null) {
                return false;
            }
            return SampleProperty == other.SampleProperty
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

            return Equals(obj as SamplePropertyValue);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (SampleProperty?.GetHashCode() ?? 1)
                    + 17 * (!string.IsNullOrEmpty(Value) ? Value.GetHashCode() : 1);
                return hashCode;
            }
        }
        #endregion
    }
}
