using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringAnalysis {

    public sealed class HbmConcentrationModelRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("The concentration model type.")]
        [DisplayName("Fitted Model")]
        public ConcentrationModelType Model { get; set; }

        [Description("Mean of log transformed concentrations.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [DisplayName("Mu (logscale)")]
        public double Mu { get; set; }

        [Description("Standard deviation log transformed concentrations.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [DisplayName("Sigma (logscale)")]
        public double Sigma { get; set; }

        [Description("Mean of untransformed concentrations: exp(mu + sigma^2/2).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [DisplayName("Mu")]
        public double Mean { get; set; }

        [Description("Standard deviation of untransformed concentrations: sqrt(exp(sigma^2)-1) * exp(mu + sigma^2/2).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [DisplayName("Sigma")]
        public double StandardDeviation { get; set; }

        [Description("Total number of analysed samples on which this model is based.")]
        [DisplayName("Total samples analysed (n)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public int TotalMeasurementsCount { get; set; }

        [Description("The percentage of values considered as censored values (value < LOD or value < LOQ).")]
        [DisplayName("Censored (%)")]
        [DisplayFormat(DataFormatString = "{0:P1}")]
        public double FractionCensored { get; set; }

    }
}