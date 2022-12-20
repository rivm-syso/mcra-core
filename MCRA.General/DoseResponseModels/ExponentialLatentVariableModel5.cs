using MCRA.Utils.Statistics;
using System;
using System.Collections.Generic;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    ///  y = transform(a*(c-(c-1)*exp(-b*x**d)))
    /// </summary>
    public class ExponentialLatentVariableModel5 : LatentVariableModelBase {
        private double b;
        private double c;
        private double d;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.LVM_Exp_M5; }
        }

        public override double Calculate(double dose) {
            return NormalDistribution.CDF(0, 1, -Math.Log(a * (c - (c - 1) * Math.Exp(-b * Math.Pow(dose / s, d)))) / sigma);
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            b = parseParameter(parameters, "b", double.NaN);
            c = parseParameter(parameters, "c");
            d = parseParameter(parameters, "d");
            sigma = parseParameter(parameters, "sigma");
            s = parseParameter(parameters, "s", 1);
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

        public override void DeriveParameterB(double ced, double bmr) {
            b = -Math.Log((Math.Exp(-NormalDistribution.InvCDF(0, 1, bmr) * sigma - Math.Log(a)) - c) / (1 - c)) / Math.Pow(ced, d);
        }
    }
}
