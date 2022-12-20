using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmIndividualSubstanceConcentrationsRecord {

        [Description("ID of the simulated individual day")]
        [DisplayName("SimulatedIndividualDayID")]
        public string SimulatedIndividualDayId { get; set; }

        [Description("ID of the monitoring survey individual used for simulation")]
        public string HumanMonitoringSurveyIndividualCode { get; set; }

        [Description("ID of the monitoring survey day used for simulation")]
        public string HumanMonitoringSurveyDay { get; set; }

        [Description("Sampling Weight")]
        public double SamplingWeight { get; set; }

        [Description("Body weight")]
        [DisplayName("Bodyweight")]
        public double Bodyweight { get; set; }

        [Description("Biological matrix")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("Sampling type")]
        [DisplayName("SamplingType")]
        public string SamplingType { get; set; }

        [Description("Substance code")]
        [DisplayName("SubstanceCode")]
        public string SubstanceCode { get; set; }

        [Description("Monitoring concentration (MonitoringConcentrationUnit)")]
        [DisplayName("Concentration (MonitoringConcentrationUnit)")]
        public double Concentration { get; set; }
    }
}
