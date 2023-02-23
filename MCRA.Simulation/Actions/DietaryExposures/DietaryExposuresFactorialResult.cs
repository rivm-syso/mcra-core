using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.DietaryExposures {
    public class DietaryExposuresFactorialResult : IUncertaintyFactorialResult {
        public double[] Percentages { get; set; }
        public List<double> Percentiles { get; set; }
    }
}
