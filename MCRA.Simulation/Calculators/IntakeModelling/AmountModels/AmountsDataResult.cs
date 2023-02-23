using MCRA.Utils;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public sealed class AmountDataResult {
        public List<int> IndividualIds { get; set; }
        public List<Individual> Individuals { get; set; }
        public List<double> IndividualSamplingWeights { get; set; }
        public List<double> Ys { get; set; }
        public double[,] X { get; set; }
        public List<string> Cofactors { get; set; }
        public List<double> Covariables { get; set; }
        public List<string> DesignMatrixDescriptions { get; set; }
        public Polynomial PolynomialResult { get; set; }
        public List<int> GroupCounts { get; set; }
        public List<double> Amounts { get; set; }
        public List<int> NDays { get; set; }
        public int DfPolynomial { get; set; }
    }
}
