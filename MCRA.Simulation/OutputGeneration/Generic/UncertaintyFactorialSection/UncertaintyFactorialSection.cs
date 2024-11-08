using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UncertaintyFactorialSection : SummarySection {
        public List<string> UncertaintySources { get; set; }
        public List<double> Percentages { get; set; }
        public List<double> ExplainedVariance = [];
        public List<List<double>> Contributions = [];
        public List<List<double>> RegressionCoefficients = [];

        public List<List<double>> Responses { get; set; }
        public List<List<double>> Design { get; set; }

        public List<string> ResponseNames = [];

        public void Summarize(List<PercentilesUncertaintyFactorialResult> linearModels, IEnumerable<double> percentages) {
            UncertaintySources = linearModels.First().UncertaintySources;
            Percentages = percentages.ToList();
            Responses = [];
            var ix = 0;
            foreach (var linearModel in linearModels) {
                if (!double.IsNaN(linearModel.ExplainedVariance)) {
                    ExplainedVariance.Add(linearModel.ExplainedVariance);
                    Contributions.Add(linearModel.Contributions);
                    RegressionCoefficients.Add(linearModel.RegressionCoefficients);
                    Responses.Add(linearModel.Response);
                    ResponseNames.Add(Percentages[ix].ToString("F2"));
                }
                ix++;
            }

            Design = [];
            var row = linearModels.First().DesignMatrix.GetLength(0);
            var col = linearModels.First().DesignMatrix.GetLength(1);
            for (int c = 0; c < col; c++) {
                var design = new List<double>();
                for (int r = 0; r < row; r++) {
                    design.Add(linearModels.First().DesignMatrix[r, c]);
                }
                Design.Add(design);
            }
        }
    }
}
