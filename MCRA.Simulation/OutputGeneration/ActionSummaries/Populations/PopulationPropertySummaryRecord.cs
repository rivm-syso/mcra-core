using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class PopulationPropertySummaryRecord {

        [DisplayName("Population name")]
        public string PopulationName { get; set; }

        [DisplayName("Population code")]
        public string PopulationCode { get; set; }

        [DisplayName("Property name")]
        public string PropertyName { get; set; }

        [DisplayName("Property code")]
        public string PropertyCode { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Location")]
        public string Location { get; set; }

        [DisplayName("Value")]
        public string Value { get; set; }

        [DisplayName("Minimum")]
        public double? MinValue { get; set; }

        [DisplayName("Maximum")]
        public double? MaxValue { get; set; }

        [DisplayName("StartDate")]
        public string StartDate { get; set; }

        [DisplayName("EndDate")]
        public string EndDate { get; set; }

        [DisplayName("Property level")]
        public string PropertyLevel { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }
    }
}
