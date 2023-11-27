using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.Data.Management.RawDataObjectConverters;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.DoseResponseModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.DoseResponseModels {

    [ActionType(ActionType.DoseResponseModels)]
    public class DoseResponseModelsActionCalculator : ActionCalculatorBase<DoseResponseModelsActionResult> {

        public DoseResponseModelsActionCalculator(ProjectDto project) : base(project) {
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (_project.UncertaintyAnalysisSettings.ReSampleRPFs) {
                result.Add(UncertaintySource.DoseResponseModels);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new DoseResponseModelsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }
        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.DoseResponseModels][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionInputRequirements[ActionType.DoseResponseData].IsRequired = ShouldCompute;
            _actionInputRequirements[ActionType.EffectRepresentations].IsRequired = false;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.DoseResponseModels = subsetManager.AllDoseResponseModels;
        }

        protected override DoseResponseModelsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var models = new List<DoseResponseModel>();

            if (!data.SelectedResponseExperiments?.Any() ?? true) {
                throw new Exception("No dose response experiments available for fitting dose response models.");
            }

            var settings = new DoseResponseModelsModuleSettings(_project);
            var referenceSubstance = data.AllCompounds?
                .FirstOrDefault(c => c.Code.Equals(settings.CodeReferenceSubstance, StringComparison.OrdinalIgnoreCase));

            foreach (var experiment in data.SelectedResponseExperiments) {
                var responses = experiment.Responses.Where(r => data.Responses.ContainsKey(r.Code));
                foreach (var response in responses) {
                    var proastDrmCalculator = new ProastDoseResponseModelCalculator(null);
                    var effectRepresentation = data.FocalEffectRepresentations?.FirstOrDefault(r => r.Response == response);
                    var defaultBenchmarkResponse = (response.ResponseType == ResponseType.Quantal || response.ResponseType == ResponseType.QuantalGroup) ? 0.05 : 0.95;
                    var defaultBenchmarkResponseType = (response.ResponseType == ResponseType.Quantal || response.ResponseType == ResponseType.QuantalGroup) ? BenchmarkResponseType.ExtraRisk : BenchmarkResponseType.Factor;
                    var benchmarkResponseType = (effectRepresentation?.HasBenchmarkResponse() ?? false)
                        ? effectRepresentation.BenchmarkResponseType
                        : defaultBenchmarkResponseType;
                    var benchmarkResponse = (effectRepresentation?.HasBenchmarkResponseValue() ?? false)
                        ? effectRepresentation.BenchmarkResponse.Value
                        : defaultBenchmarkResponse;
                    var modelResult = proastDrmCalculator
                        .TryCompute(
                            experiment,
                            response,
                            benchmarkResponse,
                            benchmarkResponseType,
                            experiment.Covariates,
                            referenceSubstance,
                            _project.UncertaintyAnalysisSettings.DoUncertaintyAnalysis
                                && _project.DoseResponseModelsSettings.CalculateParametricConfidenceInterval
                                ? null
                                : _project.UncertaintyAnalysisSettings.NumberOfResampleCycles,
                            false
                        );
                    models.AddRange(modelResult);
                }
            }

            // Hier moet afgevangen worden of er data of models aanwezig zijn. Nu ondanks dat voor calculate 
            // gekozen wordt zijn er response models aanwezig als ze al een keer geladen zijn
            var doseResponseModels = models.Any() ? models : (data.DoseResponseModels?.ToList() ?? new List<DoseResponseModel>());
            var result = new DoseResponseModelsActionResult() {
                ReferenceSubstance = referenceSubstance,
                DoseResponseModels = doseResponseModels,
            };

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(DoseResponseModelsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing dose response models", 0);
            var summarizer = new DoseResponseModelsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void updateSimulationData(ActionData data, DoseResponseModelsActionResult result) {
            if (result.DoseResponseModels != null) {
                data.DoseResponseModels = result.DoseResponseModels;
            }
        }

        protected override DoseResponseModelsActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new DoseResponseModelsActionResult();
            if (factorialSet.Contains(UncertaintySource.DoseResponseModels)) {
                result.DoseResponseModels = resampleBenchmarkDoses(
                    data.DoseResponseModels,
                    uncertaintySourceGenerators[UncertaintySource.DoseResponseModels],
                    _project.DoseResponseModelsSettings.CalculateParametricConfidenceInterval
                );
            } else {
                result.DoseResponseModels = data.DoseResponseModels.ToList();
            }
            localProgress.Update(100);
            return result;
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (factorialSet.Contains(UncertaintySource.DoseResponseModels) && data.DoseResponseModels != null) {
                data.DoseResponseModels = resampleBenchmarkDoses(
                    data.DoseResponseModels,
                    uncertaintySourceGenerators[UncertaintySource.DoseResponseModels],
                    _project.DoseResponseModelsSettings.CalculateParametricConfidenceInterval
                );
            }
            localProgress.Update(100);
        }

        protected override void writeOutputData(
            IRawDataWriter rawDataWriter,
            ActionData data,
            DoseResponseModelsActionResult result
        ) {
            var rawDataConverter = new RawDoseResponseModelDataConverter();
            var rawData = rawDataConverter.ToRaw(data.DoseResponseModels);
            rawDataWriter.Set(rawData);
        }

        private static ICollection<DoseResponseModel> resampleBenchmarkDoses(
            ICollection<DoseResponseModel> doseResponseModels,
            IRandom generator,
            bool calculateParametricConfidenceInterval,
            DistributionType distribution = DistributionType.LogNormal
        ) {
            var drms = new List<DoseResponseModel>();
            foreach (var drm in doseResponseModels) {
                var benchmarkDoses = new List<DoseResponseModelBenchmarkDose>();
                var hasBmdUncertains = drm.DoseResponseModelBenchmarkDoses?
                    .Any(r => r.Value.DoseResponseModelBenchmarkDoseUncertains?.Any() ?? false) ?? false;
                if (calculateParametricConfidenceInterval) {
                    // No uncertainty sets for this record
                    foreach (var model in drm.DoseResponseModelBenchmarkDoses.Values) {
                        var clone = model.CreateBootstrapRecord(
                            model.BenchmarkDose,
                            model.Rpf
                        );
                        benchmarkDoses.Add(clone);
                    }
                } else {
                    if (hasBmdUncertains) {
                        // Empirical bootstrap: draw from uncertainty sets
                        var uncertaintySets = drm.DoseResponseModelBenchmarkDoses
                            .SelectMany(
                                r => r.Value.DoseResponseModelBenchmarkDoseUncertains,
                                (bmd, bmdu) => (Bmd: bmd, Bmdu: bmdu))
                            .GroupBy(r => r.Bmdu.IdUncertaintySet);
                        if (uncertaintySets?.Any() ?? false) {
                            // Uncertainty sets are available, randomly draw bootstrap record
                            var ix = generator.Next(uncertaintySets.Count());
                            var drawn = uncertaintySets.ElementAt(ix);
                            foreach (var record in drawn) {
                                var clone = record.Bmd.Value.CreateBootstrapRecord(
                                    record.Bmdu.BenchmarkDose,
                                    record.Bmdu.Rpf
                                );
                                benchmarkDoses.Add(clone);
                            }
                        }
                    } else {
                        // Parametric bootstrap of BMD
                        foreach (var model in drm.DoseResponseModelBenchmarkDoses.Values) {
                            var lower = double.IsNaN(model.BenchmarkDoseLower) ? model.BenchmarkDose : model.BenchmarkDoseLower;
                            var upper = double.IsNaN(model.BenchmarkDoseUpper) ? model.BenchmarkDose : model.BenchmarkDoseUpper;
                            if (distribution == DistributionType.LogNormal) {
                                var sigmaU = Math.Log(upper / model.BenchmarkDose) / 1.645;
                                var sigmaL = Math.Log(model.BenchmarkDose / lower) / 1.645;
                                var sigma = sigmaU > sigmaL ? sigmaU : sigmaL;
                                var draw = generator.NextDouble();
                                var drawnBmd = Math.Exp(NormalDistribution.InvCDF(0, 1, draw) * sigma + Math.Log(model.BenchmarkDose));
                                benchmarkDoses.Add(model.CreateBootstrapRecord(drawnBmd, double.NaN));
                            } else if (distribution == DistributionType.Uniform) {
                                var drawn = generator.NextDouble(lower, upper);
                                benchmarkDoses.Add(model.CreateBootstrapRecord(drawn, double.NaN));
                            }
                        }
                    }
                }

                drms.Add(drm.Clone(benchmarkDoses));
            }
            return drms;
        }
    }
}
