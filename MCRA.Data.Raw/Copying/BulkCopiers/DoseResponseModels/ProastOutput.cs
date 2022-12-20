using MCRA.Utils.Converters;
using System.Text.Json.Serialization;

namespace MCRA.Data.Raw.Copying.BulkCopiers.DoseResponseModels {
    public class ProastOutput {

        [JsonPropertyName("PRversion")]
        public List<string> ProastVersion { get; set; }

        [JsonPropertyName("odt")]
        public List<Dictionary<string, object>> Data { get; set; }

        [JsonPropertyName("varnames")]
        public List<string> VariableNames { get; set; }

        [JsonPropertyName("xans")]
        public List<int> DoseVariableColumnIndexes { get; set; }

        [JsonPropertyName("yans")]
        public List<int> ResponseVariableColumnIndex { get; set; }

        [JsonPropertyName("model.ans")]
        public List<int> ModelAns { get; set; }

        [JsonPropertyName("covar.txt")]
        public List<string> CovariateLevels { get; set; }

        [JsonPropertyName("covar.name")]
        public List<string> CovariateNames { get; set; }

        [JsonPropertyName("dtype")]
        public List<int> DistributionType { get; set; }

        [JsonPropertyName("ces.ans")]
        public List<int> CriticalEffectSizeType { get; set; }

        [JsonPropertyName("CES")]
        [JsonConverter(typeof(DoubleArrayJsonConverter))]
        public double[] CriticalEffectSize { get; set; }

        [JsonPropertyName("EXP")]
        public ProastModelFit ExponentialModel { get; set; }

        [JsonPropertyName("HILL")]
        public ProastModelFit HillModel { get; set; }

        [JsonPropertyName("two.stage")]
        public ProastModelFit TwoStage { get; set; }

        [JsonPropertyName("log.logist")]
        public ProastModelFit LogLogist { get; set; }

        [JsonPropertyName("Weibull")]
        public ProastModelFit Weibull { get; set; }

        [JsonPropertyName("log.prob")]
        public ProastModelFit LogProb { get; set; }

        [JsonPropertyName("gamma")]
        public ProastModelFit Gamma { get; set; }

        [JsonPropertyName("logistic")]
        public ProastModelFit Logistic { get; set; }

        [JsonPropertyName("probit")]
        public ProastModelFit Probit { get; set; }

        [JsonPropertyName("SINGMOD")]
        public ProastModelFit SingleModel { get; set; }

        [JsonPropertyName("ref.lev")]
        public ICollection<int?> ReferenceLevel { get; set; }
    }
}
