using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.TargetExposures {
    public sealed class TargetExposuresFactorialResult : IUncertaintyFactorialResult {
        public double[] Percentages { get; set; }
        public List<double> Percentiles { get; set; }
    }
}
