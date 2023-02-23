namespace MCRA.General.DoseResponseModels {

    /// <summary>
    /// y = a [1 - x^d/(b^d+ x^d)]
    /// </summary>
    public class HillModel3 : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double d;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.Hillm3; }
        }

        public override double Calculate(double dose) {
            var value = a * (1 - Math.Pow(dose / s, d) / (Math.Pow(b, d) + Math.Pow(dose / s, d)));
            if (double.IsNaN(value) && b < 0 && !double.IsNaN(dose)) {
                value = a * (1 - Math.Pow(dose / s, d) / (-Math.Pow(-b, d) + Math.Pow(dose / s, d)));
            }
            return value;
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

        public override void DeriveParameterB(double ced, double bmr) {
            b = Math.Pow(-((bmr - a) * Math.Pow(ced, -d)) / bmr, -1 / d);
            if (double.IsNaN(b)) {
                // Use MathNet numerics
                b = -Math.Pow(-(-((bmr - a) * Math.Pow(ced, -d)) / bmr), -1 / d);
                if (Math.Abs(Calculate(ced) - bmr) > 1e-2) {
                    b = double.NaN;
                }
            }
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            return a * (1 + ces);
        }
    }
}
