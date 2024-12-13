namespace MCRA.General.DoseResponseModels {

    /// <summary>
    ///  y = a+(1-a)/(1+exp(c*ln(b/x)))
    /// </summary>
    public class LogLogistModel : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double c;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.LogLogist; }
        }

        public override double Calculate(double dose) {
            return a + (1 - a) / (1 + Math.Exp(c * Math.Log(b / (dose / s))));
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
            var l = (bmr - 1) / (a - bmr);
            b = bmd * Math.Pow(l, 1 / c);
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            return riskType switch {
                RiskType.Ed50 => 0.5,
                RiskType.AdditionalRisk => a + ces,
                RiskType.ExtraRisk => (1 - a) * ces + a,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
