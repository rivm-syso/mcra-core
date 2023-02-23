using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConcentrationLimitsFoodBySubstanceRecord {
        [Description("Specifies whether there is monitoring data that should replace part of the consumption data for the specified focal commodities.")]
        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [Description("Food code.")]
        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("The substances for which background concentration data are to be replaced by focal commodity concentrations.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Concentration value.")]
        [DisplayName("Concentration value")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConcentrationLimit { get; set; }

        [Description("Concentration unit.")]
        [DisplayName("Concentration unit")]
        public string ConcentrationUnitString { get; set; }
    }
}
