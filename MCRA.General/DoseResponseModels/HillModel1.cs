using System;
using System.Collections.Generic;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    /// y = a 
    /// </summary>
    public class HillModel1 : DoseResponseModelFunctionBase {

        private double a;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.Hillm1; }
        }

        public override double Calculate(double dose) {
            return a / s;
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            s = parseParameter(parameters, "s", 1);
        }

        public override Dictionary<string, double> ExportParameters() {
            return new Dictionary<string, double>() {
                { "a", a },
                { "s", s },
            };
        }
        public override void DeriveParameterB(double ced, double ces) {
            // No parameter b in this model
        }

        public override double ComputeBmr(double ced, double ces, RiskType riskType) {
            throw new NotImplementedException();
        }
    }
}
