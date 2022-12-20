using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class QsarMembershipModelCorrelationsSection : SummarySection {

        public List<string> ModelCodes { get; set; }
        public List<string> ModelNames { get; set; }
        public List<List<double>> PearsonCorrelations { get; set; }

        public void Summarize(ICollection<QsarMembershipModel> qsarMembershipModels, HashSet<Compound> compounds) {
            ModelCodes = qsarMembershipModels.Select(r => r.Code).ToList();
            ModelNames = qsarMembershipModels.Select(r => r.Name).ToList();
            var records = qsarMembershipModels
                .Select(r => new {
                    MembershipScores = r.MembershipScores
                        .Where(e => compounds.Contains(e.Key))
                        .Select(e => new QsarMembershipModelSubstanceRecord() {
                            SubstanceCode = e.Key.Code,
                            SubstanceName = e.Key.Name,
                            MembershipScore = e.Value
                        })
                        .ToList(),
                })
                .ToList();
            if (records?.Any() ?? false) {
                var substances = records.SelectMany(r => r.MembershipScores)
                    .GroupBy(r => r.SubstanceCode)
                    .Where(g => g.Count() == records.Count && !g.Any(r => double.IsNaN(r.MembershipScore)))
                    .ToList();
                var dataTable = new List<double[]>();
                foreach (var model in records) {
                    var membershipsDict = model.MembershipScores.ToDictionary(r => r.SubstanceCode);
                    var membershipScores = substances
                        .Select(r => membershipsDict.ContainsKey(r.Key) ? membershipsDict[r.Key].MembershipScore : double.NaN)
                        .ToArray();
                    dataTable.Add(membershipScores);
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
}
