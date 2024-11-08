using MCRA.General;
using MCRA.General.DoseResponseModels;
using System.Text.Json;
using System.Globalization;
using MCRA.Data.Raw.Objects.RawTableGroups;
using MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Raw.Copying.BulkCopiers.DoseResponseModels {

    public enum ProastModelFamily {
        Exp,
        Hill,
        TwoStage,
        LogLogist,
        Weibull,
        LogProb,
        Gamma,
        Logistic,
        Probit,
        Unknown
    }

    public class ProastJsonDataReader {

        #region Constats

        private static Dictionary<int, DoseResponseModelType> _proastModelTypesContinuous = new() {
            { 11, DoseResponseModelType.Expm1 },
            { 12, DoseResponseModelType.Expm2 },
            { 13, DoseResponseModelType.Expm3 },
            { 14, DoseResponseModelType.Expm4 },
            { 15, DoseResponseModelType.Expm5 },
            { 21, DoseResponseModelType.Hillm1 },
            { 22, DoseResponseModelType.Hillm2 },
            { 23, DoseResponseModelType.Hillm3 },
            { 24, DoseResponseModelType.Hillm4 },
            { 25, DoseResponseModelType.Hillm5 },
            { 46, DoseResponseModelType.Expm5 },
        };

        private static Dictionary<int, DoseResponseModelType> _proastModelTypesLVM = new() {
            { 12, DoseResponseModelType.LVM_Exp_M2 },
            { 13, DoseResponseModelType.LVM_Exp_M3 },
            { 14, DoseResponseModelType.LVM_Exp_M4 },
            { 15, DoseResponseModelType.LVM_Exp_M5 },
            { 22, DoseResponseModelType.LVM_Hill_M2 },
            { 23, DoseResponseModelType.LVM_Hill_M3 },
            { 24, DoseResponseModelType.LVM_Hill_M4 },
            { 25, DoseResponseModelType.LVM_Hill_M5 },
        };

        private static Dictionary<int, DoseResponseModelType> _proastModelTypesQuantal = new() {
            { 16, DoseResponseModelType.TwoStage },
            { 18, DoseResponseModelType.LogLogist },
            { 19, DoseResponseModelType.Weibull },
            { 21, DoseResponseModelType.LogProb },
            { 24, DoseResponseModelType.Gamma },
            { 25, DoseResponseModelType.Probit },
            { 26, DoseResponseModelType.Logistic },
        };

        #endregion

        /// <summary>
        /// Reads the Proast analysis output exported as JSON. Uses the output id
        /// as id of the model fit and requires the experiment id to link the output
        /// to a dose response experiment.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="idOutput"></param>
        /// <param name="idExperiment"></param>
        /// <returns></returns>
        public RawDoseResponseModelData Read(string json, string idOutput, string idExperiment) {
            // Hack: replace lower-case hill by other tag (key is used twice in proast output)
            json = json.Replace("\"hill\":", "\"hill.unwanted\":");

            var output = JsonSerializer.Deserialize<ProastOutput>(
                json,
                new JsonSerializerOptions {
                    AllowTrailingCommas = true,
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                }
            );

            var result = new RawDoseResponseModelData();
            if (output.SingleModel != null) {
                if (output.SingleModel != null) {
                    try {
                        var model = output.SingleModel;
                        parseModel(model, output, result, idExperiment, idOutput, ProastModelFamily.Exp);
                    } catch { }
                }
            } else {
                if (output.TwoStage != null) {
                    try {
                        var model = output.TwoStage;
                        parseModel(model, output, result, idExperiment, idOutput, ProastModelFamily.TwoStage);
                    } catch { }
                }
                if (output.LogLogist != null) {
                    try {
                        var model = output.LogLogist;
                        parseModel(model, output, result, idExperiment, idOutput, ProastModelFamily.LogLogist);
                    } catch { }
                }
                if (output.Weibull != null) {
                    try {
                        var model = output.Weibull;
                        parseModel(model, output, result, idExperiment, idOutput, ProastModelFamily.Weibull);
                    } catch { }
                }
                if (output.LogProb != null) {
                    try {
                        var model = output.LogProb;
                        parseModel(model, output, result, idExperiment, idOutput, ProastModelFamily.LogProb);
                    } catch { }
                }
                if (output.Gamma != null) {
                    try {
                        var model = output.Gamma;
                        parseModel(model, output, result, idExperiment, idOutput, ProastModelFamily.Gamma);
                    } catch { }
                }
                if (output.Logistic != null) {
                    try {
                        var model = output.Logistic;
                        parseModel(model, output, result, idExperiment, idOutput, ProastModelFamily.Logistic);
                    } catch { }
                }
                if (output.Probit != null) {
                    var model = output.Probit;
                    parseModel(model, output, result, idExperiment, idOutput, ProastModelFamily.Probit);
                }
                if (output.ExponentialModel != null) {
                    try {
                        var model = output.ExponentialModel;
                        parseModel(model, output, result, idExperiment, idOutput, ProastModelFamily.Exp);
                    } catch { }
                }
                if (output.HillModel != null) {
                    try {
                        var model = output.HillModel;
                        parseModel(model, output, result, idExperiment, idOutput, ProastModelFamily.Hill);
                    } catch { }
                }
            }

            return result;
        }

        private static void parseModel(ProastModelFit model,
                ProastOutput output,
                RawDoseResponseModelData doseResponseModelData,
                string idExperiment,
                string idOutput,
                ProastModelFamily modelFamily
        ) {
            var distributionType = output.DistributionType.First();
            var modelType = parseModelType(model, modelFamily, distributionType == 4);
            var modelId = $"{idExperiment}-{idOutput}-{modelType}";

            // Get response and response type
            var responseColumnName = output.VariableNames[output.ResponseVariableColumnIndex.First() - 1];
            var benchmarkResponseType = BenchmarkResponseType.Undefined;
            var riskType = RiskType.Undefined;
            if (distributionType == 4) {
                getCesType(output.CriticalEffectSizeType.First(), ref benchmarkResponseType, ref riskType);
            } else if (distributionType == 1 || distributionType == 10) {
                benchmarkResponseType = BenchmarkResponseType.FractionChange;
            }

            // Get substances/dose
            var doseColumnNames = output.DoseVariableColumnIndexes.Select(r => output.VariableNames[r - 1]).ToList();
            var substanceCodes = doseColumnNames;
            if (output.ModelAns.First() == 46) {
                substanceCodes = model.Groupings.Select(c => c.Remove(c.Length - 1).Substring(1)).ToList();
            }

            var isMultipleSubstance = doseColumnNames.Count > 1;
            var selectedCovariateLevelIndexes = model.SensitiveLevel != null && model.SensitiveLevel.All(r => output.CovariateLevels.Contains(r ?? string.Empty))
                ? model.SensitiveLevel.Select(r => output.CovariateLevels.IndexOf(r ?? string.Empty)).ToList()
                : Enumerable.Range(0, output.CovariateLevels.Count).ToList();

            if (output.ModelAns.First() == 46) {
                isMultipleSubstance = true;
                var referenceSubstance = (int)(output.ReferenceLevel.First()) - 1;
                selectedCovariateLevelIndexes = Enumerable.Range(0, (int)(model.NumberOfGroupings.First()))
                    .OrderBy(c => c != referenceSubstance).ToList();
            }

            var description = $"Proast dose response model fit for response {responseColumnName} of experiment {idExperiment}";
            var result = new RawDoseResponseModel() {
                idDoseResponseModel = modelId,
                idExperiment = idExperiment,
                Name = modelId,
                Description = modelId,
                Substances = string.Join(",", substanceCodes),
                DoseResponseModelType = modelType,
                idResponse = responseColumnName,
                Covariates = output.CovariateNames.Any() ? string.Join(",", output.CovariateNames) : null,
                CriticalEffectSize = output.CriticalEffectSize.First(),
                BenchmarkResponseType = benchmarkResponseType.ToString(),
                LogLikelihood = model.LogLikelihood.First(),
                ModelEquation = modelType.ToString(),
                ProastVersion = output.ProastVersion.FirstOrDefault(),
            };
            doseResponseModelData.DoseResponseModels.Add(result);

            var counter = 0;
            double ced = double.NaN;
            double cedReference = double.NaN;
            double cedLower = double.NaN;
            double cedUpper = double.NaN;
            double rpfLower = double.NaN;
            double rpfUpper = double.NaN;
            List<double> referenceBootstraps = null;

            for (int j = 0; j < doseColumnNames.Count; j++) {
                for (int k = 0; k < selectedCovariateLevelIndexes.Count; k++) {

                    // Find response at zero dose; should be the parameter a of the model (may be covariate dependent).
                    var covariateLevel = string.Empty;
                    if (output.CovariateLevels.First() != string.Empty) {
                        covariateLevel = output.CovariateLevels[selectedCovariateLevelIndexes[k]];
                    }
                    var aParamIndex = model.ParameterNames.FindIndex(r => r.Equals("a-" + covariateLevel));

                    // Get CED
                    var rpf = 1D;
                    if (counter == 0 || !isMultipleSubstance) {
                        if (model.CED != null && model.CED.Length > selectedCovariateLevelIndexes[k]) {
                            ced = model.CED[selectedCovariateLevelIndexes[k]];
                        } else if (model.CEDMatrix != null && model.CEDMatrix.Length > selectedCovariateLevelIndexes[k]) {
                            ced = (model.CEDMatrix[selectedCovariateLevelIndexes[k]].First());
                        }
                        if (double.IsNaN(ced)) {
                            ced = parseModelParameter(model, "CED-", covariateLevel);
                        }
                        cedReference = ced;
                    } else {
                        if (model.CED != null && model.CED.Length > selectedCovariateLevelIndexes[k]) {
                            rpf = model.CED[selectedCovariateLevelIndexes[k]];
                        } else if (model.CEDMatrix != null && model.CEDMatrix.Length > selectedCovariateLevelIndexes[k]) {
                            rpf = model.CEDMatrix[selectedCovariateLevelIndexes[k]].First();
                        }
                        if (double.IsNaN(rpf)) {
                            rpf = parseModelParameter(model, "CED-", covariateLevel);
                        }
                    }


                    if (j > 0) {
                        if (model.RPFs != null) {
                            //Proast version 66.27 based on object 'RPFs'
                            rpf = model.RPFs[j - 1];
                        } else {
                            //Since Proast version 70.3, object 'RPFs' is removed from ans.all, therefore determine rpfs based on number of parameters
                            //of the general Exponential or Hill model ('nrp' in ans.all)
                            var nParModel = model.NoModelParameters.First();
                            rpf = model.Estimates[nParModel + j - 1];
                        }
                    }
                    ced = cedReference / rpf;

                    if (model.ConfidenceIntervals.Length > 1) {
                        // Attention: Proast seems not to accept the combination of covariates and multiple substances
                        // The confidence intervals are either for covariates or multiple substances.
                        if (counter == 0 || !isMultipleSubstance) {
                            // The confidence interval is expressed as BMD
                            cedLower = model.ConfidenceIntervals[j * output.CovariateLevels.Count + selectedCovariateLevelIndexes[k]][0];
                            cedUpper = model.ConfidenceIntervals[j * output.CovariateLevels.Count + selectedCovariateLevelIndexes[k]][1];
                            rpfLower = 1;
                            rpfUpper = 1;

                        } else {
                            // The confidence interval is expressed in RPFs
                            var sigma = Math.Log(cedUpper / cedLower) / (1.645 * 2);
                            rpfLower = model.ConfidenceIntervals[j * output.CovariateLevels.Count + selectedCovariateLevelIndexes[k]][0];
                            rpfUpper = model.ConfidenceIntervals[j * output.CovariateLevels.Count + selectedCovariateLevelIndexes[k]][1];
                            cedLower = Math.Exp(Math.Log(ced) - 1.645 * sigma);
                            cedUpper = Math.Exp(Math.Log(ced) + 1.645 * sigma);
                        }
                    } else {
                        cedLower = model.ConfidenceIntervals[0][0];
                        cedUpper = model.ConfidenceIntervals[0][1];
                        rpfLower = 1;
                        rpfUpper = 1;
                    }

                    var modelParametersString = string.Empty;
                    var bmr = double.NaN;
                    if (model.ParameterNames.Count == model.Estimates.Length) {
                        var modelFunction = parseModelParameters(model, output, modelType, selectedCovariateLevelIndexes, k, result.CriticalEffectSize, ced, riskType);
                        var modelParameters = modelFunction?.ExportParameters();
                        bmr = modelFunction?.ComputeBmr(ced, result.CriticalEffectSize, riskType) ?? double.NaN;
                        if (modelParameters != null) {
                            modelParametersString = string.Join(",", modelParameters.Select(r => $"{r.Key}={r.Value.ToString(CultureInfo.InvariantCulture)}"));
                        }
                    }

                    var substanceCode = substanceCodes[j];

                    if (output.ModelAns.First() == 46) {
                        substanceCode = substanceCodes[selectedCovariateLevelIndexes[k]];
                    }
                    var record = new RawDoseResponseModelBenchmarkDose() {
                        idDoseResponseModel = modelId,
                        idSubstance = substanceCode,
                        Covariates = output.CovariateLevels.Any() ? string.Join(",", covariateLevel) : null,
                        BenchmarkResponse = bmr,
                        BenchmarkDose = ced,
                        BenchmarkDoseLower = !double.IsNaN(cedLower) ? cedLower : null,
                        BenchmarkDoseUpper = !double.IsNaN(cedUpper) ? cedUpper : null,
                        Rpf = rpf,
                        RpfLower = !double.IsNaN(rpfLower) ? rpfLower : null,
                        RpfUpper = !double.IsNaN(rpfUpper) ? rpfUpper : null,
                        ModelParameterValues = modelParametersString
                    };
                    doseResponseModelData.BenchmarkDoses.Add(record);

                    var bootstraps = model?.CEDBootstrap?
                        .Select(r => r[j * output.CovariateLevels.Count + selectedCovariateLevelIndexes[k]])
                        .ToList();

                    if (bootstraps?.Count > 0) {
                        if (counter == 0) {
                            referenceBootstraps = bootstraps;
                        }
                        var uncertainRecords = new List<RawDoseResponseModelBenchmarkDoseUncertain>();
                        for (int i = 0; i < bootstraps.Count; i++) {
                            double cedBootstrap;
                            double rpfBootstrap;
                            // Attention: Proast seems not to accept the combination of covariates and multiple substances
                            // The confidence intervals are either for covariates or multiple substances.
                            var cedReferenceBootstrap = referenceBootstraps[i];
                            if (counter == 0 || !isMultipleSubstance) {
                                // The confidence interval is expressed as BMD
                                cedBootstrap = cedReferenceBootstrap;
                                rpfBootstrap = 1;

                            } else {
                                //cedBootstrap = bootstraps[i];
                                //rpfBootstrap = cedBootstrap / cedReferenceBootstrap;
                                rpfBootstrap = bootstraps[i];
                                cedBootstrap = cedReferenceBootstrap / rpfBootstrap;
                            }
                            uncertainRecords.Add(new RawDoseResponseModelBenchmarkDoseUncertain() {
                                idSubstance = substanceCode,
                                idDoseResponseModel = modelId,
                                idUncertaintySet = i.ToString(),
                                BenchmarkDose = cedBootstrap,
                                Rpf = rpfBootstrap,
                                Covariates = output.CovariateLevels.Any() ? string.Join(",", covariateLevel) : null,
                            });
                        }
                        doseResponseModelData.BenchmarkDosesUncertain.AddRange(uncertainRecords);
                    }

                    counter++;
                }
            }
        }

        private static void getCesType(int cesType, ref BenchmarkResponseType benchmarkResponseType, ref RiskType riskType) {
            switch (cesType) {
                case 1:
                    riskType = RiskType.Ed50;
                    benchmarkResponseType = BenchmarkResponseType.Ed50;
                    break;
                case 2:
                    benchmarkResponseType = BenchmarkResponseType.AdditionalRisk;
                    riskType = RiskType.AdditionalRisk;
                    break;
                case 3:
                    benchmarkResponseType = BenchmarkResponseType.ExtraRisk;
                    riskType = RiskType.ExtraRisk;
                    break;
                default:
                    break;
            }
        }

        private static IDoseResponseModelFunction parseModelParameters(ProastModelFit model, ProastOutput output, DoseResponseModelType modelType, List<int> selectedCovariateLevelIndexes, int k, double ces, double ced, RiskType riskType) {
            var parameters = new Dictionary<string, double>();

            Func<string[], string, double> tryAddParameter = (parameterNames, myName) => {
                var level = string.Empty;
                if (output.CovariateLevels.First() != string.Empty) {
                    level = output.CovariateLevels[selectedCovariateLevelIndexes[k]];
                }
                var val = parseModelParameter(model, level, parameterNames);
                if (!double.IsNaN(val)) {
                    parameters.Add(myName, val);
                }
                return val;
            };
            var a = tryAddParameter(new string[] { "a-" }, "a");
            var c = tryAddParameter(new string[] { "c", "c-", "c ", "cc" }, "c");
            var d = tryAddParameter(new string[] { "d-" }, "d");
            var sigma = tryAddParameter(new string[] { "sigma", "sigma(fixed)" }, "sigma");
            var doseResponseModel = DoseResponseModelFactory.Create(modelType, parameters);
            if (doseResponseModel != null) {
                var bmr = doseResponseModel.ComputeBmr(ced, ces, riskType);
                doseResponseModel.DeriveParameterB(ced, bmr);
            }
            return doseResponseModel;
        }

        private static double parseModelParameter(ProastModelFit model, string covariateLevelString, params string[] names) {
            var paramIndex = -1;
            foreach (var name in names) {
                paramIndex = model.ParameterNames.FindIndex(r => r.Equals(name + covariateLevelString));
                if (paramIndex != -1) {
                    break;
                }
                paramIndex = model.ParameterNames.FindIndex(r => r.Equals(name));
                if (paramIndex != -1) {
                    break;
                }
            }
            if (paramIndex >= 0) {
                return model.Estimates[paramIndex];
            }
            return double.NaN;
        }

        private static DoseResponseModelType parseModelType(ProastModelFit model, ProastModelFamily modelFamily, bool isQuantal) {
            var modelAns = model.ModelAns.First();
            if (isQuantal) {
                if (modelFamily == ProastModelFamily.Exp || modelFamily == ProastModelFamily.Hill || modelFamily == ProastModelFamily.Unknown) {
                    return parseModelTypeLVM(modelAns);
                }
                return parseModelTypeQuantal(modelAns);
            } else {
                return parseModelTypeContinuous(modelAns);
            }
        }

        public static DoseResponseModelType parseModelTypeLVM(int proastModelAns) {
            if (_proastModelTypesLVM.ContainsKey(proastModelAns)) {
                return _proastModelTypesLVM[proastModelAns];
            }
            return DoseResponseModelType.Unknown;
        }

        public static DoseResponseModelType parseModelTypeContinuous(int proastModelAns) {
            if (_proastModelTypesContinuous.ContainsKey(proastModelAns)) {
                return _proastModelTypesContinuous[proastModelAns];
            }
            return DoseResponseModelType.Unknown;
        }

        public static DoseResponseModelType parseModelTypeQuantal(int proastModelAns) {
            if (_proastModelTypesQuantal.ContainsKey(proastModelAns)) {
                return _proastModelTypesQuantal[proastModelAns];
            }
            return DoseResponseModelType.Unknown;
        }
    }
}
