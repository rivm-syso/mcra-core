using MCRA.Utils.Statistics;

namespace MCRA.General.DoseResponseModels {

    public abstract class LatentVariableModelBase : DoseResponseModelFunctionBase {

        public double a { get; set; }
        public double sigma { get; set; }

        public LatentVariableModelBase() {
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            var background = NormalDistribution.CDF(0, 1, -Math.Log(a) / sigma);
            switch (riskType) {
                case RiskType.Ed50:
                    return 0.5;
                case RiskType.AdditionalRisk:
                    return background + ces;
                case RiskType.ExtraRisk:
                    return (1 - background) * ces + background;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
