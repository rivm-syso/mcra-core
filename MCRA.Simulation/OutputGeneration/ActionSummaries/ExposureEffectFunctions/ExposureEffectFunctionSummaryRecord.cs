using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace MCRA.Simulation.OutputGeneration {
    public class ExposureEffectFunctionSummaryRecord {

        [Description("Substance name.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Effect.")]
        [DisplayName("Effect")]
        public string Effect { get; set; }

        [Description("Target level.")]
        [DisplayName("Target level")]
        public string TargetLevel { get; set; }

        [Description("The exposure route associated with these functions.")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [Description("The biological subsystem (compartment) associated with these functions.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("Unit.")]
        [DisplayName("Unit")]
        public string DoseUnit { get; set; }

        [Description("Expression type.")]
        [DisplayName("Expression type")]
        public string ExpressionType { get; set; }

        [Description("Effect metric.")]
        [DisplayName("Effect metric")]
        public string EffectMetric { get; set; }

        [Description("The function in mathematical notation.")]
        [DisplayName("Exposure response type")]
        public string ExposureResponesType { get; set; }

        [Description("The function in mathematical notation.")]
        [DisplayName("Exposure response specification")]
        public string ExposureResponseSpecification { get; set; }

        [Description("The baseline level of the exposure effect function.")]
        [DisplayName("Baseline")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Baseline { get; set; }
    }
}

