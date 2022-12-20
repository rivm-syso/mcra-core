using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Compiled.Wrappers {

    /// <summary>
    /// Collects for each step in the conversion algorithm the food from to from to information
    /// </summary>
    public sealed class FoodConversionResultStep {
        public FoodConversionStepType Step { get; set; }
        public string FoodCodeFrom { get; set; }
        public string FoodCodeTo { get; set; }
        public bool Finished { get; set; } = false;
        public double Proportion { get; set; } = 1D;
        public double ProportionProcessed { get; set; } = 1D;
        public ICollection<ProcessingType> ProcessingTypes { get; set; }
    }
}
