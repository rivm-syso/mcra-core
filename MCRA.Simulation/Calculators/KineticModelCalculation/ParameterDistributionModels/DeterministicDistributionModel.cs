﻿using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.ParameterDistributionModels {
    public sealed class DeterministicDistributionModel : ProbabilityDistributionModel {
        public override void Initialize(double mean, double sd) {
            mu = mean;
            sigma = sd;
        }

        public override double Sample(IRandom random) {
            return mu;
        }
    }
}
