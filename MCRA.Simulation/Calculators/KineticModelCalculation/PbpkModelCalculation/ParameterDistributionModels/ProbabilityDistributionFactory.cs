using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ParameterDistributionModels {
    public sealed class ProbabilityDistributionFactory {

        public static ProbabilityDistributionModel createProbabilityDistributionModel(ProbabilityDistribution distribution) {
            ProbabilityDistributionModel model = null;
            switch (distribution) {
                case ProbabilityDistribution.LogNormal:
                    model = new LogNormalDistributionModel();
                    break;
                case ProbabilityDistribution.Normal:
                    model = new NormalDistributionModel();
                    break;
                case ProbabilityDistribution.LogisticNormal:
                    model = new LogisticDistributionModel();
                    break;
                case ProbabilityDistribution.Deterministic:
                    model = new DeterministicDistributionModel();
                    break;
            }
            return model;
        }
    }
}
