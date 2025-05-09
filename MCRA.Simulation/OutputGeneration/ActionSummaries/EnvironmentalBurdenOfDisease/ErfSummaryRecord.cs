﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class ErfSummaryRecord {

        [Description("The code of the exposure response function.")]
        [DisplayName("ERF Code")]
        public string ErfCode { get; set; }

        [Description("Substance name.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("The name of the health effect.")]
        [DisplayName("Effect name")]
        public string EffectName { get; set; }

        [Description("The code of the health effect.")]
        [DisplayName("Effect code")]
        public string EffectCode { get; set; }

        [Description("Target exposure level (internal / external) associated with this function.")]
        [DisplayName("Target level")]
        public string TargetLevel { get; set; }

        [Description("The (external) exposure route associated with this function.")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [Description("The (internal) biological sub-system (matrix) of the exposure associated with this function.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("Unit of measurement of the exposure.")]
        [DisplayName("Target unit")]
        public string TargetUnit { get; set; }

        [Description("Expression type of the exposure.")]
        [DisplayName("Expression type")]
        public string ExpressionType { get; set; }

        [Description("The effect metric.")]
        [DisplayName("Effect metric")]
        public string EffectMetric { get; set; }

        [Description("The type of exposure response specification.")]
        [DisplayName("Exposure response type")]
        public string ExposureResponseType { get; set; }

        [Description("The function in mathematical notation.")]
        [DisplayName("Exposure response specification")]
        public string ExposureResponseSpecification { get; set; }

        [Description("The lower threshold of the function in mathematical notation.")]
        [DisplayName("Threshold lower")]
        public string ExposureResponseSpecificationLower { get; set; }

        [Description("The upper threshold of the function in mathematical notation.")]
        [DisplayName("Threshold upper")]
        public string ExposureResponseSpecificationUpper { get; set; }

        [Description("Dose unit of the exposure response function.")]
        [DisplayName("EFR dose unit")]
        public string ErfDoseUnit { get; set; }

        [Description("Alignment factor to align dose unit of ERF with target unit.")]
        [DisplayName("EFR dose unit alignment factor")]
        public double ErfDoseAlignmentFactor { get; set; }

        [Description("The baseline level of the exposure response function.")]
        [DisplayName("Baseline")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Baseline { get; set; }

        [Description("Specifies whether this ERF is composed of multiple exposure subgroups/bins.")]
        [DisplayName("Has subgroups")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public bool HasSubgroups { get; set; }

        [Display(AutoGenerateField = false)]
        public List<ExposureResponseDataPoint> ExposureResponseDataPoints { get; set; }

        [Display(AutoGenerateField = false)]
        public List<ExposureResponseDataPoint> ExposureResponseChartDataPoints { get; set; }
    }
}
