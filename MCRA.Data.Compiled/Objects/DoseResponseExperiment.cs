using MCRA.General;
using System.Data;

namespace MCRA.Data.Compiled.Objects {
    public sealed class DoseResponseExperiment : StrongEntity {

        public DateTime? Date { get; set; }
        public string DoseRoute { get; set; }
        public string TimeUnit { get; set; }
        public List<string> Covariates { get; set; }
        public string Reference { get; set; }
        public List<Compound> Substances { get; set; }
        public string Time { get; set; }
        public List<string> Design { get; set; }
        public List<ExperimentalUnit> ExperimentalUnits { get; set; }
        public List<Response> Responses { get; set; }

        public DoseUnit DoseUnit { get; set; }

        public List<DoseResponseExperimentMeasurement> GetResponseMeasurements(Response response) {
            var result = new List<DoseResponseExperimentMeasurement>();
            foreach (var unit in ExperimentalUnits) {
                if (unit.Responses.ContainsKey(response)) {
                    result.Add(unit.Responses[response]);
                }
            }
            return result;
        }

        public List<ExperimentalUnit> GetResponseData(Response response) {
            return ExperimentalUnits
                .Where(r => r.Responses.ContainsKey(response))
                .ToList();
        }

        public DataTable toDataTable(Response response, bool isMixture) {
            var experimentalUnits = GetResponseData(response);
            var hasSD = experimentalUnits.All(c => c.Responses[response].ResponseSD != null);
            var hasCV = experimentalUnits.All(c => c.Responses[response].ResponseCV != null);
            var hasN = experimentalUnits.All(c => c.Responses[response].ResponseN != null);
            var hasUncertainty = experimentalUnits.All(c => c.Responses[response].ResponseUncertaintyUpper != null);

            var Block = DataTableColumnNames.Block.ToString();
            var Substance = DataTableColumnNames.Substance.ToString();
            var SD = DataTableColumnNames.SD.ToString();
            var CV = DataTableColumnNames.CV.ToString();
            var N = DataTableColumnNames.N.ToString();
            var ExperimentalUnit = DataTableColumnNames.ExperimentalUnit.ToString();
            var UncertaintyUpper = DataTableColumnNames.UncertaintyUpper.ToString();

            var dataTable = new DataTable();
            if (Design?.Count > 0) {
                dataTable.Columns.Add(Block, typeof(string));
            }
            dataTable.Columns.Add(ExperimentalUnit, typeof(string));
            foreach (var substance in Substances) {
                dataTable.Columns.Add(substance.Code, typeof(double));
            }
            dataTable.Columns.Add(response.Code, typeof(double));

            if (hasSD) {
                dataTable.Columns.Add(SD, typeof(double));
            }
            if (hasCV) {
                dataTable.Columns.Add(CV, typeof(double));
            }
            if (hasN) {
                dataTable.Columns.Add(N, typeof(int));
            }
            if (hasUncertainty) {
                dataTable.Columns.Add(UncertaintyUpper, typeof(double));
            }

            if (Covariates != null) {
                foreach (var covariate in Covariates) {
                    dataTable.Columns.Add(covariate, typeof(string));
                }
            }

            if (!string.IsNullOrEmpty(Time)) {
                dataTable.Columns.Add(Time, typeof(string));
            }

            dataTable.Columns.Add(Substance, typeof(string));

            var counter = 0;
            foreach (var unit in experimentalUnits) {
                DataRow row = dataTable.NewRow();
                if (Design?.Count > 0) {
                    row[Block] = unit.DesignFactors[Design.First()];
                }
                row[ExperimentalUnit] = unit.Code;
                var doses = new List<double>();
                foreach (var substance in Substances) {
                    var dose = 0d;
                    unit.Doses.TryGetValue(substance, out dose);
                    row[substance.Code] = dose;
                    doses.Add(dose);
                }

                if (unit.Responses.ContainsKey(response)) {
                    row[response.Code] = unit.Responses[response].ResponseValue;
                } else {
                    row[response.Code] = double.NaN;
                }

                if (hasSD) {
                    row[SD] = unit.Responses[response].ResponseSD;
                }
                if (hasCV) {
                    row[CV] = unit.Responses[response].ResponseCV;
                }
                if (hasN) {
                    row[N] = unit.Responses[response].ResponseN;
                }
                if (hasUncertainty) {
                    row[UncertaintyUpper] = unit.Responses[response].ResponseUncertaintyUpper;
                }

                if (Covariates != null) {
                    foreach (var covariate in Covariates) {
                        row[covariate] = unit.Covariates[covariate];
                    }
                }

                if (!string.IsNullOrEmpty(Time)) {
                    row[Time] = unit.Times;
                }

                if (doses.Count == 1) {
                    row[Substance] = Substances[0].Code;
                } else if (doses.Count(c => c > 0) > 1) {
                    row[Substance] = DataTableColumnNames.Mixture.ToString();

                } else {
                    var ix = doses.FindIndex(c => c > 0);
                    if (ix != -1) {
                        row[Substance] = Substances[ix].Code;
                    } else {
                        ix = counter % Substances.Count;
                        row[Substance] = Substances[ix].Code;
                        counter++;
                    }
                }
                if (isMixture || row[Substance].ToString() != DataTableColumnNames.Mixture.ToString()) {
                    dataTable.Rows.Add(row);
                }
            }
            return dataTable;
        }

        public DataTable CreateAllResponsesDataTable() {
            var Block = DataTableColumnNames.Block.ToString();
            var Substance = DataTableColumnNames.Substance.ToString();
            var SD = DataTableColumnNames.SD.ToString();
            var CV = DataTableColumnNames.CV.ToString();
            var N = DataTableColumnNames.N.ToString();
            var ExperimentalUnit = DataTableColumnNames.ExperimentalUnit.ToString();
            var UncertaintyUpper = DataTableColumnNames.UncertaintyUpper.ToString();

            var responseProperties = Responses
                .ToDictionary(r => r, r => new {
                    HasSD = ExperimentalUnits.All(c => c.Responses[r].ResponseSD != null),
                    HasCV = ExperimentalUnits.All(c => c.Responses[r].ResponseCV != null),
                    HasN = ExperimentalUnits.All(c => c.Responses[r].ResponseN != null),
                    HasUncertainty = ExperimentalUnits.All(c => c.Responses[r].ResponseUncertaintyUpper != null),
                });

            var dataTable = new DataTable();
            if (Design?.Count > 0) {
                dataTable.Columns.Add(Block, typeof(string));
            }
            dataTable.Columns.Add(ExperimentalUnit, typeof(string));
            foreach (var substance in Substances) {
                dataTable.Columns.Add(substance.Code, typeof(double));
            }

            foreach (var response in Responses) {
                dataTable.Columns.Add(response.Code, typeof(double));
                if (responseProperties[response].HasSD) {
                    dataTable.Columns.Add($"{response.Code}:{SD}", typeof(double));
                }
                if (responseProperties[response].HasCV) {
                    dataTable.Columns.Add($"{response.Code}:{CV}", typeof(double));
                }
                if (responseProperties[response].HasN) {
                    dataTable.Columns.Add($"{response.Code}:{N}", typeof(int));
                }
                if (responseProperties[response].HasUncertainty) {
                    dataTable.Columns.Add($"{response.Code}:{UncertaintyUpper}" , typeof(double));
                }
            }

            foreach (var covariate in Covariates) {
                dataTable.Columns.Add(covariate, typeof(string));
            }

            if (!string.IsNullOrEmpty(Time)) {
                dataTable.Columns.Add(Time, typeof(string));
            }

            dataTable.Columns.Add(Substance, typeof(string));
            var counter = 0;
            foreach (var unit in ExperimentalUnits) {
                DataRow row = dataTable.NewRow();
                if (Design?.Count > 0) {
                    row[Block] = unit.DesignFactors[Design.First()];
                }
                row[ExperimentalUnit] = unit.Code;
                var doses = new List<double>();
                foreach (var substance in Substances) {
                    var dose = 0d;
                    unit.Doses.TryGetValue(substance, out dose);
                    row[substance.Code] = dose;
                    doses.Add(dose);
                }

                foreach (var response in Responses) {
                    if (unit.Responses.ContainsKey(response)) {
                        row[response.Code] = unit.Responses[response].ResponseValue;
                    } else {
                        row[response.Code] = double.NaN;
                    }

                    if (responseProperties[response].HasSD) {
                        row[$"{response.Code}:{SD}"] = unit.Responses[response].ResponseSD;
                    }
                    if (responseProperties[response].HasCV) {
                        row[$"{response.Code}:{CV}"] = unit.Responses[response].ResponseCV;
                    }
                    if (responseProperties[response].HasN) {
                        row[$"{response.Code}:{N}"] = unit.Responses[response].ResponseN;
                    }
                    if (responseProperties[response].HasUncertainty) {
                        row[$"{response.Code}:{UncertaintyUpper}"] = unit.Responses[response].ResponseUncertaintyUpper;
                    }
                }

                foreach (var covariate in Covariates) {
                    row[covariate] = unit.Covariates[covariate.ToLower()];
                }

                if (!string.IsNullOrEmpty(Time)) {
                    row[Time] = unit.Times;
                }

                if (doses.Count == 1) {
                    row[Substance] = Substances[0].Code;
                } else if (doses.Count(c => c > 0) > 1) {
                    row[Substance] = DataTableColumnNames.Mixture.ToString(); ;
                } else {
                    var ix = doses.FindIndex(c => c > 0);
                    if (ix != -1) {
                        row[Substance] = Substances[ix].Code;
                    } else {
                        ix = counter % Substances.Count;
                        row[Substance] = Substances[ix].Code;
                        counter++;
                    }
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }
    }
}
