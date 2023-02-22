using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ValueQualifier {
        [Display(Name = "=", ShortName = "=")]
        Equals = 0,
        [Display(Name = "<", ShortName = "<")]
        LessThan = 1,
    }

    public sealed class QualifiedValue : IEquatable<QualifiedValue>, IComparable<QualifiedValue> {

        /// <summary>
        /// Qualifier of the qualified value.
        /// </summary>
        public ValueQualifier Qualifier { get; } = ValueQualifier.Equals;

        /// <summary>
        /// The value of the qualified value.
        /// </summary>
        public double Value { get; } = double.NaN;

        /// <summary>
        /// Creates a new <see cref="QualifiedValue" /> instance.
        /// </summary>
        public QualifiedValue() { }

        /// <summary>
        /// Creates a new <see cref="QualifiedValue" /> instance form a specified
        /// value and qualifier.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="qualifier"></param>
        public QualifiedValue(double value, ValueQualifier qualifier) {
            Value = value;
            Qualifier = qualifier;
        }

        /// <summary>
        /// Creates a new <see cref="QualifiedValue" /> instance form a specified
        /// value. Qualifier is set to the default (i.e., equals).
        /// </summary>
        /// <param name="value"></param>
        public QualifiedValue(double value) {
            Value = value;
        }

        public bool IsNan() {
            return double.IsNaN(Value);
        }

        bool IEquatable<QualifiedValue>.Equals(QualifiedValue other) {
            return this == other;
        }

        public int CompareTo(QualifiedValue other) {
            if (other == null) {
                return 1;
            } else if (Qualifier == ValueQualifier.Equals) {
                if (other.Qualifier == ValueQualifier.Equals) {
                    return Value.CompareTo(other.Value);
                } else if (other.Qualifier == ValueQualifier.LessThan) {
                    if (other.Value <= Value) {
                        return 1;
                    } else {
                        return 0;
                    }
                } else {
                    throw new NotImplementedException();
                }
            } else if (Qualifier == ValueQualifier.LessThan) {
                if (other.Qualifier == ValueQualifier.Equals) {
                    if (other.Value >= Value) {
                        return -1;
                    } else {
                        return 0;
                    }
                } else if (other.Qualifier == ValueQualifier.LessThan) {
                    return 0;
                } else {
                    throw new NotImplementedException();
                }
            } else {
                throw new NotImplementedException();
            }
        }

        public static bool operator ==(QualifiedValue val1, QualifiedValue val2) {
            if (ReferenceEquals(val1, val2)) {
                return true;
            } else if (val1 is null || val2 is null) {
                return false;
            } else {
                return val1.Qualifier == val2.Qualifier && val1.Value == val2.Value;
            }
        }

        public static bool operator !=(QualifiedValue val1, QualifiedValue val2) {
            return !(val1 == val2);
        }

        public static bool operator <(QualifiedValue val1, QualifiedValue val2) {
            return val1.CompareTo(val2) < 0;
        }

        public static bool operator >(QualifiedValue val1, QualifiedValue val2) {
            return val1.CompareTo(val2) > 0;
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = Value.GetHashCode();
                hashCode = (hashCode * 7) ^ Qualifier.GetHashCode();
                return hashCode;
            }
        }

        public string ToString(string displayFormat = null) {
            var qualifierString = Qualifier == ValueQualifier.LessThan ? "<" : string.Empty;
            if (double.IsNaN(Value)) {
                return "-";
            } else if (!string.IsNullOrEmpty(displayFormat)) {
                return $"{qualifierString}{Value.ToString(displayFormat)}";
            } else {
                return $"{qualifierString}{Value}";
            }
        }

        public override string ToString() {
            return ToString(null);
        }

        public override bool Equals(object obj) {
            if (obj is QualifiedValue val) {
                return this == val;
            }
            return base.Equals(obj);
        }
    }
}
