using MCRA.Utils.Statistics;
using System;
using System.Collections.Generic;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    ///  y = transform(a*exp(b*x^d))
    /// </summary>
    public class ExponentialLatentVariableModel3 : LatentVariableModelBase {

        public double b { get; set; }
        public double d { get; set; }
        public double s { get; set; }

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.LVM_Exp_M3; }
        }

        public override double Calculate(double dose) {
            return NormalDistribution.CDF(0, 1, -Math.Log(a * Math.Exp(b * Math.Pow(dose / s, d))) / sigma);
        }

        public override void DeriveParameterB(double ced, double bmr) {
            b = (-NormalDistribution.InvCDF(0, 1, bmr) * sigma - Math.Log(a)) / Math.Pow(ced, d);
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            b = parseParameter(parameters, "b", double.NaN);
            d = parseParameter(parameters, "d");
            sigma = parseParameter(parameters, "sigma");
            s = parseParameter(parameters, "s", 1);
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
    }
}
