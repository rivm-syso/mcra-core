using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class EffectRepresentation {

        public Effect Effect { get; set; }
        public Response Response { get; set; }

        public double? BenchmarkResponse { get; set; }
        public string BenchmarkResponseTypeString { get; set; }

        public BenchmarkResponseType BenchmarkResponseType {
            get {
                if (!string.IsNullOrEmpty(BenchmarkResponseTypeString)) {
                    return BenchmarkResponseTypeConverter.FromString(BenchmarkResponseTypeString);
                }
                return BenchmarkResponseType.Undefined;
            }
        }

        public bool HasBenchmarkResponseType() {
            return !string.IsNullOrEmpty(BenchmarkResponseTypeString)
                && BenchmarkResponseType != BenchmarkResponseType.Undefined;
        }

        public bool HasBenchmarkResponse() {
            return HasBenchmarkResponseType()
                && ((BenchmarkResponse.HasValue && !double.IsNaN(BenchmarkResponse.Value))
                || BenchmarkResponseType == BenchmarkResponseType.Ed50);
        }

        public bool HasBenchmarkResponseValue() {
            return HasBenchmarkResponseType()
                && BenchmarkResponse.HasValue && !double.IsNaN(BenchmarkResponse.Value);
        }
    }
}
