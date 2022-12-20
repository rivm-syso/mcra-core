using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UnitVariabilityFactorsRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("unit variability info may be compound specific or is indicated as general (not compound specific)")]
        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("unit variability info may be processing specific or is indicated as general (not processing specific)")]
        [DisplayName("Processing code")]
        public string ProcessingTypeCode { get; set; }

        [DisplayName("Processing description")]
        public string ProcessingTypeDescription { get; set; }

        [Description("Default unit weight of the raw agricultural commodity.")]
        [DisplayName("Unit weight (RAC) (g)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UnitWeightRac { get; set; }

        [Description("Variability factor.")]
        [DisplayName("Variability factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UnitVariabilityFactor { get; set; }

        [Description("Coefficient of variation.")]
        [DisplayName("Coefficient of variation")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double CoefficientOfVariation { get; set; }

        [Description("Number of units in composite sample.")]
        [DisplayName("Number of units in composite sample")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int UnitsInCompositeSample { get; set; }
    }
}
