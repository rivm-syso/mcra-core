using System.Collections.Generic;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public class FrequencyAmountModelSummary {
        public ParameterEstimates CorrelationEstimates { get; set; }
        public List<ParameterEstimates> FrequencyModelEstimates { get; set; }
        public ParameterEstimates DispersionEstimates { get; set; }
        public double DegreesOfFreedom { get; set; }
        public double _2LogLikelihood { get; set; }
        public double Power { get; set; }

        public double VarianceBetween { get; set; }
        public double VarianceWithin { get; set; }
        public List<double> Residuals { get; set; }
        public List<double> Blups { get; set; }
        public List<ParameterEstimates> AmountModelEstimates { get; set; }
        public IntakeTransformer IntakeTransformer { get; set; }

        private ErrorMessages errorMessage = ErrorMessages.Convergence;
        public ErrorMessages ErrorMessage {
            get { return errorMessage; }
            set { errorMessage = value; }
        }
    }
}
