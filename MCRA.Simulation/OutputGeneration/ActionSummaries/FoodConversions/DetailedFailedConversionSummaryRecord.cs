using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DetailedFailedConversionSummaryRecord {

        [DisplayName("Modelled food ")]
        public string FoodAsMeasuredCode { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

    }

}
