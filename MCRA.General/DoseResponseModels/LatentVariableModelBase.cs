using MCRA.Utils.Statistics;

namespace MCRA.General.DoseResponseModels {

    public abstract class LatentVariableModelBase : DoseResponseModelFunctionBase {

        public double a { get; set; }
        public double sigma { get; set; }

        public LatentVariableModelBase() {
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            var background = NormalDistribution.CDF(0, 1, -Math.Log(a) / sigma);
            return riskType switch {
                RiskType.Ed50 => 0.5,
                RiskType.AdditionalRisk => background + ces,
                RiskType.ExtraRisk => (1 - background) * ces + background,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
