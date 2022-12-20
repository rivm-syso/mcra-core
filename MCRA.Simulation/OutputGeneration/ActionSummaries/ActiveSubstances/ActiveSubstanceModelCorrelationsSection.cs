using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ActiveSubstanceModelCorrelationsSection : SummarySection {

        public List<string> ModelCodes { get; set; }
        public List<string> ModelNames { get; set; }
        public List<List<double>> SpearmanCorrelations { get; set; }
        public List<List<double>> PearsonCorrelations { get; set; }

        public void Summarize(ICollection<ActiveSubstanceModel> activeSubstanceModels, HashSet<Compound> substances) {
            ModelCodes = activeSubstanceModels.Select(r => r.Code).ToList();
            ModelNames = activeSubstanceModels.Select(r => r.Name).ToList();
            var membershipGroups = activeSubstanceModels.ToLookup(r => r.Code);

            var dataTable = new List<double[]>();
            for (int i = 0; i < membershipGroups.Count; i++) {
                var model = activeSubstanceModels.ElementAt(i);
                var membershipProbabilities = substances
                    .Select(r => model.MembershipProbabilities.ContainsKey(r) ? model.MembershipProbabilities[r] : double.NaN)
                    .ToArray();
                dataTable.Add(membershipProbabilities);
            }

            var spearmanMatrix = MathNet.Numerics.Statistics.Correlation.SpearmanMatrix(dataTable);
            SpearmanCorrelations = new List<List<double>>();
            for (int i = 0; i < spearmanMatrix.RowCount; i++) {
                var row = Enumerable.Range(0, spearmanMatrix.ColumnCount).Select(j => spearmanMatrix[i, j]).ToList();
                SpearmanCorrelations.Add(row);
            }

            var pearsonMatrix = MathNet.Numerics.Statistics.Correlation.PearsonMatrix(dataTable);
            PearsonCorrelations = new List<List<double>>();
            for (int i = 0; i < pearsonMatrix.RowCount; i++) {
                var row = Enumerable.Range(0, pearsonMatrix.ColumnCount).Select(j => pearsonMatrix[i, j]).ToList();
                PearsonCorrelations.Add(row);
            }
        }
    }
}
