using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ComponentRecord {
        [Description("Component.")]
        [DisplayName("Component")]
        public int ComponentNumber { get; set; }

        [Description("Number of substances.")]
        [DisplayName("Number of substances")]
        public int NumberOfSubstances { get; set; }

        [Description("Convergence reached at.")]
        [DisplayName("Convergence")]
        [DisplayFormat(DataFormatString = "{0:F6}")]
        public double Convergence { get; set; }

        [Description("Iteration.")]
        [DisplayName("Iteration")]
        public int Iteration { get; set; }

        [Description("Sparseness reached.")]
        [DisplayName("Sparseness")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Sparseness { get; set; }

        [Description("Variation explained based on X = U * Transpose(V) + E.")]
        [DisplayName("Variation explained")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Variance { get; set; }

        [Description("Cumulative % variation explained.")]
        [DisplayName("Cumulative % variation explained")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double CumulativeExplainedVariance { get; set; }

    }
}
