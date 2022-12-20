﻿using MCRA.Utils.Statistics;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public sealed class FrequencyModelSummary {
        public List<ParameterEstimates> FrequencyModelEstimates { get; set; }
        public ParameterEstimates DispersionEstimates { get; set; }
        public double DegreesOfFreedom { get; set; }
        public double _2LogLikelihood { get; set; }
        public LikelihoodRatioTestResults LikelihoodRatioTestResults { get; set; }
        public ErrorMessages ErrorMessage { get; set; } = ErrorMessages.NoConvergence;
    }
}
