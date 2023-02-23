namespace MCRA.General.DoseResponseModels {

    /// <summary>
    /// Exponential model 5: y = a [c- (c- 1)exp(-bx^d)]
    /// </summary>
    public class ExponentialModel5 : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double c;
        private double d;
        private double s;

        public ExponentialModel5() { }

        public ExponentialModel5(double a, double b, double c, double d, double s)
            : this() {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.s = s;
        }

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.Expm5; }
        }

        public override double Calculate(double dose) {
            return a * (c - (c - 1) * Math.Exp(-b * Math.Pow(dose / s, d)));
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            b = parseParameter(parameters, "b", double.NaN);
            c = parseParameter(parameters, "c");
            d = parseParameter(parameters, "d");
            s = parseParameter(parameters, "s", 1);
        }

        public override Dictionary<string, double> ExportParameters() {
            return new Dictionary<string, double>() {
                { "a", a },
                { "b", b },
                { "c", c },
                { "d", d },
                { "s", s },
            };
        }

        public override void DeriveParameterB(double bmd, double bmr) {
            b = Math.Pow(bmd / s, -d) * Math.Log((a * c - a) / (a * c - bmr));
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            return a * (1 + ces);
        }
    }
}
