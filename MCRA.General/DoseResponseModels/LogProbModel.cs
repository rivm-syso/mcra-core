using MCRA.Utils.Statistics;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    ///  y = a+(1-a)*pnorm(c*ln(x/b))
    /// </summary>
    public class LogProbModel : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double c;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.LogProb; }
        }

        public override double Calculate(double dose) {
            return a + (1 - a) * NormalDistribution.CDF(0, 1, c * Math.Log(dose / b / s));
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            b = parseParameter(parameters, "b", double.NaN);
            c = parseParameter(parameters, "c");
            s = parseParameter(parameters, "s", 1);
        }

        public override Dictionary<string, double> ExportParameters() {
            return new Dictionary<string, double>() {
                { "a", a },
                { "b", b },
                { "c", c },
                { "s", s },
            };
        }

        public override void DeriveParameterB(double bmd, double bmr) {
            b = bmd / Math.Exp(NormalDistribution.InvCDF(0, 1, (bmr - a) / (1 - a)) / c);
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            var backGround = a;

            return riskType switch {
                RiskType.Ed50 => 0.5,
                RiskType.AdditionalRisk => ces + backGround,
                RiskType.ExtraRisk => (1 - backGround) * ces + backGround,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
