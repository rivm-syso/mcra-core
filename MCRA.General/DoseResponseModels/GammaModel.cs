using MathNet.Numerics.Distributions;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    /// y = pgamma(b*x; c)
    /// </summary>

    public class GammaModel : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double c;
        private double s;

        public GammaModel() { }

        public GammaModel(double a, double b, double c, double s)
            : this() {
            this.a = a;
            this.b = b;
            this.c = c;
            this.s = s;
        }

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.Gamma; }
        }

        public override double Calculate(double dose) {
            return a + (1 - a) * Gamma.CDF(c, 1, dose * b);
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
            b = Gamma.InvCDF(c, 1, (bmr - a)/(1-a)) / bmd;
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
