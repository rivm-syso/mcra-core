using System.Collections.Generic;

namespace MCRA.Data.Compiled.Objects {
    public sealed class DietaryExposurePercentile {
        public double Percentage { get; set; }
        public double Exposure { get; set; }
        public List<double> ExposureUncertainties { get; set; }
    }
}
