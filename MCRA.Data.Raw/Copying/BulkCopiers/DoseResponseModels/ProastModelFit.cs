using MCRA.Utils.Converters;
using System.Text.Json.Serialization;

namespace MCRA.Data.Raw.Copying.BulkCopiers.DoseResponseModels {

    public class ProastModelFit {
        [JsonPropertyName("modelname")]
        public List<string> ModelName { get; set; }

        [JsonPropertyName("model.ans")]
        public List<int> ModelAns { get; set; }

        [JsonPropertyName("BMD")]
        [JsonConverter(typeof(DoubleArrayJsonConverter))]
        public double[] BMD { get; set; }

        [JsonPropertyName("CED")]
        [JsonConverter(typeof(DoubleArrayJsonConverter))]
        public double[] CED { get; set; }

        [JsonPropertyName("CED.matr")]
        [JsonConverter(typeof(DoubleMatrixJsonConverter))]
        public double[][] CEDMatrix { get; set; }

        [JsonPropertyName("CED.boot")]
        [JsonConverter(typeof(DoubleMatrixJsonConverter))]
        public double[][] CEDBootstrap { get; set; }

        [JsonPropertyName("text.par")]
        public List<string> ParameterNames { get; set; }

        [JsonPropertyName("lb")]
        [JsonConverter(typeof(DoubleArrayJsonConverter))]
        public double[] Lowers { get; set; }

        [JsonPropertyName("ub")]
        [JsonConverter(typeof(DoubleArrayJsonConverter))]
        public double[] Uppers { get; set; }

        [JsonPropertyName("MLE")]
        [JsonConverter(typeof(DoubleArrayJsonConverter))]
        public double[] Estimates { get; set; }

        [JsonPropertyName("RPF")]
        [JsonConverter(typeof(DoubleArrayJsonConverter))]
        public double[] RPFs { get; set; }

        [JsonPropertyName("conf.int")]
        [JsonConverter(typeof(DoubleMatrixJsonConverter))]
        public double[][] ConfidenceIntervals { get; set; }

        [JsonPropertyName("converged")]
        [JsonConverter(typeof(DoubleArrayJsonConverter))]
        public double[] Converged { get; set; }

        [JsonPropertyName("loglik")]
        [JsonConverter(typeof(DoubleArrayJsonConverter))]
        public double[] LogLikelihood { get; set; }

        [JsonPropertyName("sens.lev")]
        public List<string> SensitiveLevel { get; set; }

        [JsonPropertyName("nr.gr")]
        [JsonConverter(typeof(DoubleArrayJsonConverter))]
        public double[] NumberOfGroupings { get; set; }

        [JsonPropertyName("gr.txt")]
        public List<string> Groupings { get; set; }

        [JsonPropertyName("nrp")]
        public List<int> NoModelParameters { get; set; }
    }
}
