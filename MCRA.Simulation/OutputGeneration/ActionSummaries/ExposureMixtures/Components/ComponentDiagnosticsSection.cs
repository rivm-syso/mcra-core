using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Root Mean Square Error (RMSE) and ratio RMSE calculation based on difference
    /// </summary>
    public sealed class ComponentDiagnosticsSection : SummarySection {
        public List<double> RRMSEdifference { get; set; }
        public List<string> SubstanceCodes { get; set; }
        public List<string> ComponentCodes { get; set; }
        public ExposureApproachType McrExposureApproachType { get; set; }
        public GeneralMatrix UMatrix { get; set; }
        public int Optimum1 { get; set; }
        public int Optimum2 { get; set; }
        public bool Plot { get; set; }

        /// <summary>
        /// Calculates RMSE (root mean square error) and ratio RRMSE according to ANSES R-shiny application
        /// </summary>
        /// <param name="rmse"></param>
        /// <param name="exposureApproachType"></param>
        public void Summarize(
                List<ComponentRecord> componentRecords,
                ExposureApproachType exposureApproachType,
                GeneralMatrix uMatrix,
                List<Compound> substances,
                List<double> rmse
            ) {
            UMatrix = uMatrix.NormalizeColumns();
            SubstanceCodes = substances.Select(c => c.Code).ToList();
            ComponentCodes = Enumerable.Range(1, UMatrix.ColumnDimension).Select(c => c.ToString()).ToList();
            if (componentRecords.Count > 2) {
                Plot = true;
                McrExposureApproachType = exposureApproachType;
                var rrmse = new List<double>() { 0 };
                for (int i = 1; i < rmse.Count; i++) {
                    rrmse.Add(rmse[i] / rmse[i - 1]);
                }
                rrmse[0] = rrmse[1];
                RRMSEdifference = new List<double>();
                for (int i = 0; i < rmse.Count - 1; i++) {
                    RRMSEdifference.Add(rrmse[i] - rrmse[i + 1]);
                }
                var sortedRrmse = RRMSEdifference.OrderByDescending(c => c).ToList();
                Optimum1 = RRMSEdifference.FindIndex(c => c == sortedRrmse[0]) + 2;
                Optimum2 = RRMSEdifference.FindIndex(c => c == sortedRrmse[1]) + 2;
            }
        }

        /// <summary>
        /// Write normalized U matrix SNMU to csv file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string WriteUMatrixCsv(string filename) {
            return UMatrix.WriteToCsvFile(filename, SubstanceCodes, ComponentCodes);
        }
    }
}
