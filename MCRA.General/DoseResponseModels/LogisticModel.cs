using System;
using System.Collections.Generic;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    /// Logistic model: y = 1/(1+exp(-a-bx)).
    /// </summary>
    public class LogisticModel : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double s;

        public LogisticModel() { }

        public LogisticModel(double a, double b, double s) {
            this.a = a;
            this.b = b;
            this.s = s;
        }

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.Logistic; }
        }

        public override double Calculate(double dose) {
            return 1 / (1 + Math.Exp(-a - b * dose / s));
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            b = parseParameter(parameters, "b", double.NaN);
            s = parseParameter(parameters, "s", 1);
        }

        public override Dictionary<string, double> ExportParameters() {
            return new Dictionary<string, double>() {
                { "a", a },
                { "b", b },
                { "s", s },
            };
        }

        public override void DeriveParameterB(double bmd, double bmr) {
            var l = Math.Log((1 - bmr) / bmr);
            b = (-a - l) / bmd;
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            var backGround = 1 / (1 + Math.Exp(-a));
            switch (riskType) {
                case RiskType.Ed50:
                    return 0.5;
                case RiskType.AdditionalRisk:
                    return backGround + ces;
                case RiskType.ExtraRisk:
                    return (1 - backGround) * ces + backGround;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
