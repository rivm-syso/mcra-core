using MCRA.Utils.Statistics;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    /// y = transform(a*exp(b*x) ) (log(th - zz)/s
    /// </summary>
    public class ExponentialLatentVariableModel2 : LatentVariableModelBase {

        public double b { get; set; }
        public double s { get; set; }

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.LVM_Exp_M2; }
        }

        public override double Calculate(double dose) {
            return NormalDistribution.CDF(0, 1, -Math.Log(a * Math.Exp(b * dose / s)) / sigma);
        }

        public override void DeriveParameterB(double ced, double bmr) {
            b = (-NormalDistribution.InvCDF(0, 1, bmr) * sigma - Math.Log(a)) / ced;
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            b = parseParameter(parameters, "b", double.NaN);
            sigma = parseParameter(parameters, "sigma");
            s = parseParameter(parameters, "s", 1);
        }

        public override Dictionary<string, double> ExportParameters() {
            return new Dictionary<string, double>() {
                { "a", a },
                { "b", b },
                { "sigma", sigma },
                { "s", s },
            };
        }
    }
}
