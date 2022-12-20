using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.IntakeModelling;
using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class UncorrelatedModelResultsSection : SummarySection {
        public double DegreesOfFreedom { get; set; }
        public double _2LogLikelihood { get; set; }
        public List<ParameterEstimates> FrequencyModelEstimates { get; set; }
        public ParameterEstimates VarianceEstimates { get; set; }
        public ParameterEstimates DispersionEstimates { get; set; }
        public LikelihoodRatioTestResults LikelihoodRatioTestResults { get; set; }
        public ErrorMessages Message { get; set; }
    }
}
