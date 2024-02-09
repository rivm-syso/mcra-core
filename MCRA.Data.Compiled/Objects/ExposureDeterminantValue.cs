
using System.Globalization;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ExposureDeterminantValue {
        public string TextValue { get; set; }
        public double? DoubleValue { get; set; }
        public ExposureDeterminant Property { get; set; }
        public string DisplayValue => TextValue ?? DoubleValue?.ToString(CultureInfo.InvariantCulture) ?? "";
    }
}
