namespace MCRA.General.DoseResponseModels {

    /// <summary>
    /// y = a exp(bx^d)
    /// </summary>
    public class ExponentialModel3 : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double d;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.Expm3; }
        }

        public override double Calculate(double dose) {
            return a * Math.Exp(b * Math.Pow(dose / s, d));
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            b = parseParameter(parameters, "b", double.NaN);
            d = parseParameter(parameters, "d");
            s = parseParameter(parameters, "s", 1);
        }

        public override Dictionary<string, double> ExportParameters() {
            return new Dictionary<string, double>() {
                { "a", a },
                { "b", b },
                { "d", d },
                { "s", s },
            };
        }
        public override void DeriveParameterB(double bmd, double bmr) {
            b = Math.Log(bmr / a) / Math.Pow(bmd, d);
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            return a * (1 + ces);
        }
    }
}
