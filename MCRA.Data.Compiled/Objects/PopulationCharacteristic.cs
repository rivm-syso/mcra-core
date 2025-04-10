using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class PopulationCharacteristic {
        public string idPopulation { get; set; }
        public PopulationCharacteristicType Characteristic { get; set; }
        public string Unit { get; set; }
        public PopulationCharacteristicDistributionType DistributionType { get; set; }
        public double Value { get; set; }
        public double? CvVariability { get; set; }
    }
}
