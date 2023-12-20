using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData.SamplesBySubstance {
    public sealed class HbmSampleConcentrationOutlierRecord {
        [Description("Code of the biological matrix.")]
        [DisplayName("Biological matrix code")]
        public string BiologicalMatrix { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Individual code.")]
        [DisplayName("IndividualCode")]
        public string IndividualCode { get; set; }

        [Description("Sample code.")]
        [DisplayName("SampleCode")]
        public string SampleCode { get; set; }

        [Description("Residue.")]
        [DisplayName("Residue")]
        public double Residue { get; set; }
    }
}
