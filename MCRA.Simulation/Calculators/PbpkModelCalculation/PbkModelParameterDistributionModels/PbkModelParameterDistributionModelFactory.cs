using MCRA.General;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation.PbkModelParameterDistributionModels {
    public sealed class PbkModelParameterDistributionModelFactory {

        public static PbkModelParameterDistributionModel Create(
            PbkModelParameterDistributionType distribution
        ) {
            PbkModelParameterDistributionModel model = null;
            switch (distribution) {
                case PbkModelParameterDistributionType.LogNormal:
                    model = new PbkModelParameterLogNormalDistributionModel();
                    break;
                case PbkModelParameterDistributionType.Normal:
                    model = new PbkModelParameterNormalDistributionModel();
                    break;
                case PbkModelParameterDistributionType.LogisticNormal:
                    model = new PbkModelParameterLogisticDistributionModel();
                    break;
                case PbkModelParameterDistributionType.Deterministic:
                case PbkModelParameterDistributionType.Unspecified:
                    model = new PbkModelParameterDeterministicModel();
                    break;
            }
            return model;
        }
    }
}
