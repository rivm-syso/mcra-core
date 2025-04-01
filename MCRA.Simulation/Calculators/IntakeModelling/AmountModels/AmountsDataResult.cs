using MCRA.Simulation.Objects;
using MCRA.Utils;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public sealed class AmountDataResult {
        public List<SimulatedIndividual> SimulatedIndividuals { get; set; }
        public List<double> IndividualSamplingWeights => SimulatedIndividuals?.Select(s => s.SamplingWeight).ToList();
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
