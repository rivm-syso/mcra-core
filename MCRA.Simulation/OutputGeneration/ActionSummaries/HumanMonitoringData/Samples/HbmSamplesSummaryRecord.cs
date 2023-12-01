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
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [Description("The total number of samples of this sampling method.")]
        [DisplayName("Total samples (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfSamples { get; set; }

        [Description("The number of individuals from which samples of this sampling method were taken.")]
        [DisplayName("Individuals with samples (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfIndividualsWithSamples { get; set; }

        [Description("Number of days on which samples of this sampling method were taken.")]
        [DisplayName("Days with samples (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfIndividualDaysWithSamples { get; set; }

        [Description("The number of samples that have one or more non-analysed substances.")]
        [DisplayName("Samples with non-analysed substances (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfSamplesNonAnalysed { get; set; }

        [Display(AutoGenerateField = false)]
        public List<string> SamplingTimes { get; set; }

    }
}
