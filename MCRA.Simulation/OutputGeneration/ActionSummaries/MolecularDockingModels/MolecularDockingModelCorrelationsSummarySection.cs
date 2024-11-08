using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MolecularDockingModelCorrelationsSummarySection : SummarySection {

        public List<string> ModelCodes { get; set; }
        public List<string> ModelNames { get; set; }
        public List<List<double>> SpearmanCorrelations { get; set; }
        public List<List<double>> PearsonCorrelations { get; set; }

        public void Summarize(ICollection<MolecularDockingModel> molecularDockingModels, HashSet<Compound> compounds) {
            ModelCodes = molecularDockingModels.Select(r => r.Code).ToList();
            ModelNames = molecularDockingModels.Select(r => r.Name).ToList();
            var records = molecularDockingModels
                .Select(r => new {
                    BindingEnergies = r.BindingEnergies
                        .Where(e => compounds.Contains(e.Key))
                        .Select(e => new MolecularDockingModelCompoundRecord() {
                            SubstanceCode = e.Key.Code,
                            SubstanceName = e.Key.Name,
                            BindingEnergy = e.Value
                        })
                        .ToList(),
                })
                .ToList();

            if (records?.Count > 0) {
                var substances = records.SelectMany(r => r.BindingEnergies)
                    .GroupBy(r => r.SubstanceCode)
                    .Where(g => g.Count() == records.Count && !g.Any(r => double.IsNaN(r.BindingEnergy)));
                var dataTable = new List<double[]>();
                foreach (var model in records) {
                    var bindingEnergiesDict = model.BindingEnergies.ToDictionary(r => r.SubstanceCode);
                    var bindingEnergies = substances
                        .Select(r => bindingEnergiesDict.ContainsKey(r.Key) ? bindingEnergiesDict[r.Key].BindingEnergy : double.NaN)
                        .ToArray();
                    dataTable.Add(bindingEnergies);
                }
                var spearmanMatrix = MathNet.Numerics.Statistics.Correlation.SpearmanMatrix(dataTable);
                SpearmanCorrelations = [];
                for (int i = 0; i < spearmanMatrix.RowCount; i++) {
                    var row = Enumerable.Range(0, spearmanMatrix.ColumnCount).Select(j => spearmanMatrix[i, j]).ToList();
                    SpearmanCorrelations.Add(row);
                }
                var pearsonMatrix = MathNet.Numerics.Statistics.Correlation.PearsonMatrix(dataTable);
                PearsonCorrelations = [];
                for (int i = 0; i < pearsonMatrix.RowCount; i++) {
                    var row = Enumerable.Range(0, pearsonMatrix.ColumnCount).Select(j => pearsonMatrix[i, j]).ToList();
                    PearsonCorrelations.Add(row);
                }
            }
        }
    }
}
