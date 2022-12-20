using System.Collections.Generic;

namespace MCRA.Utils.Statistics.Modelling {
    public sealed class MultipleLinearRegressionResult {
        public double MeanDeviance { get; set; }
        public double Sigma2 { get; set; }
        public int DegreesOfFreedom { get; set; }
        public List<double> RegressionCoefficients { get; set; }
        public List<double> FittedValues { get; set; }
        public List<double> StandardErrors { get; set; }
        public List<double> Residuals { get; set; }
    }
}
