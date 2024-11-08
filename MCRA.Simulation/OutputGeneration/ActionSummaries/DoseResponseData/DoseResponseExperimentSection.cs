using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public class DoseResponseExperimentSection : SummarySection {

        [Display(Name = "Experiment", Order = 1)]
        public string ExperimentCode { get; set; }

        [Display(Name = "Response", Order = 2)]
        public string ResponseCode { get; set; }

        [Display(Name = "Response unit", Order = 3)]
        public string ResponseUnit { get; set; }

        [Display(Name = "Response type", Order = 4)]
        public ResponseType ResponseType { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsMixture { get; set; }

        [Display(Name = "Experiment type", Order = 5)]
        [Description("The type of the experiment (single substance / non-mixture multi-substance / mixture).")]
        public string ExperimentType {
            get {
                if (DoseResponseSets.Count > 1) {
                    return IsMixture ? "Mixture" : "Non-mixture multi-substance";
                }
                return "Single substance";
            }
        }

        [Display(Name = "Substance(s)", Order = 6)]
        public string SubstanceCodesString {
            get {
                var values = SubstanceNames?.Count > 0 ? SubstanceNames : SubstanceCodes;
                return string.Join(",", values);
            }
        }

        [Display(Name = "Dose unit", Order = 7)]
        public DoseUnit DoseUnit { get; set; }

        [Display(AutoGenerateField = false)]
        public List<string> SubstanceCodes { get; set; }

        [Display(AutoGenerateField = false)]
        public List<string> SubstanceNames { get; set; }

        [Display(AutoGenerateField = false)]
        public List<DoseResponseSet> DoseResponseSets { get; set; }

        [Display(AutoGenerateField = false)]
        public DoseResponseMixtureSet DoseResponseMixtureSet { get; set; }

        public void Summarize(DoseResponseExperiment experiment, Response response) {
            ExperimentCode = experiment.Code;
            ResponseCode = response.Code;
            DoseUnit = experiment.DoseUnit;
            ResponseUnit = response.ResponseUnit;
            ResponseType = response.ResponseType;
            var substances = experiment.Substances.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList();
            SubstanceCodes = substances.Select(r => r.Code).ToList();
            SubstanceNames = substances.Select(r => r.Name).ToList();
            if (experiment.ExperimentalUnits != null) {
                var isMixture = experiment.ExperimentalUnits.Select(c => c.Doses.Count(d => d.Value > 0)).Any(r => r > 1);
                var dataTable = experiment.toDataTable(response, isMixture);
                DoseResponseSets = getDosesPerSubstance(dataTable, experiment, response);
                DoseResponseMixtureSet = getDosesPerMixture(dataTable, experiment, response);
            }
        }

        /// <summary>
        /// Get all information per substance, ignore mixtures.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="experiment"></param>
        /// <param name="response"></param>
        private List<DoseResponseSet> getDosesPerSubstance(
            DataTable dataTable,
            DoseResponseExperiment experiment,
            Response response
        ) {
            var Block = DataTableColumnNames.Block.ToString();
            var Substance = DataTableColumnNames.Substance.ToString();
            var SD = DataTableColumnNames.SD.ToString();
            var CV = DataTableColumnNames.CV.ToString();
            var N = DataTableColumnNames.N.ToString();

            var doseResponseSets = new List<DoseResponseSet>();
            var hasSD = dataTable.Columns.Contains(SD);
            var hasN = dataTable.Columns.Contains(N);
            var hasCovariate = experiment.Covariates.Count > 0;
            var countSubstances = 0;
            //var nColumns = dataTable.Columns.Count - 1;
            var nRows = dataTable.Rows.Count;
            foreach (var substance in experiment.Substances) {
                var doseResponseRecords = new List<KeyValuePair<string, DoseResponseRecord>>();
                foreach (DataRow row in dataTable.Rows) {
                    if (row[Substance].ToString() == substance.Code) {
                        var doseResponseRecord = new DoseResponseRecord() {
                            Dose = (double)row[substance.Code],
                            Response = (double)row[response.Code],
                        };
                        if (hasSD) {
                            doseResponseRecord.SD = (double)row[SD];
                            doseResponseRecord.Response = getReconstructedGeometricMean(doseResponseRecord);
                        }
                        if (hasN) {
                            doseResponseRecord.N = (int)row[N];
                            if (response.ResponseType == ResponseType.Quantal || response.ResponseType == ResponseType.QuantalGroup) {
                                doseResponseRecord.Response = doseResponseRecord.Response / doseResponseRecord.N;
                            }
                        }
                        var covariateLevel = string.Empty;
                        if (hasCovariate) {
                            covariateLevel = (string)row[experiment.Covariates.First()];
                        }
                        doseResponseRecords.Add(new KeyValuePair<string, DoseResponseRecord>(covariateLevel, doseResponseRecord));
                        countSubstances++;
                    }
                }
                var covariateLevels = doseResponseRecords.Select(c => c.Key).Distinct().ToList();
                foreach (var level in covariateLevels) {
                    doseResponseSets.Add(new DoseResponseSet() {
                        DoseResponseRecords = doseResponseRecords.Where(c => c.Key == level).Select(c => c.Value).ToList(),
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        CovariateLevel = level,
                    });
                }
            }
            if (countSubstances < nRows) {
                IsMixture = true;
            }
            return doseResponseSets;
        }

        /// <summary>
        /// Get all information for mixtures.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="experiment"></param>
        /// <param name="response"></param>
        private DoseResponseMixtureSet getDosesPerMixture(
            DataTable dataTable,
            DoseResponseExperiment experiment,
            Response response
        ) {
            //var nColumns = dataTable.Columns.Count - 1;
            var Substance = DataTableColumnNames.Substance.ToString();

            var doseResponseMixtureRecords = new List<DoseResponseMixtureRecord>();
            foreach (DataRow row in dataTable.Rows) {
                if (row[Substance].ToString() == DataTableColumnNames.Mixture.ToString()) {
                    var mixtureDoses = new List<MixtureDose>();
                    foreach (var substance in experiment.Substances) {
                        mixtureDoses.Add(new MixtureDose() {
                            SubstanceCode = substance.Code,
                            Dose = (double)row[substance.Code]
                        });
                    }
                    doseResponseMixtureRecords.Add(new DoseResponseMixtureRecord() {
                        MixtureDose = mixtureDoses,
                        Response = (double)row[response.Code],
                    });
                }
            }
            return new DoseResponseMixtureSet() {
                DoseResponseMixtureRecords = doseResponseMixtureRecords,
                Mixture = string.Join(",", experiment.Substances),
                RPFDict = [],
            };
        }

        /// <summary>
        /// For calculation of new means, see Slob Dose response modeling of continuous endpoints.
        /// </summary>
        /// <param name="doseResponseRecord"></param>
        /// <returns></returns>
        private double getReconstructedGeometricMean(DoseResponseRecord doseResponseRecord) {
            var cv = doseResponseRecord.SD / doseResponseRecord.Response;
            var expZ = Math.Exp(Math.Log(doseResponseRecord.Response / Math.Sqrt(1 + cv * cv)));
            return expZ;
        }
    }
}
