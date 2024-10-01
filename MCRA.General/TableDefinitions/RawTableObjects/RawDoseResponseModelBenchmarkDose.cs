using System.Globalization;

namespace MCRA.General.TableDefinitions.RawTableObjects {
    public partial class RawDoseResponseModelBenchmarkDose {
        public double? Rpf { get; set; }
        public double? RpfUpper { get; set; }
        public double? RpfLower { get; set; }

        public Dictionary<string, double> GetParameterValuesDict() {
            return ModelParameterValues?
                .Split(',').Select(c => c.Split('='))
                .Where(r => r.Any())
                .ToDictionary(r => r[0], r => Convert.ToDouble(r[1], CultureInfo.InvariantCulture));
        }
    }
}
