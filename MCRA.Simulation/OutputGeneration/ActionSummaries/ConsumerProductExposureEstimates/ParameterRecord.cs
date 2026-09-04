using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ParameterRecord : PercentileRecord {
        [Display(AutoGenerateField = false)]
        public LogNormalDistribution Distribution { get; set; }
        [Display(AutoGenerateField = false)]
        public List<(double x, double y)> FittedValues { get; set; }

        [Description("Mu")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(Name = "Mu", Order = 2)]
        public double Mu { get => Distribution.Mu; }
        [Description("Sigma")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(Name = "Sigma", Order = 3)]
        public double Sigma { get => Distribution.Sigma; }
    }
}
