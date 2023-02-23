using MCRA.Utils.Statistics;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    ///  y = transform(y = a*[c-(c-1)exp(-bx)])
    /// </summary>
    public class ExponentialLatentVariableModel4 : LatentVariableModelBase {
        private double b;
        private double c;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.LVM_Exp_M4; }
        }

        public override double Calculate(double dose) {
            return NormalDistribution.CDF(0, 1, -Math.Log(a * (c - (c - 1) * Math.Exp(-b * dose / s))) / sigma);
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            b = parseParameter(parameters, "b", double.NaN);
            c = parseParameter(parameters, "c");
            sigma = parseParameter(parameters, "sigma");
            s = parseParameter(parameters, "s", 1);
        }

        public override Dictionary<string, double> ExportParameters() {
            return new Dictionary<string, double>() {
                { "a", a },
                { "b", b },
                { "c", c },
                { "sigma", sigma },
                { "s", s },
            };
        }

        public override void DeriveParameterB(double ced, double bmr) {
            b = -Math.Log((Math.Exp(-NormalDistribution.InvCDF(0, 1, bmr) * sigma - Math.Log(a)) - c) / (1 - c)) / ced;
        }
    }
}
