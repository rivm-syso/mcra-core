using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using System.Collections.Generic;

namespace MCRA.Simulation.Actions.DietaryExposures {
    public class DietaryExposuresFactorialResult : IUncertaintyFactorialResult {
        public double[] Percentages { get; set; }
        public List<double> Percentiles { get; set; }
    }
}
