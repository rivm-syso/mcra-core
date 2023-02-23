using MCRA.Utils.Statistics;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    ///  y = transform(a * (1 + (c-1)*x**d/(b**d+x**d)))
    /// </summary>
    public class HillLatentVariableModel5 : LatentVariableModelBase {
        private double b;
        private double c;
        private double d;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.LVM_Hill_M5; }
        }

        public override double Calculate(double dose) {
            var dose_d = Math.Pow(dose / s, d);
            return NormalDistribution.CDF(0, 1, -Math.Log(a * (1 + (c - 1) * dose_d / (Math.Pow(b, d) + dose_d))) / sigma);
        }

        public override void DeriveParameterB(double ced, double bmr) {
            var tmp = -NormalDistribution.InvCDF(0, 1, bmr) * sigma - Math.Log(a);
            b = Math.Exp(Math.Log(Math.Pow(ced, d) * (c - 1) / ((Math.Exp(tmp) - 1)) - Math.Pow(ced, d)) / d);
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            b = parseParameter(parameters, "b", double.NaN);
            c = parseParameter(parameters, "c");
            d = parseParameter(parameters, "d");
            s = parseParameter(parameters, "s", 1);
            sigma = parseParameter(parameters, "sigma");
        }

        public override Dictionary<string, double> ExportParameters() {
            return new Dictionary<string, double>() {
                { "a", a },
                { "b", b },
                { "c", c },
                { "d", d },
                { "sigma", sigma },
                { "s", s },
            };
        }
    }
}
