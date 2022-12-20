using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EffectRepresentationRecord {

        [DisplayName("Effect code")]
        public string EffectCode { get; set; }

        [DisplayName("Effect name")]
        public string EffectName { get; set; }

        [DisplayName("Response code")]
        public string ResponseCode { get; set; }

        [DisplayName("Response name")]
        public string ResponseName { get; set; }

        [DisplayName("Has benchmark response")]
        public bool HasBenchmarkResponse {
            get {
                return !string.IsNullOrEmpty(BenchmarkResponseType);
            }
        }

        [DisplayName("Benchmark response type")]
        public string BenchmarkResponseType { get; set; }

        [DisplayName("Benchmark response")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BenchmarkResponse { get; set; }

        [Display(AutoGenerateField = false)]
        [DisplayName("Benchmark response unit")]
        public string BenchmarkResponseUnit { get; set; }

    }
}
