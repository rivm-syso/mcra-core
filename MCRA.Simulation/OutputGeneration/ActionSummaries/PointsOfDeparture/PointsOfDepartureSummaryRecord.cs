using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PointsOfDepartureSummaryRecord {

        [Description("dose response model")]
        [DisplayName("Code")]
        public string Code { get; set; }

        [Description("The name of the effect for which this point of departure is defined.")]
        [DisplayName("Effect name")]
        public string EffectName { get; set; }

        [Description("The code of the effect for which this point of departure is defined.")]
        [DisplayName("Effect code")]
        public string EffectCode { get; set; }

        [Description("The name of the substance for which this point of departure is defined.")]
        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [Description("The code of the substance for which this point of departure is defined.")]
        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("Species of the test-system used for determining this point of departure.")]
        [DisplayName("Species")]
        public string System { get; set; }

        [Description("Model code")]
        [DisplayName("Model code")]
        [Display(AutoGenerateField = false)]
        public string ModelCode { get; set; }

        [Description("The model equation of the dose response model from which the point of departure is derived.")]
        [DisplayName("Equation")]
        public string ModelEquation { get; set; }

        [Description("The parameter values of the dose response model.")]
        [DisplayName("Parameter values")]
        public string ParameterValues { get; set; }

        [Description("The point of departure for the test-system.")]
        [DisplayName("Point of departure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PointOfDeparture { get; set; }

        [Description("The type of the hazard dose.")]
        [DisplayName("Point of departure type")]
        public string PointOfDepartureType { get; set; }

        [Description("The route of exposure.")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [Description("The dose unit of the dose response model.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Critical effect size")]
        [DisplayName("Critical effect size")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double CriticalEffectSize { get; set; }

        [Description("Number of uncertainty sets.")]
        [DisplayName("Uncertainty sets (N)")]
        public int NumberOfUncertaintySets { get; set; }

        [Description("Median point of departure uncertainty sets")]
        [DisplayName("Median point of departure uncertainty")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Minimum point of departure uncertainty sets")]
        [DisplayName("Minimum point of departure uncertainty")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Minimum { get; set; }

        [Description("Maximum point of departure uncertainty sets")]
        [DisplayName("Maximum point of departure uncertainty")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Maximum { get; set; }
    }
}
