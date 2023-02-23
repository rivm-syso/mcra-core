using MCRA.Utils;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Stores data to fit the frequency model for chronic exposure models
    /// </summary>
    public class FrequencyDataResult {
        public List<double> Ybin { get; set; }
        public List<double> Nbin { get; set; }
        public List<double> Weights { get; set; }
        public double[,] X { get; set; }
        public List<string> Cofactor { get; set; }
        public List<double> Covariable { get; set; }
        public List<string> DesignMatrixDescription { get; set; }
        public Polynomial PolynomialResult { get; set; }
        public List<int> GroupCounts { get; set; }
        public int DfPolynomial { get; set; }
        public List<int> IdIndividual { get; set; }
    }
}
