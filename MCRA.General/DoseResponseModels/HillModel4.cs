﻿namespace MCRA.General.DoseResponseModels {

    /// <summary>
    /// y = a [1 + (c - 1)x/(b + x)]
    /// </summary>
    public class HillModel4 : DoseResponseModelFunctionBase {
        private double a;
        private double b;
        private double c;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.Hillm4; }
        }

        public override double Calculate(double dose) {
            return a * (1 + (c - 1) * dose / s / (b + dose / s));
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
            b = (bmd * bmr - bmd * a * c) / (a - bmr);
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            return a * (1 + ces);
        }
    }
}
