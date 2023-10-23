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
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.RelativePotencyFactors {

    [ActionType(ActionType.RelativePotencyFactors)]
    public class RelativePotencyFactorsActionCalculator : ActionCalculatorBase<RelativePotencyFactorsActionResult> {

        public RelativePotencyFactorsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionInputRequirements[ActionType.AOPNetworks].IsRequired = _project.EffectSettings.IncludeAopNetworks;
            _actionInputRequirements[ActionType.AOPNetworks].IsVisible = _project.EffectSettings.IncludeAopNetworks;
            _actionDataLinkRequirements[ScopingType.RelativePotencyFactors][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.RelativePotencyFactorsUncertain][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.RelativePotencyFactorsUncertain][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (_project.UncertaintyAnalysisSettings.ReSampleRPFs) {
                result.Add(UncertaintySource.RPFs);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new RelativePotencyFactorsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(
            ActionData data,
            SubsetManager subsetManager,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (data.SelectedEffect != null && subsetManager.AllRelativePotencyFactors.ContainsKey(data.SelectedEffect.Code)) {
                data.RawRelativePotencyFactors = subsetManager.AllRelativePotencyFactors[data.SelectedEffect.Code].ToDictionary(r => r.Compound);
            } else if (data.SelectedEffect == null) {
                //quick fix, only works when only one list of RPFs is available for one effect.
                if (subsetManager.AllRelativePotencyFactors.Count == 1) {
                    data.RawRelativePotencyFactors = subsetManager.AllRelativePotencyFactors
                        .First().Value
                        .ToDictionary(r => r.Compound);
                } else {
                    throw new Exception("No effect selected. Uncheck multiple effects analysis in Effects module.");
                }
            } else if (!subsetManager.AllRelativePotencyFactors.ContainsKey(data.SelectedEffect.Code)) {
                throw new Exception("No RPFs for selected effect available.");
            }
            data.ReferenceSubstance = subsetManager.ReferenceCompound;
            var correctedRpfs = computeCorrectedRelativePotencyFactors(data.ActiveSubstances, data.ReferenceSubstance, data.RawRelativePotencyFactors);
            data.CorrectedRelativePotencyFactors = correctedRpfs;
            checkRpfs(data);
            localProgress.Update(100);
        }

        private static void checkRpfs(ActionData data) {
            if (data.ActiveSubstances.Any(r => !data.CorrectedRelativePotencyFactors.TryGetValue(r, out var rpf) || double.IsNaN(rpf))) {
                var missingCount = data.CorrectedRelativePotencyFactors.Where(r => double.IsNaN(r.Value)).ToList();
                throw new Exception($"Missing relative potency factors for {missingCount.Count} substances");
            }
        }

        protected override RelativePotencyFactorsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new RelativePotencyFactorsActionResult();
            var referenceSubstance = data.AllCompounds?
                .FirstOrDefault(c => c.Code.Equals(_project.EffectSettings?.CodeReferenceCompound, StringComparison.OrdinalIgnoreCase));
            var correctedRpfs = computeRelativePotencyFactors(
                data.ActiveSubstances,
                referenceSubstance,
                data.HazardCharacterisationModelsCollections
            );

            result.ReferenceCompound = referenceSubstance;
            result.CorrectedRelativePotencyFactors = correctedRpfs;
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, RelativePotencyFactorsActionResult result) {
            if (result.ReferenceCompound != null) {
                data.ReferenceSubstance = result.ReferenceCompound;
            }
            if (result.CorrectedRelativePotencyFactors != null) {
                data.CorrectedRelativePotencyFactors = result.CorrectedRelativePotencyFactors;
            }
            if (_project.ActionType != ActionType.RelativePotencyFactors) {
                checkRpfs(data);
            }
        }

        protected override void summarizeActionResult(RelativePotencyFactorsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new RelativePotencyFactorsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (factorialSet.Contains(UncertaintySource.RPFs) && data.RawRelativePotencyFactors != null) {
                var compounds = data.ActiveSubstances;
                var reference = compounds.First(r => r.Code == _project.EffectSettings.CodeReferenceCompound);
                var rawRelativePotencyFactors = data.RawRelativePotencyFactors;
                var correctedRpfs = resampleRelativePotencyFactors(
                    compounds,
                    reference,
                    rawRelativePotencyFactors,
                    uncertaintySourceGenerators[UncertaintySource.RPFs]
                );
                data.CorrectedRelativePotencyFactors = correctedRpfs;
            }
            localProgress.Update(100);
        }

        protected override RelativePotencyFactorsActionResult runUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new RelativePotencyFactorsActionResult();
            var correctedRpfs = computeRelativePotencyFactors(
                data.ActiveSubstances,
                data.ReferenceSubstance,
                data.HazardCharacterisationModelsCollections
            );
            result.CorrectedRelativePotencyFactors = correctedRpfs;
            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, RelativePotencyFactorsActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new RelativePotencyFactorsSummarizer();
            summarizer.SummarizeUncertain(_project, actionResult, data, header);
            localProgress.Update(100);
        }

        protected override void updateSimulationDataUncertain(ActionData data, RelativePotencyFactorsActionResult result) {
            updateSimulationData(data, result);
        }

        private static Dictionary<Compound, double> computeCorrectedRelativePotencyFactors(
            ICollection<Compound> substances,
            Compound reference,
            IDictionary<Compound, RelativePotencyFactor> rawRelativePotencyFactors
        ) {
            if (!rawRelativePotencyFactors.TryGetValue(reference, out var refRpf)) {
                throw new Exception("No RPF defined for reference substance.");
            } else if (double.IsNaN(refRpf.RPF)) {
                throw new Exception("Incorrect value for RPF of reference substance.");
            }
            var referenceRpf = refRpf.RPF;
            var referenceRpfCorrectionFactor = 1D / referenceRpf;
            var result = new Dictionary<Compound, double>();
            foreach (var substance in substances) {
                if (substance == reference) {
                    result[substance] = 1D;
                } else if (rawRelativePotencyFactors.TryGetValue(substance, out var rawRpf)) {
                    result[substance] = referenceRpfCorrectionFactor * rawRpf.RPF;
                } else {
                    throw new Exception($"No RPF record found for substance [{substance.Name} ({substance.Code})].");
                }
            }
            return result;
        }

        private static Dictionary<Compound, double> computeRelativePotencyFactors(
            ICollection<Compound> substances,
            Compound referenceSubstance,
            ICollection<HazardCharacterisationModelCompoundsCollection> hazardCharacterisationModelsCollections
        ) {
            var result = new Dictionary<Compound, double>();

            if (referenceSubstance != null && !substances.Contains(referenceSubstance)) {
                throw new Exception("Reference substance is not part of the active substances.");
            } else if (referenceSubstance == null) {
                //For MixtureSelection purposes, it is irrelevant which substance is the reference substance
                referenceSubstance = substances.First();
            }
            var hazardCharacterisations = hazardCharacterisationModelsCollections?
                .SelectMany(c => c.HazardCharacterisationModels.Select(r => r.Key))
                .ToHashSet();

            var hazardCharacterisationsModels = hazardCharacterisationModelsCollections?
                .SelectMany(c => c.HazardCharacterisationModels.Select(m => new {
                    c.TargetUnit,
                    Substance = m.Key,
                    Model = m.Value
                }))
                .ToDictionary(r => (r.Substance, r.TargetUnit.Target), r => r.Model);

            ExposureTarget target = null;
            var targets = hazardCharacterisationsModels?.Select(c => c.Key.Target).Distinct();
            if (targets != null && targets.Count() > 1) {
                throw new Exception("RPF weighted risk assessement not implemented for multiple targets.");
            } else {
                target = targets?.FirstOrDefault();
            }
            var referenceHazardDose = (double)(hazardCharacterisations != null && hazardCharacterisations.Contains(referenceSubstance)
                ? hazardCharacterisationsModels?[(referenceSubstance, target)].Value
                : double.NaN);

            foreach (var substance in substances) {
                if (!double.IsNaN(referenceHazardDose) && hazardCharacterisations.Contains(substance)) {
                    var hazardDose = (double)hazardCharacterisationsModels?[(substance, target)].Value;
                    var correctedRpf = referenceHazardDose / hazardDose;
                    result[substance] = correctedRpf;
                } else {
                    result[substance] = double.NaN;
                }
            }
            return result;
        }

        private static Dictionary<Compound, double> resampleRelativePotencyFactors(
            ICollection<Compound> substances,
            Compound reference,
            IDictionary<Compound, RelativePotencyFactor> rawRelativePotencyFactors,
            IRandom generator
        ) {
            var result = new Dictionary<Compound, double>();

            var rpfUncertaintySets = rawRelativePotencyFactors
                .SelectMany(
                    r => r.Value.RelativePotencyFactorsUncertains,
                    (rpf, rpfu) => (rpf, rpfu)
                )
                .GroupBy(r => (r.rpfu.idUncertaintySet))
                .ToList();

            if (rpfUncertaintySets.Any()) {
                var ix = generator.Next(0, rpfUncertaintySets.Count);

                // Draw uncertainty set
                var uncertaintySet = rpfUncertaintySets[ix];
                var uncertaintyRpfs = uncertaintySet.ToDictionary(r => r.rpf.Key, r => r.rpfu.RPF);

                // Get RPF of reference substance
                if (!uncertaintyRpfs.ContainsKey(reference)) {
                    throw new Exception($"Missing RPF for reference substance [{reference.Name}({reference.Code})] in RPF uncertainty set [{uncertaintySet.Key}].");
                } else if (substances.Any(r => !uncertaintyRpfs.ContainsKey(r))) {
                    var missingSubstances = substances.Where(r => !uncertaintyRpfs.ContainsKey(r)).ToList();
                    var missingSubstancesString = missingSubstances.Count > 3
                        ? $"{missingSubstances.Count} substances"
                        : string.Join(", ", missingSubstances.Select(r => $"[{r.Name} ({r.Code})]"));
                    throw new Exception($"Missing RPF for {missingSubstancesString} in RPF uncertainty set [{uncertaintySet.Key}].");
                }

                // Get RPFs for other substances
                var referenceRpfCorrection = 1D / uncertaintyRpfs[reference];
                foreach (var compound in substances) {
                    if (compound == reference) {
                        result[reference] = 1D;
                    } else {
                        result[compound] = referenceRpfCorrection * uncertaintyRpfs[compound];
                    }
                }

                return result;
            } else {
                return computeCorrectedRelativePotencyFactors(
                    substances, 
                    reference, 
                    rawRelativePotencyFactors
                );
            }
        }

        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, RelativePotencyFactorsActionResult result) {
            if (data.SelectedEffect != null) {
                var rawDataConverter = new RawRelativePotencyFactorDataConverter();
                var rawData = rawDataConverter.ToRaw(data.SelectedEffect, data.CorrectedRelativePotencyFactors);
                rawDataWriter.Set(rawData);
            }
        }
    }
}
