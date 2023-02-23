namespace MCRA.Utils.Statistics {
    public class LikelihoodRatioTestResults {
        public List<int> DfPolynomial { get; set; }
        public List<double> LogLikelihood { get; set; }
        public List<int> DegreesOfFreedom { get; set; }
        public List<double> DeltaChi { get; set; }
        public List<int> DeltaDf { get; set; }
        public List<double> PValue { get; set; }
        public int IndexSelectedModel { get; set; }
        public int SelectedOrder { get; set; }
    }
}