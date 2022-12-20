using System;
using System.Collections.Generic;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    /// y = a exp(bx)
    /// </summary>
    public class ExponentialModel2 : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.Expm2; }
        }

        public override double Calculate(double dose) {
            return a * Math.Exp(b * dose / s);
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
            b = Math.Log(bmr / a) / bmd;
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            return a * (1 + ces);
        }
    }
}
