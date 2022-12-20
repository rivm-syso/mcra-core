using System.ComponentModel;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConversionStepRecord {

        [DisplayName("Conversion step")]
        public FoodConversionStepType Step { get; set; }

        [DisplayName("Food code from")]
        public string FoodCodeFrom { get; set; }

        [DisplayName("Food code to")]
        public string FoodCodeTo { get; set; }

    }
}
