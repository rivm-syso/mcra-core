using MCRA.Utils.Statistics;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    ///  y = transform(a * (1 - x^d/(b^d+x^d))
    /// </summary>
    public class HillLatentVariableModel3 : LatentVariableModelBase {
        private double b;
        private double d;
        private double s;
        //private double sigma;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.LVM_Hill_M3; }
        }

        public override double Calculate(double dose) {
            var dose_d = Math.Pow(dose / s, d);
            return NormalDistribution.CDF(0, 1, -Math.Log(a * (1 - dose_d / (Math.Pow(b, d) + dose_d))) / sigma);
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            b = parseParameter(parameters, "b", double.NaN);
            d = parseParameter(parameters, "d");
            s = parseParameter(parameters, "s", 1);
            sigma = parseParameter(parameters, "sigma");
        }

        public override Dictionary<string, double> ExportParameters() {
            return new Dictionary<string, double>() {
                { "a", a },
                { "b", b },
                { "d", d },
                { "sigma", sigma },
                { "s", s },
            };
        }

        public override void DeriveParameterB(double ced, double bmr) {
            var tmp = -NormalDistribution.InvCDF(0, 1, bmr) * sigma - Math.Log(a);
            b = Math.Pow(Math.Pow(ced, d), 1 / d) * Math.Pow(-(Math.Exp(tmp) - 1) / Math.Exp(tmp), -1 / d);
        }
    }
}
