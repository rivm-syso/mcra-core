namespace MCRA.General.DoseResponseModels {

    /// <summary>
    ///  y = a + (1-a)(1-exp(-(x/b)^c))
    /// </summary>
    public class WeibullModel : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double c;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.Weibull; }
        }

        public override double Calculate(double dose) {
            return a + (1 - a) * (1 - Math.Exp(-Math.Pow(dose / b / s, c)));
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
            var l = Math.Log((1 - a) / (1 - bmr));
            b = bmd / Math.Exp(Math.Log(l) / c);
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            switch (riskType) {
                case RiskType.Ed50:
                    return 0.5;
                case RiskType.AdditionalRisk:
                    return a + ces;
                case RiskType.ExtraRisk:
                    return (1 - a) * ces + a;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
