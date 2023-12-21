using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmSamplesBySamplingMethodSubstanceRecord {

        [Description("The biological matrix (e.g., compartment) from which the samples are taken.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("The target unit of the concentration values.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("The sampling type.")]
        [DisplayName("Sampling type")]
        public string SamplingType { get; set; }

        [Description("The exposure route associated with these samples.")]
        [DisplayName("Exposure Route")]
        public string ExposureRoute { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("The total number of samples by this method for this particular substance.")]
        [DisplayName("Number of samples")]
        public int NumberOfSamples { get; set; }

        [Description("The total number of positive measurement values.")]
        [DisplayName("Positives")]
        public int PositiveMeasurements { get; set; }

        [Description("The total number of measurements below the limit of quantification or detection, LOQ or LOD.")]
        [DisplayName("Censored values")]
        public int CensoredValuesMeasurements { get; set; }

        [Description("The total number of measurements below the limit of detection, LOD.")]
        [DisplayName("Non-detects")]
        public int NonDetects { get; set; }

        [Description("The total number of measurements below the limit of quantification, LOQ.")]
        [DisplayName("Non-quantifications")]
        public int NonQuantifications { get; set; }

        [Description("The total number of measurement values.")]
        [DisplayName("Missing values")]
        public int MissingValueMeasurements { get; set; }

        [Description("The total number of substance samples not analysed.")]
        [DisplayName("Non-analysed")]
        public int NonAnalysed { get; set; }

        [Description("The total number of individuals having at least one sample with a positive measurement value.")]
        [DisplayName("Individuals with positive measurements")]
        public int NumberOfIndividualsWithPositives { get; set; }
        
        [Description("The total number of individual days having at least one sample with a positive measurement value.")]
        [DisplayName("Individual days with positive measurements")]
        public int NumberOfIndividualDaysWithPositives { get; set; }

        [Description("The mean of the positive measurement values (not corrected for specific gravity), i.e., values > LOQ, LOD.")]
        [DisplayName("Mean positive samples")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanPositives { get; set; }

        [Description("The median of the positive measurement values (not corrected for specific gravity), i.e., values > LOQ, LOD.")]
        [DisplayName("Median positive samples")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianPositives { get; set; }

        [Description("The lower percentile point of the positive measurement values (not corrected for specific gravity), i.e., values > LOQ, LOD.")]
        [DisplayName("Lower {LowerPercentage} positives)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentilePositives { get; set; }

        [Description("The upper percentile point of the positive measurement values (not corrected for specific gravity), i.e., values > LOQ, LOD.")]
        [DisplayName("Upper {UpperPercentage} positives)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentilePositives { get; set; }
    }
}
