using System;
using System.Collections.Generic;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    ///  y = a + (1-a)(1-exp(-(x/b)-c(x/b)^2))
    /// </summary>
    public class TwoStageModel : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double c;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.TwoStage; }
        }

        public override double Calculate(double dose) {
            return a + (1 - a) * (1 - Math.Exp(-dose / b / s - c * Math.Pow(dose / b / s, 2)));
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
            if (c > 0) {
                var l = Math.Log((1 - bmr) / (1 - a));
                var sqrtD = Math.Sqrt(1 - 4 * c * l);
                var b1 = (-2 * c * bmd) / (1 + sqrtD);
                var b2 = (-2 * c * bmd) / (1 - sqrtD);
                b = Math.Max(b1, b2);
            } else {
                var l = Math.Log((1 - bmr) / (1 - a));
                var D = Math.Pow(bmd / l, 2) + 4 * c * Math.Pow(bmd, 2) / l;
                var b1 = (-bmd / l + Math.Sqrt(D)) / 2;
                var b2 = (-bmd / l - Math.Sqrt(D)) / 2;
                b = Math.Max(b1, b2);
            }
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
