using MCRA.Utils.Statistics;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    ///  y = pnorm(a +bx))
    /// </summary>
    public class ProbitModel : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.Probit; }
        }

        public override double Calculate(double dose) {
            return NormalDistribution.CDF(0, 1, a + b * dose / s);
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            b = parseParameter(parameters, "b", double.NaN);
            s = parseParameter(parameters, "s", 1);
        }

        public override Dictionary<string, double> ExportParameters() {
            return new Dictionary<string, double>() {
                { "a", a },
                { "b", b },
                { "s", s },
            };
        }

        public override void DeriveParameterB(double bmd, double bmr) {
            b = (NormalDistribution.InvCDF(0, 1, bmr) - a) / bmd;
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            var backGround = NormalDistribution.CDF(0, 1, a);
            switch (riskType) {
                case RiskType.Ed50:
                    return 0.5;
                case RiskType.AdditionalRisk:
                    return backGround + ces;
                case RiskType.ExtraRisk:
                    return (1 - backGround) * ces + backGround;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
