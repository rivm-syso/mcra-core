using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.ComponentCalculation.Component {

    /// <summary>
    /// A component, described as a collection of component records, i.e.,
    /// substances that are part of the component.
    /// </summary>
    public class Component {
        public double Fraction { get; set; }
        public IDictionary<Compound, ComponentRecord> ComponentRecords { get; set; }
    }
}
