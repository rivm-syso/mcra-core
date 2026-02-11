using MCRA.Data.Compiled.Interfaces;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Compiled.Objects {
    public sealed class AirConcentrationDistribution : IConcentrationDistribution {
        public string IdDistribution { get; set; }
        public Compound Substance { get; set; }
        public double Mean { get; set; }
        public AirConcentrationUnit Unit { get; set; } = AirConcentrationUnit.ugPerm3;
        public AirConcentrationDistributionType DistributionType { get; set; }
        public double? CvVariability { get; set; }
        public double? OccurrencePercentage { get; set; }
    }
}
