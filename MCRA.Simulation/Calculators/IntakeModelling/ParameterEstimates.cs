namespace MCRA.Simulation.Calculators.IntakeModelling {
    /// <summary>
    /// Summarizes an estimate, the corresponding standard error, parameter name and tvalue
    /// </summary>
    public class ParameterEstimates {
        public string ParameterName { get; set; }
        public double Estimate { get; set; }
        public double StandardError { get; set; }
        public double TValue {
            get { return Estimate / StandardError; }
        }
    }
}
