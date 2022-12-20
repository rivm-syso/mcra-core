using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ProcessedModelledFoodConsumptionDataRecord : ModelledFoodConsumptionDataRecord {
        [Display(Name = "Processing type name", Order = 3)]
        public string ProcessingTypeName { get; set; }

        [Display(Name = "Processing type code", Order = 4)]
        public string ProcessingTypeCode { get; set; }

        [Description("Weight correction factor / reverse yield factor processing.")]
        [Display(Name = "Proportion processing", Order = 5)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ProportionProcessing { get; set; }

    }
}
