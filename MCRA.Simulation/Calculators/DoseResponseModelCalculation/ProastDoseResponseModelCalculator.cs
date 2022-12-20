using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Logger;
using MCRA.Utils.R.REngines;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.RawDataObjectConverters;
using MCRA.Data.Raw.Copying.BulkCopiers.DoseResponseModels;
using MCRA.General;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MCRA.Simulation.Calculators.DoseResponseModelCalculation {
    public sealed class ProastDoseResponseModelCalculator {

        public const string ProastVersion = "70.3";

        public string OutputPath { get; set; }

        public ProastDoseResponseModelCalculator(string outputPath = null) {
            OutputPath = (outputPath != null) ? outputPath : Path.Combine(Path.GetTempPath(), "ProastFits");
        }

        public bool CheckProastAvailable() {
            try {
                using (var R = new RDotNetEngine()) {
                    R.LoadLibrary($"proast{ProastVersion}");
                    var version = R.EvaluateString("f.version()");
                    return !string.IsNullOrEmpty(version);
                }
            } catch {
                return false;
            }
        }

        public List<DoseResponseModel> TryCompute(DoseResponseExperiment experiment, Response response, double benchmarkResponse, BenchmarkResponseType benchmarkResponseType, List<string> covariates, Compound referenceCompound, int? numberOfBootstrapRuns, bool fitCovariates) {
            try {
                return calculate(experiment, response, benchmarkResponse, benchmarkResponseType, covariates, referenceCompound, numberOfBootstrapRuns, fitCovariates);
            } catch (Exception ex) {
                var message = string.Empty;
                if (ex.GetType().IsAssignableFrom(typeof(NotImplementedException))) {
                    message = ex.Message;
                }
                if (message.Contains("Proast interaction needed")) {
                    message = "Cannot fit model. Modelling requires user interaction with Proast.";
                }
                var fitName = experiment.Code + "-" + response.Code;
                var drm = new DoseResponseModel() {
                    IdDoseResponseModel = fitName,
                    IdExperiment = experiment.Code,
                    Name = fitName,
                    Description = $"Failed dose response model fit for response {response.Code} of experiment {experiment.Code}",
                    Substances = experiment.Substances,
                    Covariates = covariates,
                    Response = response,
                    CriticalEffectSize = benchmarkResponse,
                    BenchmarkResponseType = benchmarkResponseType,
                    ProastVersion = ProastVersion,
                    ExceptionMessage = message,
                    DoseResponseModelBenchmarkDoses = new Dictionary<string, DoseResponseModelBenchmarkDose>(StringComparer.OrdinalIgnoreCase)
                };
                return new List<DoseResponseModel>() { drm };
            }
        }

        private List<DoseResponseModel> calculate(DoseResponseExperiment experiment, Response response, double benchmarkResponse, BenchmarkResponseType benchmarkResponseType, List<string> covariates, Compound referenceCompound, int? numberOfBootstrapRuns, bool fitCovariates) {
            var fitName = experiment.Code + "-" + response.Code;
            var outputFolder = Path.Combine(OutputPath, fitName);

            if (!Directory.Exists(outputFolder)) {
                Directory.CreateDirectory(outputFolder);
            }

            //In Proast: for a mixture the first compound is used as a reference, so order substances accordingly (ans.all$xans)
            //         : for a dose additive model, the substance levels are ordered and a level is selected (e.g. first, second etc), 
            //         : order of substances does not matter (ans.all$xans)
            var substances = experiment.Substances
                .OrderBy(r => r == referenceCompound ? 0D : 1D)
                .ThenBy(r => r.Code, System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            var ces = getCes(benchmarkResponse, benchmarkResponseType, response.ResponseType);
            var experimentalUnits = experiment.GetResponseData(response);
            var hasSD = experimentalUnits.All(c => c.Responses[response].ResponseSD != null);
            var hasCV = experimentalUnits.All(c => c.Responses[response].ResponseCV != null);
            var hasN = experimentalUnits.All(c => c.Responses[response].ResponseN != null);
            var hasUncertainty = experimentalUnits.All(c => c.Responses[response].ResponseUncertaintyUpper != null);
            var isMixture = experiment.ExperimentalUnits.Select(c => c.Doses.Where(d => d.Value > 0).Count()).Any(r => r > 1);

            var dataTable = experiment.toDataTable(response, isMixture);
            var responseIndex = dataTable.Columns[response.Code].Ordinal + 1;

            var substanceIndexes = substances.Select(r => dataTable.Columns[r.Code].Ordinal + 1).ToList();

            var mixtureIndex = dataTable.Columns["Substance"].Ordinal + 1;
            var covariateIndexes = covariates.Select(r => dataTable.Columns[r].Ordinal + 1).ToList();
            using (var logger = new FileLogger(Path.Combine(outputFolder, fitName + ".R"))) {
                using (var R = new LoggingRDotNetEngine(logger)) {
                    R.LoadLibrary($"proast{ProastVersion}");

                    R.EvaluateNoReturn("menu <- function(title, ...) {" +
                        "if (grepl('you entered a positive value for CES', title, fixed = TRUE) | grepl('you entered a negative value for CES', title, fixed = TRUE)) {" +
                            "return (1)" +
                        "}" +
                    "}");
                    R.EvaluateNoReturn("f.press.key.to.continue <-function(...) { cat(\"f.press.key.to.continue\n\"); return (invisible()) }");
                    R.EvaluateNoReturn("f.overlap <-function(...) { cat(\"f.overlap\n\"); return (invisible()) }");
                    R.EvaluateNoReturn($"rlang::env_unlock(env = asNamespace('proast{ProastVersion}'))");
                    R.EvaluateNoReturn($"rlang::env_binding_unlock(env = asNamespace('proast{ProastVersion}'))");
                    R.EvaluateNoReturn($"assign('f.press.key.to.continue', f.press.key.to.continue, envir = asNamespace('proast{ProastVersion}'))");
                    R.EvaluateNoReturn($"assign('f.overlap', f.overlap, envir = asNamespace('proast{ProastVersion}'))");
                    R.EvaluateNoReturn($"rlang::env_binding_lock(env = asNamespace('proast{ProastVersion}'))");
                    R.EvaluateNoReturn($"rlang::env_lock(asNamespace('proast{ProastVersion}'))");



                    R.SetSymbol("mydata", dataTable);
                    R.SetSymbol("ans.all", "f.ini(mydata)");
                    R.EvaluateNoReturn($"ans.all$plotprefix <- '{outputFolder.Replace("\\", "/")}/'");

                    R.SetSymbol("ans.all$CES", ces);
                    R.SetSymbol("ans.all$WAPP", true);

                    if (response.ResponseType == ResponseType.ContinuousMultiplicative) {
                        if (hasN) {
                            R.SetSymbol("ans.all$nans", dataTable.Columns["N"].Ordinal + 1);
                            R.SetSymbol("ans.all$dtype", 10);
                            if (hasSD) {
                                R.SetSymbol("ans.all$sd.se", 1);
                                R.SetSymbol("ans.all$sans", dataTable.Columns["SD"].Ordinal + 1);
                            } else if (hasCV) {
                                throw new NotImplementedException("Not implemented for CV");
                            } else {
                                throw new NotImplementedException("N specified, but no other data on variability found");
                            }
                        } else {
                            R.SetSymbol("ans.all$dtype", 1);
                        }
                    } else if (response.ResponseType == ResponseType.Quantal || response.ResponseType == ResponseType.QuantalGroup) {
                        R.SetSymbol("ans.all$dtype", 4);
                        R.SetSymbol("ans.all$nans", dataTable.Columns["N"].Ordinal + 1);
                        var riskType = getProastRiskTypeChoice(benchmarkResponseType);
                        R.SetSymbol("ans.all$ces.ans", riskType);
                        R.SetSymbol("ans.all$BMR", ces);
                    } else {
                        throw new Exception($"Proast DRM calculation not implemented for response type {response.ResponseType}");
                    }

                    R.SetSymbol("ans.all$xans", substanceIndexes);
                    R.SetSymbol("ans.all$yans", responseIndex);
                    if (substanceIndexes.Count > 1 && isMixture) {
                        R.SetSymbol("ans.all$displ.no", mixtureIndex);
                    }

                    var measurements = experiment.GetResponseMeasurements(response);
                    if (measurements.Any(r => r.ResponseValue <= 0)) {
                        R.SetSymbol("ans.all$auto.detlim", true);
                    }

                    if (substanceIndexes.Count == 1 || isMixture) {
                        // Set covariates
                        if (fitCovariates && covariates.Any()) {
                            R.SetSymbol("ans.all$covar.no", covariateIndexes);
                        }

                        // Run f.quick to get best candidate model
                        if (response.ResponseType == ResponseType.Quantal || response.ResponseType == ResponseType.QuantalGroup) {
                            R.EvaluateNoReturn("ans.all <- f.quick.cat(ans.all)");
                        } else {
                            R.EvaluateNoReturn("ans.all <- f.quick.con(ans.all)");
                        }
                        //choice 1: dose addition
                        //choice 2: parallel curves, mixtures are removed by proast gui
                        R.SetSymbol("ans.all$DA.ans", 1);
                        R.SetSymbol("ans.all$do.boot", false);
                    } else if (substanceIndexes.Count > 1 && !isMixture) {
                        // Select model 46
                        R.SetSymbol("ans.all$fct2.no", mixtureIndex);
                        R.SetSymbol("ans.all$displ.no", mixtureIndex);
                        R.SetSymbol("ans.all$DA.ans", 2);

                        // Get substance names and order first (which is how Proast processes substances)
                        // The reference level should be the index of the reference substance of the sorted
                        // dose column indexes. Apparently, proast sorts the column indexes internally in this
                        // case, so in order to get set the index of the reference substance correctly we need
                        // to do the same to set the index substance column correctly.

                        var referenceLevel = substances.OrderBy(r => dataTable.Columns[r.Code].ColumnName, System.StringComparer.OrdinalIgnoreCase).ToList().IndexOf(substances[0]) + 1;
                        R.SetSymbol("ans.all$ref.lev", referenceLevel);
                        R.SetSymbol("ans.all$model.ans", 46);
                        R.SetSymbol("ans.all$do.boot", false);
                    }

                    // If bootstrap: then set bootstrap parameters
                    if (numberOfBootstrapRuns != null) {
                        R.SetSymbol("ans.all$do.boot", true);
                        R.SetSymbol("ans.all$nruns", (int)numberOfBootstrapRuns);
                        R.EvaluateNoReturn("h.pr <- function() {}");
                        R.EvaluateNoReturn("ans.all$plot.ans <- -1");
                    }

                    // Run single model (if available)
                    var singleModelChoice = R.EvaluateInteger("ans.all$model.ans");
                    if (singleModelChoice > 0 && !isMixture) {
                        if (response.ResponseType == ResponseType.Quantal || response.ResponseType == ResponseType.QuantalGroup) {
                            R.EvaluateNoReturn("ans.all <- f.singlemodel.cat(ans.all)");
                        } else {
                            R.EvaluateNoReturn("ans.all <- f.singlemodel.con(ans.all)");
                        }
                    }

                    R.LoadLibrary("jsonlite");
                    var jsonFile = Path.Combine(outputFolder, "fit.json");
                    R.SetSymbol("json", "jsonlite::toJSON(ans.all, digits=I(6))");
                    R.SetSymbol("json", "jsonlite::prettify(json)");
                    R.EvaluateNoReturn($"write(json, file='{jsonFile.Replace("\\", "/")}')");

                    var reader = new ProastJsonDataReader();
                    var json = R.EvaluateString("json");
                    var doseResponseModelData = reader.Read(json, response.Code, experiment.Code);

                    var modelResults = new List<DoseResponseModel>();
                    foreach (var model in doseResponseModelData.DoseResponseModels) {
                        model.BenchmarkResponseType = benchmarkResponseType;
                        model.CriticalEffectSize = benchmarkResponse;
                        var bmds = doseResponseModelData.BenchmarkDoses.Where(r => r.idDoseResponseModel == model.idDoseResponseModel).ToList();
                        var bmdus = doseResponseModelData.BenchmarkDosesUncertain.Where(r => r.idDoseResponseModel == model.idDoseResponseModel).ToList();
                        var drmRecord = RawDoseResponseModelDataConverter.ToCompiled(model, bmds, bmdus, response, substances, experiment.DoseUnitString);
                        modelResults.Add(drmRecord);
                    }
                    return modelResults;
                }
            }
        }

        private static double getCes(double benchmarkResponse, BenchmarkResponseType benchmarkResponseType, ResponseType responseType) {
            switch (responseType) {
                case ResponseType.ContinuousMultiplicative:
                    return getCesContinuousMultiplicative(benchmarkResponse, benchmarkResponseType);
                case ResponseType.ContinuousAdditive:
                    throw new NotImplementedException();
                case ResponseType.Quantal:
                case ResponseType.QuantalGroup:
                    return getCesQuantal(benchmarkResponse, benchmarkResponseType);
                case ResponseType.Binary:
                case ResponseType.Count:
                case ResponseType.Ordinal:
                default:
                    throw new NotImplementedException();
            }
        }

        private static int getProastRiskTypeChoice(BenchmarkResponseType benchmarkResponseType) {
            int riskType;
            switch (benchmarkResponseType) {
                case BenchmarkResponseType.ExtraRisk:
                    riskType = 3;
                    break;
                case BenchmarkResponseType.AdditionalRisk:
                    riskType = 2;
                    break;
                case BenchmarkResponseType.Ed50:
                    riskType = 1;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return riskType;
        }

        private static double getCesContinuousMultiplicative(double benchmarkResponse, BenchmarkResponseType benchmarkResponseType) {
            switch (benchmarkResponseType) {
                case BenchmarkResponseType.Undefined:
                case BenchmarkResponseType.Factor:
                    return Math.Abs(benchmarkResponse - 1);
                case BenchmarkResponseType.Percentage:
                    return Math.Abs(benchmarkResponse / 100D);
                case BenchmarkResponseType.FractionChange:
                    return benchmarkResponse;
                case BenchmarkResponseType.PercentageChange:
                    return benchmarkResponse / 100D;
                case BenchmarkResponseType.Absolute:
                case BenchmarkResponseType.Difference:
                case BenchmarkResponseType.ExtraRisk:
                case BenchmarkResponseType.AdditionalRisk:
                case BenchmarkResponseType.Ed50:
                default:
                    throw new NotImplementedException($"Benchmark response type {benchmarkResponseType.GetDisplayName()} not implemented for continuous multiplicative responses");
            }
        }

        private static double getCesQuantal(double benchmarkResponse, BenchmarkResponseType benchmarkResponseType) {
            switch (benchmarkResponseType) {
                case BenchmarkResponseType.ExtraRisk:
                case BenchmarkResponseType.AdditionalRisk:
                    return benchmarkResponse;
                case BenchmarkResponseType.Ed50:
                    return 0.05;
                case BenchmarkResponseType.Factor:
                case BenchmarkResponseType.Percentage:
                case BenchmarkResponseType.FractionChange:
                case BenchmarkResponseType.PercentageChange:
                case BenchmarkResponseType.Absolute:
                case BenchmarkResponseType.Difference:
                case BenchmarkResponseType.Undefined:
                default:
                    throw new NotImplementedException($"Benchmark response type {benchmarkResponseType.GetDisplayName()} not implemented for quantal responses");
            }
        }
    }
}
