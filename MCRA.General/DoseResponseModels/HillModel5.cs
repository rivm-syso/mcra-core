using System;
using System.Collections.Generic;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    /// y = a [1 + (c - 1)x^d/(b^d + x^d)]
    /// </summary>
    public class HillModel5 : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double c;
        private double d;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.Hillm5; }
        }

        public override double Calculate(double dose) {
            return a * (1 + (c - 1) * Math.Pow(dose / s, d) / (Math.Pow(b, d) + Math.Pow(dose / s, d)));
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
            var r1 = Math.Pow(bmd, d) * (a / (bmr - a) * (c - 1) - 1);
            var r2 = Math.Log(r1);
            b = Math.Exp(r2 / d);
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
           return a * (1 + ces);
        }
    }
}
