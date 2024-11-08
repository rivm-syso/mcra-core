namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Combines cofactor and covariate levels for the frequency and amount model
    /// </summary>
    public class CovariatesCollection {
        private string cofactor = string.Empty;
        private double covariable = double.NaN;
        private string cofactorName = string.Empty;
        private string covariableName = string.Empty;

        public string FrequencyCofactor { get; set; }
        public double FrequencyCovariable { get; set; }
        public string AmountCofactor { get; set; }
        public double AmountCovariable { get; set; }
        public string CofactorName { get; set; }
        public string CovariableName { get; set; }

        public string OverallCofactor {
            get {
                if (AmountCofactor != string.Empty) {
                    cofactor = AmountCofactor;
                }
                if (FrequencyCofactor != string.Empty) {
                    cofactor = FrequencyCofactor;
                }
                return cofactor;
            }
        }

        public double OverallCovariable {
            get {
                if (!double.IsNaN(AmountCovariable)) {
                    covariable = AmountCovariable;
                }
                if (!double.IsNaN(FrequencyCovariable)) {
                    covariable = FrequencyCovariable;
                }
                return covariable;
            }
        }
    }
}
