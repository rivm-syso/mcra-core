using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation {
    public sealed class PercentilesUncertaintyFactorialCalculator {

       public List<PercentilesUncertaintyFactorialResult> Compute(
           List<(List<UncertaintySource> Tags, List<double> Percentiles)> factorialPercentilesResults,
           double[] percentages,
           List<string> uncertaintySources,
           double[,] designMatrix
        ) {
           var linearModels = new List<PercentilesUncertaintyFactorialResult>();
           for (int i = 0; i < percentages.Length; i++) {
               var uncertaintyFactorialResults = factorialPercentilesResults
                    .Select(c => (
                       uncertaintyGroup: string.Join("+", c.Tags),
                       value: c.Percentiles
                    ))
                   .GroupBy(gr => gr.uncertaintyGroup)
                   .Select(c => (
                       value: c.Select(a => Math.Log(a.value.ElementAt(i))).Variance(),
                       source: c.Key
                   ))
                   .ToList();
               var linearModel = new PercentilesUncertaintyFactorialResult() {
                   Response = uncertaintyFactorialResults.Select(c => c.value).ToList(),
                   UncertaintySources = uncertaintySources,
                   DesignMatrix = designMatrix,
               };
               linearModel.Calculate();
               linearModels.Add(linearModel);
           }
           return linearModels;
       }
    }
}
