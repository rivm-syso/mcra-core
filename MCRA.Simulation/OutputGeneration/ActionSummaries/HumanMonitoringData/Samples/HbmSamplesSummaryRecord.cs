using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmSamplesSummaryRecord {

        [Description("The biological subsystem (compartment) from which the samples are taken.")]
        [DisplayName("Biological subsystem")]
        public string BiologicalMatrix { get; set; }

        [Description("The sampling type.")]
        [DisplayName("Sampling type")]
        public string SamplingType { get; set; }

        [Description("The exposure route associated with these samples.")]
        [DisplayName("Exposure Route")]
        public string ExposureRoute { get; set; }

        [Description("The total number of samples of this sampling method.")]
        [DisplayName("Number of samples")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfSamples { get; set; }

        [Description("The number of individuals from which samples of this sampling method were taken.")]
        [DisplayName("Number of individuals with samples")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfIndividualsWithSamples { get; set; }

        [Description("Number of days on which samples of this sampling method were taken.")]
        [DisplayName("Number of days with samples")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfIndividualDaysWithSamples { get; set; }

        [Display(AutoGenerateField = false)]
        public List<string> SamplingTimes { get; set; }

    }
}
