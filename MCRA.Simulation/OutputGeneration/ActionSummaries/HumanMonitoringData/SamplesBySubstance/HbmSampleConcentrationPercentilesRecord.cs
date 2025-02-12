using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData {
    public sealed class HbmSampleConcentrationPercentilesRecord : PercentilesRecordBase {

        [Display(AutoGenerateField = false)]
        public ExposureTarget TargetUnit { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Code of the biological matrix.")]
        [DisplayName("Biological matrix code")]
        public string BiologicalMatrix { get; set; }

        [Description("Code of the sample type.")]
        [DisplayName("Sample type code")]
        public string SampleTypeCode { get; set; }

        [Description("Description, e.g. analytical method, sampling type.")]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Description("Limit of reporting (LOR).")]
        [DisplayName("LOR")]
        public double LOR { get; set; }

        [Description("Number of measurements.")]
        [DisplayName("Number of measurements")]
        public int NumberOfMeasurements { get; set; }
    }
}
