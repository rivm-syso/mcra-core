using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class EffectRepresentation {

        public Effect Effect { get; set; }
        public Response Response { get; set; }

        public double? BenchmarkResponse { get; set; }

        public BenchmarkResponseType BenchmarkResponseType { get; set; } = BenchmarkResponseType.Undefined;

        public bool HasBenchmarkResponseType() {
            return BenchmarkResponseType != BenchmarkResponseType.Undefined;
        }

        public bool HasBenchmarkResponse() {
            return HasBenchmarkResponseType()
                && (BenchmarkResponse.HasValue && !double.IsNaN(BenchmarkResponse.Value)
                || BenchmarkResponseType == BenchmarkResponseType.Ed50);
        }

        public bool HasBenchmarkResponseValue() {
            return HasBenchmarkResponseType()
                && BenchmarkResponse.HasValue && !double.IsNaN(BenchmarkResponse.Value);
        }
    }
}
