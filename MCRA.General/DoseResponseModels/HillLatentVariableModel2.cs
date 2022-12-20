using MCRA.Utils.Statistics;
using System;
using System.Collections.Generic;

namespace MCRA.General.DoseResponseModels {

    /// <summary>
    ///  y = transform(a * (1 - x/(b+x)))
    /// </summary>
    public class HillLatentVariableModel2 : LatentVariableModelBase {
        private double b;
        private double s;

        public override DoseResponseModelType Type {
            get { return DoseResponseModelType.LVM_Hill_M2; }
        }

        public override double Calculate(double dose) {
            return NormalDistribution.CDF(0, 1, -Math.Log(a * (1 - dose / s / (b + dose / s))) / sigma);
        }

        public override void Init(IDictionary<string, double> parameters) {
            a = parseParameter(parameters, "a");
            b = parseParameter(parameters, "b", double.NaN);
            s = parseParameter(parameters, "s", 1);
            sigma = parseParameter(parameters, "sigma");
        }

        public override Dictionary<string, double> ExportParameters() {
            return new Dictionary<string, double>() {
                { "a", a },
                { "b", b },
                { "s", s },
            };
        }

        public override void DeriveParameterB(double ced, double bmr) {
            var tmp = -NormalDistribution.InvCDF(0, 1, bmr) * sigma - Math.Log(a);
            b = ced / (1 - Math.Exp(tmp)) - ced;
        }
    }
}
