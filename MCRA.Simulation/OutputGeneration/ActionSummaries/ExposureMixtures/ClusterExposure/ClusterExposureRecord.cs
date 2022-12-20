using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ClusterExposureRecord {
        public string IdComponent { get; set; }
        public int IdCluster { get; set; }

        [Description("Relative contribution to component.")]
        [DisplayName("Relative contribution to component (%)")]
        public string Contribution { get; set; }

        [Description("Number of individuals.")]
        [DisplayName("Number of individuals")]
        public int NumberOfIndividuals { get; set; }

        [Description("Substance name.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Mean exposure.")]
        [DisplayName("Mean exposure (MonitoringConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanExposure { get; set; }

        [Description("Sd exposure.")]
        [DisplayName("Standard deviation")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Sd { get; set; }

        [Description("Median exposure.")]
        [DisplayName("Median exposure (MonitoringConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianExposure { get; set; }

        [Description("Maximum exposure.")]
        [DisplayName("P95 exposure (MonitoringConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double P95 { get; set; }

        [Description("Minimum exposure.")]
        [DisplayName("Minimum exposure (MonitoringConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MinimumExposure { get; set; }

        [Description("Maximum exposure.")]
        [DisplayName("Maximum exposure (MonitoringConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MaximumExposure { get; set; }

        [Description("Mean exposure other subgroups.")]
        [DisplayName("Mean exposure other subgroups (MonitoringConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanExposureOthers { get; set; }

        [Description("Log transformed data, two-sided: mu1 != mu2, significance level (p < 0.05).")]
        [DisplayName("Significance")]
        public string pValue { get; set; }
    }
}
