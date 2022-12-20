using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.ComponentCalculation.Component {

    /// <summary>
    /// Represent an element of a component description.
    /// </summary>
    public class ComponentRecord {
        public Compound Compound { get; set; }
        public double Mu { get; set; }
        public double Sigma { get; set; }
    }
}
