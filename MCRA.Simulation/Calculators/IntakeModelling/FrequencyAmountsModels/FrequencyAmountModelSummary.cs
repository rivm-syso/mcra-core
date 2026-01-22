using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public class FrequencyAmountModelSummary {

        private ErrorMessages _errorMessage = ErrorMessages.Convergence;
        public ParameterEstimates CorrelationEstimates { get; set; }
        public List<ParameterEstimates> FrequencyModelEstimates { get; set; }
        public ParameterEstimates DispersionEstimates { get; set; }
        public double DegreesOfFreedomFrequencies { get; set; }
        public double DegreesOfFreedomAmounts { get; set; }
        public double _2LogLikelihood { get; set; }
        public double Power { get; set; }
        public ParameterEstimates VarianceBetween { get; set; }
        public ParameterEstimates VarianceWithin { get; set; }
        public List<ParameterEstimates> AmountModelEstimates { get; set; }
        public ErrorMessages ErrorMessage {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }
    }
}
