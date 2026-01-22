using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.IntakeModelling.FrequencyAmountsModels {
    public sealed class LNNModelResult {
        public LNNParameters Parameters { get; set; }
        public double LogLik { get; set; }
        public ErrorMessages ErrorMessages { get; set; }

        public LNNParameters StandardErrors { get; set; }
    }
}
