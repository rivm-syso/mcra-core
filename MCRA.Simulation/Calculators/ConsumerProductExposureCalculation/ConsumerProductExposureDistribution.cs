using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureCalculation {

    public class ConsumerProductExposureDistribution {
        public string ExposureSetCode { get; set; }
        public ExposureRoute ExposureRoute { get; set; }
        public Compound Substance { get; set; }
        public double? AgeMin { get; set; }
        public double? AgeMax { get; set; }
        public GenderType Gender { get; set; }

        public LogNormalDistribution Distribution { get; set; }
        public double FractionZeros { get; set; }
        public ConsumerProductExposureEstimateUnit Unit { get; set; }

        public double Draw(IRandom random) {
            var result = random.NextDouble() > FractionZeros
                ? Distribution.Draw(random) : 0;
            return result;
        }
    }
}
